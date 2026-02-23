using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class MailMessageRepository : ISQLRepository<MailMessage1>, IMailMessageRepository
    {
        private readonly AppDbContext _context;

        public MailMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<MailMessage1>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
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
                var lambda = Expression.Lambda<Func<MailMessage1, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<MailMessage1>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<MailMessage1?> GetAsync(string skey)
        {
            return await _context.MailMessages.FindAsync(skey);
        }

        public async Task<MailMessage1?> GetAsync(int skey)
        {
            return await _context.MailMessages.FindAsync(skey);
        }

        public async Task<MailMessage1> AddAsync(MailMessage1 model)
        {
            try
            {
                
                await _context.MailMessages.AddAsync(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return model;
        }

        public async Task<MailMessage1> UpdateAsync(MailMessage1 model)
        {
            _context.MailMessages.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailMessage1> DeleteAsync(MailMessage1 model)
        {
            _context.MailMessages.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
