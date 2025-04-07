using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Cages
{
    public class CageFilterModel
    {
        public Guid? FarmId { get; set; }  // Lọc theo FarmId
        public string? PenCode { get; set; } // Lọc theo PenCode
        public string? Name { get; set; }  // Lọc theo Name của chuồng
        public string? SearchKey { get; set; }  // Trường tìm kiếm tổng hợp
        public bool? HasFarmingBatch { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
