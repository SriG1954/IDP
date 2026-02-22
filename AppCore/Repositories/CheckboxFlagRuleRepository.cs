using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class CheckboxFlagRuleRepository : ICheckboxFlagRuleRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public CheckboxFlagRuleRepository(AppDbContext context, IMemoryCache cache) { _context = context; _cache = cache; }

        public async Task<PaginatedList<CheckboxFlagRule>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.CheckboxFlagRules.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<CheckboxFlagRule, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<CheckboxFlagRule>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<CheckboxFlagRule?> GetAsync(string skey)
        {
            return await _context.CheckboxFlagRules.FindAsync(skey);
        }

        public async Task<CheckboxFlagRule?> GetAsync(int skey)
        {
            return await _context.CheckboxFlagRules.FindAsync(skey);
        }

        public async Task<CheckboxFlagRule> AddAsync(CheckboxFlagRule model)
        {
            await _context.CheckboxFlagRules.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<CheckboxFlagRule> UpdateAsync(CheckboxFlagRule model)
        {
            _context.CheckboxFlagRules.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<CheckboxFlagRule> DeleteAsync(CheckboxFlagRule model)
        {
            _context.CheckboxFlagRules.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
