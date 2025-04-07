using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Vaccine
{
    public class VaccineDetail
    {
        public string VaccineName { get; set; }
        public int? Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? DateAdministered { get; set; }
    }
}
