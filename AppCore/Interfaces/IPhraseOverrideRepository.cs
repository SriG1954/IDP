using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IPhraseOverrideRepository
    {
        Task<PaginatedList<PhraseOverride>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<PhraseOverride?> GetAsync(string skey);
        Task<PhraseOverride?> GetAsync(int skey);
        Task<PhraseOverride> AddAsync(PhraseOverride model);
        Task<PhraseOverride> UpdateAsync(PhraseOverride model);
        Task<PhraseOverride> DeleteAsync(PhraseOverride model);
    }
}
