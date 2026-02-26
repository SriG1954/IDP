using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IVLMService
    {
        Task<string> CallKvAsync(string endpointName, string kvPrompt, Dictionary<string, object> textractLines, string imagePath, int maxTokens, double temperature, CancellationToken ct);
        Task<string> CallClassifierAsync(string endpointName, string kvSummary, string workTypeCatalog, string imagePath, string classifierInstructions, int maxTokens, double temperature, CancellationToken ct);
    }

    public sealed class VlmOptions
    {
        public required string Region { get; init; }
        public int MaxImageSizeMb { get; init; } = 5;
        public int TimeoutSeconds { get; init; } = 120;
    }
}
