using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IPdfTextractService
    {
        Task<Dictionary<string, object>> ProcessPdfAsync(
            string pdfPath,
            string bucketName,
            string s3Key,
            CancellationToken ct);
    }
}
