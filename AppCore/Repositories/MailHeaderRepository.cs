using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class MailHeaderRepository : ISQLRepository<MailHeader>, IMailHeaderRepository
    {
        private readonly AppDbContext _context;

        public MailHeaderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<MailHeader>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.MailHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<MailHeader, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<MailHeader>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<MailHeader?> GetAsync(string skey)
        {
            return await _context.MailHeaders.FindAsync(skey);
        }

        public async Task<MailHeader?> GetAsync(int skey)
        {
            return await _context.MailHeaders.FindAsync(skey);
        }

        public async Task<MailHeader> AddAsync(MailHeader model)
        {
            await _context.MailHeaders.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailHeader> UpdateAsync(MailHeader model)
        {
            _context.MailHeaders.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailHeader> DeleteAsync(MailHeader model)
        {
            _context.MailHeaders.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
