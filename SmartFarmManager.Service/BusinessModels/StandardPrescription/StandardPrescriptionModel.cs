using SmartFarmManager.Service.BusinessModels.StandardPrescriptionMedication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.StandardPrescription
{
    public class StandardPrescriptionModel
    {
        public Guid Id { get; set; }
        public string Notes { get; set; }
        public Guid DiseaseId { get; set; }
        public List<StandardPrescriptionMedicationModel> Medications { get; set; }
    }

}
