using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IRuleProvider
    {
        //Task<ModelThreshold?> GetModelThresholdAsync(string label, CancellationToken ct);
        //Task<IReadOnlyList<RegexRule>> GetActiveRegexRulesAsync(CancellationToken ct);
        //Task<IReadOnlyList<KvExtractionRule>> GetActiveKvRulesAsync(CancellationToken ct);
    }

    public interface IPromptProvider
    {
        Task<Prompt> GetActivePromptAsync(string name, CancellationToken ct); // highest Version + IsActive
    }
}
