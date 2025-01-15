using SmartFarmManager.Service.BusinessModels.MedicalSymptomDetail;
using SmartFarmManager.Service.BusinessModels.Picture;
using SmartFarmManager.Service.BusinessModels.Prescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.MedicalSymptom
{
    public class MedicalSymptomModel
    {
        public Guid Id { get; set; }
        public Guid FarmingBatchId { get; set; }
        public string Symtom {  get; set; }

        public Guid? PrescriptionId { get; set; }

        public string Diagnosis { get; set; }

        public DateTime? CreateAt { get; set; }
        public string? Status { get; set; }

        public int? AffectedQuantity { get; set; }
        public int? Quantity { get; set; }
        public string NameAnimal { get; set; }

        public string Notes { get; set; }

        public virtual ICollection<PictureModel> Pictures { get; set; } = new List<PictureModel>();

        public List<MedicalSymptomDetailModel> MedicalSymptomDetails { get; set; }

        public PrescriptionModel? Prescriptions { get; set; }
    }
}
