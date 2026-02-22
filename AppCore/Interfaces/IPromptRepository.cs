using AppCore.EntityModels;
using AppCore.Helper;

namespace AppCore.Interfaces
{
    public interface IPromptRepository
    {
        Task<PaginatedList<Prompt>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<Prompt?> GetAsync(string skey);
        Task<Prompt?> GetAsync(int skey);
        Task<Prompt> AddAsync(Prompt model);
        Task<Prompt> UpdateAsync(Prompt model);
        Task<Prompt> DeleteAsync(Prompt model);

        Task<string> GetActivePromptAsync(string name, CancellationToken ct);
        Task<(string endpointName, string region, int maxTokens, double temperature)> GetActiveEndpointAsync(string name, CancellationToken ct);
        Task<string> GetActiveCatalogJsonAsync(CancellationToken ct);
        Task<IReadOnlyList<(string phrase, string wt, string swt)>> GetPhraseOverridesAsync(CancellationToken ct);

    }
}
