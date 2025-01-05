using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class SaleType :EntityBase
    {
        public string StageTypeName { get; set; }
        public string? Discription {  get; set; }
        public virtual ICollection<AnimalSale> AnimalSales { get; set; } = new List<AnimalSale>();
    }
}
