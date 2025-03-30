using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.StandardPrescription
{
    public class StandardPrescriptionFilterModel
    {
        public string? KeySearch { get; set; }

        public Guid? DiseaseId { get; set; }
        public string? Notes { get; set; }
        public int? RecommendDay { get; set; }
        public bool? IsDeleted { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
