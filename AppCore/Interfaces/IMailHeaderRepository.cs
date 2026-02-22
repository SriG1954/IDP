using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IMailHeaderRepository
    {
        Task<PaginatedList<MailHeader>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<MailHeader?> GetAsync(string skey);
        Task<MailHeader?> GetAsync(int skey);
        Task<MailHeader> AddAsync(MailHeader model);
        Task<MailHeader> UpdateAsync(MailHeader model);
        Task<MailHeader> DeleteAsync(MailHeader model);
    }
}
