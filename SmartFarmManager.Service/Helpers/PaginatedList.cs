using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int TotalCount { get; }
        public int CurrentPage { get; }
        public int PageSize { get; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = count;
            CurrentPage = pageIndex;
            PageSize = pageSize;
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
