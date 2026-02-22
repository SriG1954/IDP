using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface ISQLRepository<T> where T : class
    {
        Task<PaginatedList<T>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<T?> GetAsync(string skey);
        Task<T?> GetAsync(int skey);

        Task<T> AddAsync(T model);
        Task<T> UpdateAsync(T model);
        Task<T> DeleteAsync(T model);
    }
}
