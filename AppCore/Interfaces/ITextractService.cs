using AppCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface ITextractService
    {
        Task<TextractResult> AnalyzeAsync(string s3Bucket, string s3Key, CancellationToken ct);
        Task<TextractResult> AnalyzeBytesAsync(byte[] bytes, CancellationToken ct); // small docs
        Task<Dictionary<string, object>> DetectLinesAsync(string imagePath, CancellationToken ct);
    }

    public sealed record TextractResult(string PlainText, IReadOnlyList<KVPair> KeyValues);
}
