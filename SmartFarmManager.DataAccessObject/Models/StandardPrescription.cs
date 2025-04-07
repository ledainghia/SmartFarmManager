 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class StandardPrescription : EntityBase
    {
        public Guid DiseaseId { get; set; }

        public string Notes { get; set; }
        public int RecommendDay{ get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual Disease Disease { get; set; }

        public virtual ICollection<StandardPrescriptionMedication> StandardPrescriptionMedications { get; set; } = new List<StandardPrescriptionMedication>();
    }
}
