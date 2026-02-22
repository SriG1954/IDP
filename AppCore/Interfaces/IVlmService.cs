using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IVlmService
    {
        Task<string> CallKvAsync(string endpointName, string kvPrompt, Dictionary<string, object> textractLines, string imagePath, int maxTokens, double temperature, CancellationToken ct);
        Task<string> CallClassifierAsync(string endpointName, string classifierPromptWithCatalogAndInput, string imagePath, int maxTokens, double temperature, CancellationToken ct);
    }
}
