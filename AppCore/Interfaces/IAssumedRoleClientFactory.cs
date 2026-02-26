using Amazon.S3;
using Amazon.SageMakerRuntime;
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
        Task<IAmazonTextract> GetTextractClientAsync(CancellationToken ct = default);
        Task<AmazonSageMakerRuntimeClient> GetAmazonSageMakerRuntimeClientAsync(CancellationToken ct = default);
        Task<IAmazonS3> GetS3ClientAsync(CancellationToken ct = default);
    }
}
