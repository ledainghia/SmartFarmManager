using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Medication
{
    public class MedicationDetailResponseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UsageInstructions { get; set; }
        public decimal? Price { get; set; }
        public int? DoseWeight { get; set; }
        public int? Weight { get; set; }
        public int? DoseQuantity { get; set; }
        public decimal? PricePerDose { get; set; }
    }
}
