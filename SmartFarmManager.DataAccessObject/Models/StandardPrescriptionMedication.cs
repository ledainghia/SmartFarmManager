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


        public int Morning { get; set; }
        public int Noon { get; set; }

        public int Afternoon { get; set; }

        public int Evening { get; set; }



        public virtual StandardPrescription Prescription { get; set; }

        public virtual Medication Medication { get; set; }
    }
}
