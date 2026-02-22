using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Repositories
{
    public class DocumentResultRepository : IDocumentResultRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DocumentResultRepository(AppDbContext context, IMemoryCache cache) { _context = context; _cache = cache; }

        public async Task<PaginatedList<DocumentResult>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.DocumentResults.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<DocumentResult, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<DocumentResult>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<DocumentResult?> GetAsync(string skey)
        {
            return await _context.DocumentResults.FindAsync(skey);
        }

        public async Task<DocumentResult?> GetAsync(int skey)
        {
            return await _context.DocumentResults.FindAsync(skey);
        }

        public async Task<DocumentResult> AddAsync(DocumentResult model)
        {
            await _context.DocumentResults.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<DocumentResult> UpdateAsync(DocumentResult model)
        {
            _context.DocumentResults.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<DocumentResult> DeleteAsync(DocumentResult model)
        {
            _context.DocumentResults.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
