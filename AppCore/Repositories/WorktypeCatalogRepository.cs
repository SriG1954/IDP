using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class WorktypeCatalogRepository : IWorktypeCatalogRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public WorktypeCatalogRepository(AppDbContext context, IMemoryCache cache) { _context = context; _cache = cache; }

        public async Task<PaginatedList<WorktypeCatalog>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.WorktypeCatalogs.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<WorktypeCatalog, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<WorktypeCatalog>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<WorktypeCatalog?> GetAsync(string skey)
        {
            return await _context.WorktypeCatalogs.FindAsync(skey);
        }

        public async Task<WorktypeCatalog?> GetAsync(int skey)
        {
            return await _context.WorktypeCatalogs.FindAsync(skey);
        }

        public async Task<WorktypeCatalog> AddAsync(WorktypeCatalog model)
        {
            await _context.WorktypeCatalogs.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<WorktypeCatalog> UpdateAsync(WorktypeCatalog model)
        {
            _context.WorktypeCatalogs.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<WorktypeCatalog> DeleteAsync(WorktypeCatalog model)
        {
            _context.WorktypeCatalogs.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
