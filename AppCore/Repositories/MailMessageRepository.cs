using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class MailMessageRepository : ISQLRepository<MailMessage>, IMailMessageRepository
    {
        private readonly AppDbContext _context;

        public MailMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<MailMessage>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.MailMessages.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<MailMessage, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<MailMessage>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<MailMessage?> GetAsync(string skey)
        {
            return await _context.MailMessages.FindAsync(skey);
        }

        public async Task<MailMessage?> GetAsync(int skey)
        {
            return await _context.MailMessages.FindAsync(skey);
        }

        public async Task<MailMessage> AddAsync(MailMessage model)
        {
            await _context.MailMessages.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailMessage> UpdateAsync(MailMessage model)
        {
            _context.MailMessages.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailMessage> DeleteAsync(MailMessage model)
        {
            _context.MailMessages.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
