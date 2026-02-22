using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class DocumentJobRepository : IDocumentJobRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DocumentJobRepository(AppDbContext context, IMemoryCache cache) { _context = context; _cache = cache; }

        public async Task<PaginatedList<DocumentJob>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.DocumentJobs.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<DocumentJob, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<DocumentJob>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<DocumentJob?> GetAsync(string skey)
        {
            return await _context.DocumentJobs.FindAsync(skey);
        }

        public async Task<DocumentJob?> GetAsync(int skey)
        {
            return await _context.DocumentJobs.FindAsync(skey);
        }

        public async Task<DocumentJob> AddAsync(DocumentJob model)
        {
            await _context.DocumentJobs.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<DocumentJob> UpdateAsync(DocumentJob model)
        {
            _context.DocumentJobs.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<DocumentJob> DeleteAsync(DocumentJob model)
        {
            _context.DocumentJobs.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
