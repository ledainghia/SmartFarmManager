using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class StandardPrescriptionMedication : EntityBase
    {
        public Guid PrescriptionId { get; set; }

        public Guid MedicationId { get; set; }

        public int Dosage { get; set; }

        public bool Morning { get; set; }
        public bool Noon { get; set; }

        public bool Afternoon { get; set; }

        public bool Evening { get; set; }



        public virtual StandardPrescription Prescription { get; set; }

        public virtual Medication Medication { get; set; }
    }
}
