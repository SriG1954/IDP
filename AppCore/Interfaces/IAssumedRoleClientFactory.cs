using Amazon.S3;
using Amazon.Textract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public record S3ClientDescriptor(IAmazonS3 Client, DateTime ExpiryUtc);
    public record TextractClientDescriptor(IAmazonTextract Client, DateTime ExpiryUtc);

    public interface IAssumedRoleClientFactory
    {
        Task<S3ClientDescriptor> CreateS3ClientAsync(CancellationToken ct = default);
        Task<TextractClientDescriptor> CreateTextractClientAsync(CancellationToken ct = default);
    }
}
