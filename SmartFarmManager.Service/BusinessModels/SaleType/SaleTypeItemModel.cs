using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.SaleType
{
    public class SaleTypeItemModel
    {
        public Guid Id { get; set; }
        public string StageTypeName { get; set; } // Tên giai đoạn bán
        public string? Description { get; set; } // Mô tả
    }

}
