using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCore.Services
{
    public class BatchService : IBatchService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<BatchService>? _logger;
        private string csvPath = @"C:\IDP\IDPTestCases.csv";
        private string folderPath = @"C:\Temp\ARSDownloads";

        public BatchService(AppDbContext db, ILogger<BatchService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> ExecuteAsync(CancellationToken ct)
        {
            bool success = false;
            if (!File.Exists(csvPath))
                throw new FileNotFoundException("CSV file not found.", csvPath);

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            var requestIds = await File.ReadAllLinesAsync(csvPath, ct);

            foreach (var line in requestIds)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var columns = line.Split(',');

                if (columns.Length == 0)
                    continue;

                var requestId = columns[0].Trim();

                if (string.IsNullOrWhiteSpace(requestId))
                    continue;

                _logger?.LogInformation("Processing RequestId: {RequestId}", requestId);

                var matchingFiles = Directory
                    .EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                    .Where(f => Path.GetFileName(f)
                        .Contains(requestId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!matchingFiles.Any())
                {
                    _logger?.LogWarning("No files found for RequestId: {RequestId}", requestId);
                    continue;
                }

                using var transaction = await _db.Database.BeginTransactionAsync(ct);

                try
                {
                    var batch = new IDPBatch
                    {
                        ChannelType = ChannelType.BPMS,
                        SourceReference = requestId,
                        BatchStatus = BatchStatus.Created,
                        TotalDocuments = matchingFiles.Count,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.IDPBatches.Add(batch);
                    await _db.SaveChangesAsync(ct);

                    foreach (var file in matchingFiles)
                    {
                        var fileInfo = new FileInfo(file);

                        // Prevent duplicate insert using FileHash
                        var fileHash = BatchHelper.ComputeSha256(fileInfo.FullName);

                        var exists = await _db.IDPDocuments
                            .Where(d => d.FileHash == fileHash)
                            .FirstOrDefaultAsync(ct);

                        if (exists != null)
                        {
                            _logger?.LogWarning("Duplicate file skipped: {File}", file);
                            continue;
                        }

                        var document = new IDPDocument
                        {
                            BatchId = batch.BatchId,
                            FileName = fileInfo.Name,
                            FilePath = fileInfo.FullName,
                            FileHash = fileHash,
                            MimeType = BatchHelper.GetMimeType(fileInfo.Extension),
                            FileSizeBytes = fileInfo.Length,
                            DocumentStatus = DocumentStatus.Downloaded,
                            RetryCount = 0,
                            CreatedAt = DateTime.UtcNow
                        };

                        _db.IDPDocuments.Add(document);
                    }

                    await _db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    success = true;
                    _logger?.LogInformation("Batch created successfully for {RequestId}", requestId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    _logger?.LogError(ex, "Failed processing RequestId: {RequestId}", requestId);
                }
            }
            return success;
        }
    }
}
