using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Helper;
using AppCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    public class PromptRepository : IPromptRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public PromptRepository(AppDbContext context, IMemoryCache cache) 
        { 
            _context = context; 
            _cache = cache; 
        }

        public async Task<PaginatedList<Prompt1>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25)
        {
            var query = _context.Prompts.AsQueryable();

            if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(search))
            {
                var parameter = Expression.Parameter(typeof(IDPAuditLog), "x");
                var property = Expression.PropertyOrField(parameter, column);
                var toStringMethod = typeof(object).GetMethod("ToString");
                var propertyAsString = Expression.Call(property, toStringMethod!);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Call(propertyAsString, containsMethod!, Expression.Constant(search));
                var lambda = Expression.Lambda<Func<Prompt1, bool>>(searchExpression, parameter);

                query = query.Where(lambda);
            }

            return await PaginatedList<Prompt1>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<Prompt1?> GetAsync(string skey)
        {
            return await _context.Prompts.FindAsync(skey);
        }

        public async Task<Prompt1?> GetAsync(int skey)
        {
            return await _context.Prompts.FindAsync(skey);
        }

        public async Task<Prompt1> AddAsync(Prompt1 model)
        {
            await _context.Prompts.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Prompt1> UpdateAsync(Prompt1 model)
        {
            _context.Prompts.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Prompt1> DeleteAsync(Prompt1 model)
        {
            _context.Prompts.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public Task<string> GetActivePromptAsync(string name, CancellationToken ct)
            => _cache.GetOrCreateAsync($"prompt:{name}", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var p = await _context.Prompts.AsNoTracking().Where(x => x.IsActive && x.Name == name)
                    .OrderByDescending(x => x.Version).FirstAsync(ct);
                return p.Content;
            })!;

        public Task<(string endpointName, string region, int maxTokens, double temperature)> GetActiveEndpointAsync(string name, CancellationToken ct)
            => _cache.GetOrCreateAsync($"endpoint:{name}", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var ep = await _context.ModelEndpoints.AsNoTracking().Where(x => x.IsActive && x.Name == name)
                    .OrderByDescending(x => x.UpdatedAtUtc).FirstAsync(ct);
                return (ep.EndpointName, ep.Region, ep.MaxTokens ?? 2000, (double)(ep.Temperature ?? 0.1m));
            })!;

        public Task<string> GetActiveCatalogJsonAsync(CancellationToken ct)
            => _cache.GetOrCreateAsync("catalog:active", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var cat = await _context.WorktypeCatalogs.AsNoTracking().Where(x => x.IsActive)
                    .OrderByDescending(x => x.Version).FirstAsync(ct);
                return cat.JsonContent;
            })!;

        public async Task<IReadOnlyList<(string phrase, string wt, string swt)>> GetPhraseOverridesAsync(CancellationToken ct)
        {
            var rows = await _context.PhraseOverrides.AsNoTracking().Where(x => x.IsActive).ToListAsync(ct);
            return rows.Select(r => (r.Phrase, r.Worktype, r.Subworktype)).ToList();
        }
    }
}
