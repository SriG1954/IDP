using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IMailAttachmentRepository
    {
        Task<PaginatedList<MailAttachment>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<MailAttachment?> GetAsync(string skey);
        Task<MailAttachment?> GetAsync(int skey);
        Task<MailAttachment> AddAsync(MailAttachment model);
        Task<MailAttachment> UpdateAsync(MailAttachment model);
        Task<MailAttachment> DeleteAsync(MailAttachment model);
    }
}
