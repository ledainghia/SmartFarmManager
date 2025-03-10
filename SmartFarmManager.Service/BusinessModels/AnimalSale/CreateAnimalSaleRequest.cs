using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalSale
{
    public class CreateAnimalSaleRequest
    {
        public Guid GrowthStageId { get; set; }
        public DateTime? SaleDate { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public Guid StaffId { get; set; }
        public Guid SaleTypeId { get; set; }
    }
}
