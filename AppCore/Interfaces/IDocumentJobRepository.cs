using AppCore.EntityModels;
using AppCore.Helper;

namespace AppCore.Interfaces
{
    public interface IDocumentJobRepository
    {
        Task<PaginatedList<DocumentJob>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<DocumentJob?> GetAsync(string skey);
        Task<DocumentJob?> GetAsync(int skey);
        Task<DocumentJob> AddAsync(DocumentJob model);
        Task<DocumentJob> UpdateAsync(DocumentJob model);
        Task<DocumentJob> DeleteAsync(DocumentJob model);
    }
}
