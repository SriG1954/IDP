using Amazon;
using Amazon.Textract;
using Amazon.Textract.Model;
using AppCore.EntityModels;
using AppCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AppCore.Services
{
    public class TextractService
    {
        private readonly IConfiguration _cfg;
        private readonly IIDPAuditLogRepository _audit;
        private readonly ILogger<TextractService> _logger;
        private readonly IAssumedRoleClientFactory _factory;
        private readonly IS3Service _s3Service;

        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private IAmazonTextract? _textractClient;
        private DateTime _expiryUtc;

        private readonly string _environment;
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(2);

        // Async-supported file types for StartDocumentTextDetection.
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".tiff", ".tif", ".jpg", ".jpeg", ".png" };

        public TextractService(
            IConfiguration cfg,
            IIDPAuditLogRepository audit,
            ILogger<TextractService> logger,
            IAssumedRoleClientFactory factory,
            IS3Service s3Service)
        {
            _cfg = cfg;
            _audit = audit;
            _logger = logger;
            _factory = factory;
            _s3Service = s3Service;

            _environment = _cfg["AWS:Environment"] ?? "DEV";
        }

        /// <summary>
        /// Returns a cached IAmazonTextract assumed-role client, refreshed before STS expiry.
        /// Uses local default chain if AWS:BypassAssumeRoleInDev is true.
        /// </summary>
        public async Task<IAmazonTextract> GetClientAsync(CancellationToken ct = default)
        {
            try
            {
                // Optional dev bypass (use local profile/role etc.)
                var bypassDev = bool.TryParse(_cfg["AWS:BypassAssumeRoleInDev"], out var b) && b;
                if (bypassDev && string.Equals(_environment, "DEV", StringComparison.OrdinalIgnoreCase))
                {
                    _textractClient ??= new AmazonTextractClient(ResolveRegion());
                    return _textractClient;
                }

                if (_textractClient is not null && DateTime.UtcNow < _expiryUtc - RefreshBuffer)
                    return _textractClient;

                await _refreshLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    if (_textractClient is not null && DateTime.UtcNow < _expiryUtc - RefreshBuffer)
                        return _textractClient;

                    var desc = await _factory.CreateTextractClientAsync(ct).ConfigureAwait(false);
                    _textractClient = desc.Client;
                    _expiryUtc = desc.ExpiryUtc;

                    _logger.LogInformation("Textract client refreshed; expires {ExpiryUtc:u}", _expiryUtc);
                    return _textractClient!;
                }
                finally
                {
                    _refreshLock.Release();
                }
            }
            catch (OperationCanceledException)
            {
                await SafeAuditAsync("GetClientAsync cancelled", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }
            catch (Exception ex)
            {
                await SafeAuditAsync($"GetClientAsync error: {ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> DetectLinesAsync(string imagePath, CancellationToken ct)
        {
            var bytes = await File.ReadAllBytesAsync(imagePath, ct);
            _textractClient = await GetClientAsync(ct);
            var resp = await _textractClient.DetectDocumentTextAsync(new DetectDocumentTextRequest
            {
                Document = new Document { Bytes = new MemoryStream(bytes) }
            }, ct);

            var filtered = new List<object>();
            foreach (var b in resp.Blocks)
            {
                if (b.BlockType == BlockType.LINE)
                {
                    filtered.Add(new
                    {
                        BlockType = b.BlockType.Value,
                        Confidence = b.Confidence,
                        Text = b.Text,
                        Geometry = b.Geometry
                    });
                }
            }
            return new Dictionary<string, object> { ["Blocks"] = filtered };
        }

        /// <summary>
        /// Starts async OCR for a batch of OCR documents.
        /// Uses bounded concurrency and polling for completion.
        /// </summary>
        public async Task SendForOCRBatchAsync(List<Ocrdocument> ocrDocuments, CancellationToken ct)
        {
            if (ocrDocuments is null || ocrDocuments.Count == 0) return;

            // Degree of parallelism: keep modest to avoid throttling
            const int maxParallel = 3;
            using var concurrency = new SemaphoreSlim(maxParallel, maxParallel);

            var tasks = ocrDocuments
                .Where(d => IsSupportedExt(d.DocumentPath))
                .Select(async document =>
                {
                    await concurrency.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        // 1) Upload and start job
                        var jobId = await StartTextDetectionAsync(document, ct).ConfigureAwait(false);

                        // 2) Poll until completion
                        var final = await PollJobUntilCompleteAsync(jobId, ct).ConfigureAwait(false);
                        if (final.JobStatus == JobStatus.SUCCEEDED)
                        {
                            var full = await FetchAllTextBlocksAsync(jobId, ct).ConfigureAwait(false);
                            // TODO: Persist results:
                            // await _persistBulkCopy.BulkCopyMultiPageToDB(full, document.Id, document.DocumentPath, document.DocumentId);

                            await SafeAuditAsync(
                                $"OCR completed - JobId {jobId} - {document.DocumentPath} (DocId: {document.Id})",
                                AuditLogLevel.Info, AuditEventType.TextractContext);
                        }
                        else
                        {
                            await SafeAuditAsync(
                                $"OCR failed - JobId {jobId} - {document.DocumentPath} (DocId: {document.Id}) - {final.StatusMessage}",
                                AuditLogLevel.Error, AuditEventType.TextractContext);
                        }
                    }
                    finally
                    {
                        concurrency.Release();
                    }
                });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads the local file to S3 (streamed) and starts StartDocumentTextDetection.
        /// </summary>
        public async Task<string> StartTextDetectionAsync(Ocrdocument doc, CancellationToken ct)
        {
            var bucket = _cfg["AWS:Textract:Upload:Bucket"] ?? throw new InvalidOperationException("AWS:Textract:Upload:Bucket is required");
            var prefix = _cfg["AWS:Textract:Upload:Prefix"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(doc.DocumentPath) || !File.Exists(doc.DocumentPath))
                throw new FileNotFoundException($"File not found: {doc.DocumentPath}");

            var key = GenerateS3Key(prefix, doc.DocumentPath, doc.Id, doc.Id);


            // Read the entire file into memory as a byte[]
            byte[] bytes = await File.ReadAllBytesAsync(doc.DocumentPath, ct);

            // Upload using your IS3Service overload that accepts byte[]
            await _s3Service
                .PutObjectAsync(bucket, key, bytes, ct)
                .ConfigureAwait(false);

            await SafeAuditAsync(
                $"Uploaded for Textract: s3://{bucket}/{key} (Local: {doc.DocumentPath}, DocId: {doc.Id})",
                AuditLogLevel.Info, AuditEventType.TextractContext);

            var request = new StartDocumentTextDetectionRequest
            {
                DocumentLocation = new DocumentLocation
                {
                    S3Object = new Amazon.Textract.Model.S3Object { Bucket = bucket, Name = key }
                },
                // Idempotency: prevents duplicate jobs if retried
                ClientRequestToken = BuildClientRequestToken(bucket, key, doc.Id, doc.Id),
                JobTag = $"doc-{doc.Id}",
            };

            var client = await GetClientAsync(ct).ConfigureAwait(false);
            var response = await client.StartDocumentTextDetectionAsync(request, ct).ConfigureAwait(false);

            await SafeAuditAsync(
                $"Textract job started. JobId: {response.JobId}, s3://{bucket}/{key}",
                AuditLogLevel.Info, AuditEventType.AWSContext);

            return response.JobId;
        }

        /// <summary>
        /// Polls GetDocumentTextDetection until SUCCEEDED/FAILED or timeout, using exponential backoff with jitter.
        /// </summary>
        public async Task<GetDocumentTextDetectionResponse> PollJobUntilCompleteAsync(string jobId, CancellationToken ct)
        {
            var client = await GetClientAsync(ct).ConfigureAwait(false);

            var pollCfg = _cfg.GetSection("AWS:Textract:Poll");
            var maxWait = TimeSpan.FromMinutes(pollCfg.GetValue("MaxWaitMinutes", 30));
            var baseDelayMs = pollCfg.GetValue("BaseDelayMs", 1500);
            var maxDelayMs = pollCfg.GetValue("MaxDelayMs", 15000);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var rng = RandomNumberGenerator.Create();

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var res = await client.GetDocumentTextDetectionAsync(new GetDocumentTextDetectionRequest
                {
                    JobId = jobId,
                    MaxResults = 1000
                }, ct).ConfigureAwait(false);

                _logger.LogInformation("Textract job status: {Status} (JobId: {JobId})", res.JobStatus, jobId);

                if (res.JobStatus == JobStatus.SUCCEEDED || res.JobStatus == JobStatus.FAILED || res.JobStatus == JobStatus.PARTIAL_SUCCESS)
                {
                    return res;
                }

                if (sw.Elapsed > maxWait)
                    throw new TimeoutException($"Textract job {jobId} did not complete within {maxWait}.");

                // decorrelated jitter backoff
                var delay = JitterDelay(baseDelayMs, maxDelayMs, rng);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves ALL Blocks across pages using NextToken pagination.
        /// </summary>
        public async Task<GetDocumentTextDetectionResponse> FetchAllTextBlocksAsync(string jobId, CancellationToken ct)
        {
            var client = await GetClientAsync(ct).ConfigureAwait(false);

            var allBlocks = new List<Block>(capacity: 1024);
            string? nextToken = null;
            GetDocumentTextDetectionResponse? first = null;

            do
            {
                var resp = await client.GetDocumentTextDetectionAsync(new GetDocumentTextDetectionRequest
                {
                    JobId = jobId,
                    NextToken = nextToken,
                    MaxResults = 1000
                }, ct).ConfigureAwait(false);

                first ??= resp;
                if (resp.Blocks is { Count: > 0 })
                    allBlocks.AddRange(resp.Blocks);

                nextToken = resp.NextToken;
            }
            while (!string.IsNullOrEmpty(nextToken));

            if (first is null) throw new InvalidOperationException("No response received from Textract.");

            first.Blocks = allBlocks;
            return first;
        }

        // --------------------------- Helpers ---------------------------

        private RegionEndpoint ResolveRegion()
        {
            var regionName = _cfg["AWS:Region"];
            return !string.IsNullOrWhiteSpace(regionName)
                ? RegionEndpoint.GetBySystemName(regionName)
                : RegionEndpoint.APSoutheast2;
        }

        private static TimeSpan JitterDelay(int baseMs, int maxMs, RandomNumberGenerator rng)
        {
            // Full jitter between baseMs and maxMs
            var buf = new byte[4];
            rng.GetBytes(buf);
            var val = Math.Abs(BitConverter.ToInt32(buf, 0));
            var jitter = baseMs + (val % Math.Max(1, (maxMs - baseMs)));
            return TimeSpan.FromMilliseconds(jitter);
        }

        private static bool IsSupportedExt(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var ext = Path.GetExtension(path);
            return AllowedExtensions.Contains(ext);
        }

        private static string GenerateS3Key(string prefix, string localPath, long docId, long? documentId)
        {
            var fileName = Path.GetFileName(localPath);
            var safeName = fileName.Replace(' ', '_');
            var ts = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            var idPart = documentId.HasValue ? $"doc-{documentId}-" : string.Empty;
            return $"{prefix}{ts}-{idPart}{docId}-{safeName}";
        }

        private static string BuildClientRequestToken(string bucket, string key, long docId, long? documentId)
        {
            // Must be <= 64 chars, [A-Za-z0-9-_]. Hash to compact + include a small readable prefix.
            var baseStr = $"{bucket}/{key}:{docId}:{documentId}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(baseStr)));
            return $"job-{hash[..20]}"; // 24 chars
        }

        private async Task SafeAuditAsync(string message, AuditLogLevel level, AuditEventType evtType)
        {
            try
            {
                await _audit.AddLogAsync(0, 0, message, level, evtType);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit write failed");
            }
        }
    }
}