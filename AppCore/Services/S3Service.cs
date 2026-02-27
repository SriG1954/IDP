using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AppCore.EntityModels;
using AppCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;

namespace AppCore.Services
{
    public class S3Service :IS3Service
    {
        private readonly IConfiguration _cfg;
        private readonly IIDPAuditLogRepository _log;
        private readonly ILogger<S3Service> _logger;
        private readonly IAssumedRoleClientFactory _factory;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private string tempKey = string.Empty;
        private string environment = string.Empty;
        private IAmazonS3? _s3Client;
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(2);

        public S3Service(IConfiguration cfg, IIDPAuditLogRepository log, ILogger<S3Service> logger, IAssumedRoleClientFactory factory)
        {
            _cfg = cfg;
            _log = log;
            _logger = logger;
            _factory = factory;
            environment = _cfg["AWS:Environment"]!;
        }

        public async Task<IAmazonS3> GetClientAsync(CancellationToken ct = default)
        {
            try
            {
                _s3Client = await _factory.GetS3ClientAsync(ct);
                return _s3Client;
            }
            catch (OperationCanceledException ex)
            {
                await _log.AddLogAsync(0, 0, $"GetClientAsync Operation cancelled: {ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }
            catch (Exception ex)
            {
                await _log.AddLogAsync(0, 0, $"GetClientAsync Error GetClientAsync: {ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw new Exception($"Error GetClientAsync: {ex.Message}");
            }
        }

        public async Task<XElement> GetObjectMetadataAsync(string bucketName, string s3Key, CancellationToken cancellationToken)
        {
            // check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            XElement root = new XElement("root");

            try
            {
                _s3Client = await GetClientAsync(cancellationToken);
                var head = await _s3Client.GetObjectMetadataAsync(bucketName, s3Key, cancellationToken);
                long originalSize = head.ContentLength;
                string oldETag = head.ETag?.Trim('"')!;

                root.Add(new XElement("BucketName", bucketName));
                root.Add(new XElement("Key", s3Key));
                root.Add(new XElement("OriginalSize", originalSize.ToString()));
                root.Add(new XElement("ETag", oldETag));


            }
            catch (OperationCanceledException ex)
            {
                await _log.AddLogAsync(0, 0, $"Operation cancelled: {ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }
            catch (AmazonS3Exception s3Ex)
            {
                await _log.AddLogAsync(0, 0, $"S3 error: {s3Ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }
            catch (Exception ex)
            {
                await _log.AddLogAsync(0, 0, $"General error: {ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }

            return root;
        }

        public async Task PutObjectAsync(string bucketName, string s3Key, byte[] data, CancellationToken cancellationToken)
        {
            // check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var ms = new MemoryStream(data);
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    InputStream = ms
                };

                _s3Client = await GetClientAsync(cancellationToken);
                await _s3Client.PutObjectAsync(request, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                await _log.AddLogAsync(0, 0, $"Operation cancelled: {ex.Message}", AuditLogLevel.Error, AuditEventType.AWSContext);
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if bucket exists: {ex.Message}");
            }
        }

        public async Task PutObjectV1Async(string bucketName, string s3Key, string filePath, CancellationToken cancellationToken)
        {
            InitiateMultipartUploadResponse initiate = null!;
            Stopwatch swTotal = Stopwatch.StartNew();

            try
            {
                Console.WriteLine($"Starting multipart upload. Bucket={bucketName}, Key={s3Key}");

                cancellationToken.ThrowIfCancellationRequested();
                _s3Client = await GetClientAsync(cancellationToken);

                var fileInfo = new FileInfo(filePath);
                Console.WriteLine($"File={filePath}, Size={fileInfo.Length / (1024 * 1024)} MB");

                initiate = await _s3Client.InitiateMultipartUploadAsync(
                    new InitiateMultipartUploadRequest
                    {
                        BucketName = bucketName,
                        Key = s3Key
                    },
                    cancellationToken);

                Console.WriteLine($"Multipart upload initiated. UploadId={initiate.UploadId}");

                const long partSize = 15L * 1024 * 1024; // 15 MB
                //const long partSize = 100L * 1024 * 1024; // 100 MB
                //const long partSize = 1L * 1024 * 1024 * 1024; // 1 GB
                var partETags = new List<PartETag>();

                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 1024 * 1024,
                    useAsync: true);

                long fileLength = fileStream.Length;
                long position = 0;
                int partNumber = 1;

                while (position < fileLength)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    long currentPartSize = Math.Min(partSize, fileLength - position);
                    fileStream.Position = position;

                    Console.WriteLine(
                        $"Uploading part {partNumber} " +
                        $"({currentPartSize / (1024 * 1024)} MB) " +
                        $"at position {position / (1024 * 1024)} MB");

                    Stopwatch swPart = Stopwatch.StartNew();

                    try
                    {
                        var response = await _s3Client.UploadPartAsync(
                            new UploadPartRequest
                            {
                                BucketName = bucketName,
                                Key = s3Key,
                                UploadId = initiate.UploadId,
                                PartNumber = partNumber,
                                InputStream = fileStream,
                                PartSize = currentPartSize
                            },
                            cancellationToken);

                        swPart.Stop();

                        partETags.Add(new PartETag(partNumber, response.ETag));

                        Console.WriteLine(
                            $"Part {partNumber} uploaded successfully. " +
                            $"ETag={response.ETag}, " +
                            $"Time={swPart.Elapsed.TotalSeconds:F1}s");

                        position += currentPartSize;
                        partNumber++;
                    }
                    catch (AmazonS3Exception s3Ex)
                    {
                        Console.WriteLine(
                            $"S3 error on part {partNumber}. " +
                            $"Status={s3Ex.StatusCode}, " +
                            $"Code={s3Ex.ErrorCode}, " +
                            $"Message={s3Ex.Message}");

                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error on part {partNumber}: {ex}");
                        throw;
                    }
                }

                Console.WriteLine("All parts uploaded. Completing multipart upload.");

                await _s3Client.CompleteMultipartUploadAsync(
                    new CompleteMultipartUploadRequest
                    {
                        BucketName = bucketName,
                        Key = s3Key,
                        UploadId = initiate.UploadId,
                        PartETags = partETags
                    },
                    cancellationToken);

                swTotal.Stop();
                Console.WriteLine($"Multipart upload completed successfully in {swTotal.Elapsed.TotalSeconds:F1}s");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Multipart upload cancelled by request.");
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine(
                    $"AWS S3 failure. " +
                    $"Status={ex.StatusCode}, " +
                    $"Code={ex.ErrorCode}, " +
                    $"Message={ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception during multipart upload: {ex}");
                throw;
            }
            finally
            {
                if (initiate != null)
                {
                    try
                    {
                        Console.WriteLine("Ensuring multipart upload cleanup.");

                        await _s3Client!.AbortMultipartUploadAsync(
                            new AbortMultipartUploadRequest
                            {
                                BucketName = bucketName,
                                Key = s3Key,
                                UploadId = initiate.UploadId
                            });

                        Console.WriteLine("Multipart upload aborted (if not already completed).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to abort multipart upload: {ex.Message}");
                    }
                }
            }
        }


        public async Task<string> DownloadPdfAsync(string bucket, string key, CancellationToken cancellationToken)
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
            _s3Client = await GetClientAsync(cancellationToken);

            using var resp = await _s3Client.GetObjectAsync(new GetObjectRequest { BucketName = bucket, Key = key }, cancellationToken);
            await using var fs = File.Create(tmp);
            await resp.ResponseStream.CopyToAsync(fs, cancellationToken);
            await _log.AddLogAsync(0, 0, $"Downloaded S3 {bucket}/{key} to {tmp}", AuditLogLevel.Info, AuditEventType.S3Context);

            return tmp;
        }

    }

}
