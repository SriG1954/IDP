using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IDocumentResultRepository
    {
        Task<PaginatedList<DocumentResult>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<DocumentResult?> GetAsync(string skey);
        Task<DocumentResult?> GetAsync(int skey);
        Task<DocumentResult> AddAsync(DocumentResult model);
        Task<DocumentResult> UpdateAsync(DocumentResult model);
        Task<DocumentResult> DeleteAsync(DocumentResult model);
    }
}
