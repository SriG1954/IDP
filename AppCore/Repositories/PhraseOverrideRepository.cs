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
    public class PhraseOverrideRepository : IPhraseOverrideRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PhraseOverrideRepository(AppDbContext context, IMemoryCache cache) { _context = context; _cache = cache; }

        public async Task<PaginatedList<PhraseOverride>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.PhraseOverrides.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<PhraseOverride, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<PhraseOverride>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<PhraseOverride?> GetAsync(string skey)
        {
            return await _context.PhraseOverrides.FindAsync(skey);
        }

        public async Task<PhraseOverride?> GetAsync(int skey)
        {
            return await _context.PhraseOverrides.FindAsync(skey);
        }

        public async Task<PhraseOverride> AddAsync(PhraseOverride model)
        {
            await _context.PhraseOverrides.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<PhraseOverride> UpdateAsync(PhraseOverride model)
        {
            _context.PhraseOverrides.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<PhraseOverride> DeleteAsync(PhraseOverride model)
        {
            _context.PhraseOverrides.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
