using AppCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Helper
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalRows { get; private set; }
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalRows = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();

            List<T> items = null!;

            if (count <= pageSize)
            {
                pageIndex = 1;
                pageSize = count;
                items = await source.ToListAsync();
            }
            else
            {
                if (count > pageSize)
                {
                    items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
                }
                else
                {
                    items = await source.ToListAsync();
                }
            }

            //var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> CreateFromList(List<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            List<T> items = null!;

            if (count <= pageSize)
            {
                pageIndex = 1;
                pageSize = count;
                items = source.ToList();
            }
            else
            {
                if (count > pageSize)
                {
                    items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    items = source.ToList();
                }
            }

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> SearchCreate(IEnumerable<T> source, int totalRows, int pageIndex, int pageSize)
        {
            var count = totalRows;
            return new PaginatedList<T>(source.ToList(), count, pageIndex, pageSize);
        }

    }
}
