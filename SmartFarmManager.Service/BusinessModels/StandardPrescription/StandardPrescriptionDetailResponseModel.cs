using SmartFarmManager.Service.BusinessModels.StandardPrescriptionMedication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.StandardPrescription
{
    public class StandardPrescriptionDetailResponseModel
    {
        public Guid Id { get; set; }
        public string DiseaseName { get; set; }
        public string DiseaseDescription { get; set; }
        public string Notes { get; set; }
        public int RecommendDay { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DiseaseId { get; set; }
        public List<StandardPrescriptionMedicationModel> StandardPrescriptionMedications { get; set; }
    }
}
