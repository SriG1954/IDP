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
        Task<PaginatedList<MailMessage1>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<MailMessage1?> GetAsync(string skey);
        Task<MailMessage1?> GetAsync(int skey);
        Task<MailMessage1> AddAsync(MailMessage1 model);
        Task<MailMessage1> UpdateAsync(MailMessage1 model);
        Task<MailMessage1> DeleteAsync(MailMessage1 model);
    }
}
