using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalSale
{
    public class AnimalSaleGroupedByTypeModel
    {
        public string SaleType { get; set; }
        public int TotalQuantity { get; set; }
        public double TotalRevenue { get; set; }
        public double UnitPriceAverage { get; set; }
        public List<AnimalSaleLogModel> Logs { get; set; }
    }

    public class AnimalSaleLogModel
    {
        public DateTime? SaleDate { get; set; }
        public int Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double Total { get; set; }
    }

}
