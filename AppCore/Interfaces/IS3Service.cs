using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppCore.Interfaces
{
    public interface IS3Service
    {
        Task<XElement> GetObjectMetadataAsync(string bucketName, string s3Key, CancellationToken cancellationToken);
        Task PutObjectAsync(string bucketName, string s3Key, byte[] data, CancellationToken cancellationToken);
        Task PutObjectV1Async(string bucketName, string s3Key, string filePath, CancellationToken cancellationToken);
        Task<string> DownloadPdfAsync(string bucket, string key, CancellationToken cancellationToken);
    }
}
