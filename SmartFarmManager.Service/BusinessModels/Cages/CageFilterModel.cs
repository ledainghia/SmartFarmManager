using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Cages
{
    public class CageFilterModel
    {
        public Guid? FarmId { get; set; } // Lọc theo farm
        public string? AnimalType { get; set; } // Lọc theo loại động vật
        public string? Name { get; set; } // Tìm kiếm theo tên
        public bool? BoardStatus { get; set; } // Lọc theo trạng thái board
        public int PageNumber { get; set; } = 1; // Số trang (mặc định là 1)
        public int PageSize { get; set; } = 10; // Số item trên mỗi trang (mặc định là 10)
    }
}
