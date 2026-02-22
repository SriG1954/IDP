using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IMailRecipientRepository
    {
        Task<PaginatedList<MailRecipient>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<MailRecipient?> GetAsync(string skey);
        Task<MailRecipient?> GetAsync(int skey);
        Task<MailRecipient> AddAsync(MailRecipient model);
        Task<MailRecipient> UpdateAsync(MailRecipient model);
        Task<MailRecipient> DeleteAsync(MailRecipient model);
    }
}
