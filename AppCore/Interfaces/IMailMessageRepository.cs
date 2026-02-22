using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IMailMessageRepository
    {
        Task<PaginatedList<MailMessage>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<MailMessage?> GetAsync(string skey);
        Task<MailMessage?> GetAsync(int skey);
        Task<MailMessage> AddAsync(MailMessage model);
        Task<MailMessage> UpdateAsync(MailMessage model);
        Task<MailMessage> DeleteAsync(MailMessage model);
    }
}
