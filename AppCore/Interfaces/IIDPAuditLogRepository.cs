using AppCore.EntityModels;
using AppCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IIDPAuditLogRepository
    {
        Task<PaginatedList<IDPAuditLog>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<IDPAuditLog?> GetAsync(string skey);
        Task<IDPAuditLog?> GetAsync(int skey);
        Task<IDPAuditLog> AddAsync(IDPAuditLog model);
        Task<IDPAuditLog> UpdateAsync(IDPAuditLog model);
        Task<IDPAuditLog> DeleteAsync(IDPAuditLog model);
        Task<bool> AddLogAsync(int entityid, long batchid, string? message, AuditLogLevel auditLogLevel = AuditLogLevel.Info, AuditEventType eventtype = AuditEventType.System);
    }
}
