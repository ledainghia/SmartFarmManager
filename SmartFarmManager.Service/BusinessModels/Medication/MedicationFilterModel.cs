using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Medication
{
    public class MedicationFilterModel
    {
        public string KeySearch { get; set; } // Tìm kiếm theo tên thuốc
        public int PageNumber { get; set; } = 1;  // Số trang
        public int PageSize { get; set; } = 10;   // Số mục trên mỗi trang
        public decimal? MinPrice { get; set; } // Giá tối thiểu
        public decimal? MaxPrice { get; set; } // Giá tối đa
        public int? MinDoseWeight { get; set; } // Khối lượng liều tối thiểu
        public int? MaxDoseWeight { get; set; } // Khối lượng liều tối đa
        public int? MinWeight { get; set; } // Khối lượng thuốc tối thiểu
        public int? MaxWeight { get; set; } // Khối lượng thuốc tối đa
        public int? MinDoseQuantity { get; set; } // Số lượng liều tối thiểu
        public int? MaxDoseQuantity { get; set; } // Số lượng liều tối đa
        public decimal? MinPricePerDose { get; set; } // Giá theo liều tối thiểu
        public decimal? MaxPricePerDose { get; set; } // Giá theo liều tối đa
        public bool? IsDeleted { get; set; }
    }
}
