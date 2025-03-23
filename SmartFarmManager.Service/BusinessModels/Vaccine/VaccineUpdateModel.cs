using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Vaccine
{
    public class VaccineUpdateModel
    {
        public string? Name { get; set; }
        public string? Method { get; set; }
        public double? Price { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
    }

}
