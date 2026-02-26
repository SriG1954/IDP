using Amazon.S3;
using Amazon.Textract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IAwsClientProvider
    {
        public Task<(AmazonS3Client s3Client, AmazonTextractClient textractClient)> GetAwsClientsOnEC2Async();
        public (IAmazonS3 s3Client, IAmazonTextract textractClient) GetAwsClientsWhileRunningOnLocal();
    }
}
