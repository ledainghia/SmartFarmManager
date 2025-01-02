using SmartFarmManager.Service.BusinessModels.Picture;
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

        public string Symptoms { get; set; }

        public string Diagnosis { get; set; }

        public string Treatment { get; set; }

        public string Status { get; set; }

        public int? AffectedQuantity { get; set; }
        public int? Quantity { get; set; }
        public string NameAnimal { get; set; }

        public string Notes { get; set; }

        public virtual ICollection<PictureModel> Pictures { get; set; } = new List<PictureModel>();
    }
}
