using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using Amazon.Textract.Model;
using AppCore.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Services
{
    public class PdfTextractService : IPdfTextractService
    {
        private readonly IAmazonS3 _s3;
        private readonly IAmazonTextract _textract;
        private readonly ILogger<PdfTextractService> _logger;

        public PdfTextractService(IAmazonS3 s3, IAmazonTextract textract, ILogger<PdfTextractService> logger)
        {
            _s3 = s3;
            _textract = textract;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> ProcessPdfAsync(string pdfPath, string bucketName, string s3Key, CancellationToken ct)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("PDF not found", pdfPath);

            try
            {
                // 1️⃣ Upload original PDF to S3
                _logger.LogInformation("Uploading PDF to S3: {File}", pdfPath);

                await _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    FilePath = pdfPath,
                    ContentType = "application/pdf"
                }, ct);

                // 2️⃣ Start Textract async job
                _logger.LogInformation("Starting Textract async job...");

                var startResponse = await _textract.StartDocumentTextDetectionAsync(
                    new StartDocumentTextDetectionRequest
                    {
                        DocumentLocation = new DocumentLocation
                        {
                            S3Object = new Amazon.Textract.Model.S3Object
                            {
                                Bucket = bucketName,
                                Name = s3Key
                            }
                        }
                    }, ct);

                var jobId = startResponse.JobId;

                _logger.LogInformation("Textract JobId: {JobId}", jobId);

                // 3️⃣ Poll job status
                GetDocumentTextDetectionResponse result;

                do
                {
                    ct.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(5), ct);

                    result = await _textract.GetDocumentTextDetectionAsync(
                        new GetDocumentTextDetectionRequest
                        {
                            JobId = jobId,
                            MaxResults = 1000
                        }, ct);

                    _logger.LogInformation("Textract job status: {Status}", result.JobStatus);

                } while (result.JobStatus == JobStatus.IN_PROGRESS);

                if (result.JobStatus != JobStatus.SUCCEEDED)
                {
                    throw new Exception($"Textract job failed: {result.JobStatus}");
                }

                // 4️⃣ Retrieve all pages (handle pagination)
                var allLines = new List<object>();
                string? nextToken = null;

                do
                {
                    var pageResponse = await _textract.GetDocumentTextDetectionAsync(
                        new GetDocumentTextDetectionRequest
                        {
                            JobId = jobId,
                            NextToken = nextToken,
                            MaxResults = 1000
                        }, ct);

                    foreach (var block in pageResponse.Blocks)
                    {
                        if (block.BlockType == BlockType.LINE)
                        {
                            allLines.Add(new
                            {
                                Page = block.Page,
                                Text = block.Text,
                                Confidence = block.Confidence,
                                Geometry = block.Geometry
                            });
                        }
                    }

                    nextToken = pageResponse.NextToken;

                } while (!string.IsNullOrEmpty(nextToken));

                _logger.LogInformation("Textract processing completed successfully.");

                return new Dictionary<string, object>
                {
                    ["Blocks"] = allLines,
                    ["JobId"] = jobId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Textract async processing failed.");
                throw;
            }
        }
    }
}
