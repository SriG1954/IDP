using AppCore.EntityModels;
using AppCore.Helper;

namespace AppCore.Interfaces
{
    public interface IPromptRepository
    {
        Task<PaginatedList<Prompt1>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<Prompt1?> GetAsync(string skey);
        Task<Prompt1?> GetAsync(int skey);
        Task<Prompt1> AddAsync(Prompt1 model);
        Task<Prompt1> UpdateAsync(Prompt1 model);
        Task<Prompt1> DeleteAsync(Prompt1 model);

        Task<string> GetActivePromptAsync(string name, CancellationToken ct);
        Task<(string endpointName, string region, int maxTokens, double temperature)> GetActiveEndpointAsync(string name, CancellationToken ct);
        Task<string> GetActiveCatalogJsonAsync(CancellationToken ct);
        Task<IReadOnlyList<(string phrase, string wt, string swt)>> GetPhraseOverridesAsync(CancellationToken ct);

    }
}
