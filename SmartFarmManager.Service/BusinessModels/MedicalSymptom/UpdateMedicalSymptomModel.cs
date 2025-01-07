using SmartFarmManager.Service.BusinessModels.Prescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.MedicalSymptom
{
    public class UpdateMedicalSymptomModel
    {
        public Guid Id { get; set; }
        public string Diagnosis { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public PrescriptionModel? Prescriptions { get; set; }
    }
}
