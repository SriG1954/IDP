using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class MailAttachmentRepository : ISQLRepository<MailAttachment>, IMailAttachmentRepository
    {
        private readonly AppDbContext _context;

        public MailAttachmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<MailAttachment>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.MailAttachments.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<MailAttachment, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<MailAttachment>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<MailAttachment?> GetAsync(string skey)
        {
            return await _context.MailAttachments.FindAsync(skey);
        }

        public async Task<MailAttachment?> GetAsync(int skey)
        {
            return await _context.MailAttachments.FindAsync(skey);
        }

        public async Task<MailAttachment> AddAsync(MailAttachment model)
        {
            await _context.MailAttachments.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailAttachment> UpdateAsync(MailAttachment model)
        {
            _context.MailAttachments.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MailAttachment> DeleteAsync(MailAttachment model)
        {
            _context.MailAttachments.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }

    }
}
