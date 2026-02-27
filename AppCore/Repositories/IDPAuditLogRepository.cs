using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class IDPAuditLogRepository : ISQLRepository<IDPAuditLog>, IIDPAuditLogRepository
    {
        private readonly AppDbContext _context;

        public IDPAuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<IDPAuditLog>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.IDPAuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<IDPAuditLog, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<IDPAuditLog>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<IDPAuditLog?> GetAsync(string skey)
        {
            return await _context.IDPAuditLogs.FindAsync(skey);
        }

        public async Task<IDPAuditLog?> GetAsync(int skey)
        {
            return await _context.IDPAuditLogs.FindAsync(skey);
        }

        public async Task<IDPAuditLog> AddAsync(IDPAuditLog model)
        {
            await _context.IDPAuditLogs.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<IDPAuditLog> UpdateAsync(IDPAuditLog model)
        {
            _context.IDPAuditLogs.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<IDPAuditLog> DeleteAsync(IDPAuditLog model)
        {
            _context.IDPAuditLogs.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> AddLogAsync(int entityid, long batchid, string? message, AuditLogLevel auditLogLevel = AuditLogLevel.Info, AuditEventType eventtype = AuditEventType.System)
        {
            try
            {
                IDPAuditLog auditLog = new IDPAuditLog
                {
                    EntityId = entityid,
                    BatchId = batchid,
                    DocumentId = null,
                    Message = message!,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    EventType = eventtype, // event is batch processing
                    AuditLogLevel = auditLogLevel // general info
                };

                await AddAsync(auditLog);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }

}
