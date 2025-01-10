using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.SaleType
{
    public class SaleTypeFilterModel
    {
        public string? StageTypeName { get; set; } // Lọc theo tên giai đoạn bán
        public int PageNumber { get; set; } = 1; // Số trang
        public int PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }

}
