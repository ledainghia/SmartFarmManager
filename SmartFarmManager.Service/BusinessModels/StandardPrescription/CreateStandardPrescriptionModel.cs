using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.StandardPrescription
{
    public class CreateStandardPrescriptionModel
    {
        public Guid DiseaseId { get; set; }
        public string Notes { get; set; }
        public int RecommendDay { get; set; }

        public List<CreateStandardPrescriptionMedicationModel> StandardPrescriptionMedications { get; set; }
    }
}
