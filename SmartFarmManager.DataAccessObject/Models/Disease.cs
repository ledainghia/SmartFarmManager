using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class Disease : EntityBase
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<StandardPrescription> StandardPrescriptions { get; set; } = new List<StandardPrescription>();

        public virtual ICollection<MedicalSymptom> MedicalSymptoms { get; set; } = new List<MedicalSymptom>();
    }
}
