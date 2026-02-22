using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class ModelEndpointRepository : IModelEndpointRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public ModelEndpointRepository(AppDbContext context, IMemoryCache cache) { _context = context; _cache = cache; }

        public async Task<PaginatedList<ModelEndpoint>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.ModelEndpoints.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<ModelEndpoint, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<ModelEndpoint>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<ModelEndpoint?> GetAsync(string skey)
        {
            return await _context.ModelEndpoints.FindAsync(skey);
        }

        public async Task<ModelEndpoint?> GetAsync(int skey)
        {
            return await _context.ModelEndpoints.FindAsync(skey);
        }

        public async Task<ModelEndpoint> AddAsync(ModelEndpoint model)
        {
            await _context.ModelEndpoints.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<ModelEndpoint> UpdateAsync(ModelEndpoint model)
        {
            _context.ModelEndpoints.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<ModelEndpoint> DeleteAsync(ModelEndpoint model)
        {
            _context.ModelEndpoints.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
    }
}
