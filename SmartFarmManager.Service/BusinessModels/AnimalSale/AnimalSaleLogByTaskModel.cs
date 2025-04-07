using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalSale
{
    public class AnimalSaleLogByTaskModel
    {
        public Guid GrowthStageId { get; set; }
        public string GrowthStageName { get; set; }
        public DateTime? SaleDate { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; }
        public Guid SaleTypeId { get; set; }
        public string SaleTypeName { get; set; }
        public DateTime LogTime { get; set; }
    }
}
