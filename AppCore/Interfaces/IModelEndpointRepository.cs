using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IModelEndpointRepository
    {
        Task<PaginatedList<ModelEndpoint>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<ModelEndpoint?> GetAsync(string skey);
        Task<ModelEndpoint?> GetAsync(int skey);
        Task<ModelEndpoint> AddAsync(ModelEndpoint model);
        Task<ModelEndpoint> UpdateAsync(ModelEndpoint model);
        Task<ModelEndpoint> DeleteAsync(ModelEndpoint model);
    }
}
