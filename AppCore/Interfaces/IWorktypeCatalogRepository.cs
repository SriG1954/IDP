using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IWorktypeCatalogRepository
    {
        Task<PaginatedList<WorktypeCatalog>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<WorktypeCatalog?> GetAsync(string skey);
        Task<WorktypeCatalog?> GetAsync(int skey);
        Task<WorktypeCatalog> AddAsync(WorktypeCatalog model);
        Task<WorktypeCatalog> UpdateAsync(WorktypeCatalog model);
        Task<WorktypeCatalog> DeleteAsync(WorktypeCatalog model);
    }
}
