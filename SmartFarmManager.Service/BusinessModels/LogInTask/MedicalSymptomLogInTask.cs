using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.LogInTask
{
    public class MedicalSymptomLogInTaskModel
    {
        public Guid MedicalSymptomId { get; set; }
        public Guid FarmingBatchId { get; set; }
        public string FarmingBatchName { get; set; }
        public string Diagnosis { get; set; }
        public string Status { get; set; }
        public int? AffectedQuantity { get; set; }
        public bool IsEmergency { get; set; } = false;
        public int? QuantityInCage { get; set; }
        public string Notes { get; set; }
        public DateTime? CreateAt { get; set; }
        public List<SymptomLogInTaskModel> Symptoms { get; set; } = new List<SymptomLogInTaskModel>();

    }
    public class SymptomLogInTaskModel
    {
        public Guid SymptomId { get; set; }
        public string SymptomName { get; set; }

    }
}
