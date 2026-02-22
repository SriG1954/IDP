using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class MailRecipientRepository : ISQLRepository<MailRecipient>, IMailRecipientRepository
    {
        private readonly AppDbContext _context;

        public MailRecipientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<MailRecipient>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.MailRecipients.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<MailRecipient, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<MailRecipient>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<MailRecipient?> GetAsync(string skey)
        {
            return await _context.MailRecipients.FindAsync(skey);
        }

        public async Task<MailRecipient?> GetAsync(int skey)
        {
            return await _context.MailRecipients.FindAsync(skey);
        }

        public async Task<MailRecipient> AddAsync(MailRecipient model)
        {
            await _context.MailRecipients.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailRecipient> UpdateAsync(MailRecipient model)
        {
            _context.MailRecipients.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailRecipient> DeleteAsync(MailRecipient model)
        {
            _context.MailRecipients.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
