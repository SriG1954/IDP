using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class MailSyncStateRepository : ISQLRepository<MailSyncState>, IMailSyncStateRepository
    {
        private readonly AppDbContext _context;

        public MailSyncStateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<MailSyncState>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.MailSyncStates.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<MailSyncState, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<MailSyncState>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<MailSyncState?> GetAsync(string skey)
        {
            return await _context.MailSyncStates.FindAsync(skey);
        }

        public async Task<MailSyncState?> GetAsync(int skey)
        {
            return await _context.MailSyncStates.FindAsync(skey);
        }

        public async Task<MailSyncState> AddAsync(MailSyncState model)
        {
            await _context.MailSyncStates.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailSyncState> UpdateAsync(MailSyncState model)
        {
            _context.MailSyncStates.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailSyncState> DeleteAsync(MailSyncState model)
        {
            _context.MailSyncStates.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
