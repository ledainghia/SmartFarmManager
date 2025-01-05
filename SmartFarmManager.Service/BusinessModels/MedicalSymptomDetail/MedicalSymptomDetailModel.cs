using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.MedicalSymptomDetail
{
    public class MedicalSymptomDetailModel
    {
        public Guid MedicalSymptomId { get; set; }
        public Guid SymptomId { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? Notes { get; set; }
    }
}
