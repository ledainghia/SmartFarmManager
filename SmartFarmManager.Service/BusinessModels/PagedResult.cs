using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set;}
    }
}
