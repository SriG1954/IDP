using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IMailSyncStateRepository
    {
        Task<PaginatedList<MailSyncState>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<MailSyncState?> GetAsync(string skey);
        Task<MailSyncState?> GetAsync(int skey);
        Task<MailSyncState> AddAsync(MailSyncState model);
        Task<MailSyncState> UpdateAsync(MailSyncState model);
        Task<MailSyncState> DeleteAsync(MailSyncState model);
    }
}
