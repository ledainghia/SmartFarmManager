using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.StandardPrescription
{
    public class CreateStandardPrescriptionMedicationModel
    {
        public Guid MedicationId { get; set; }
        public int Morning { get; set; }
        public int Noon { get; set; }
        public int Afternoon { get; set; }
        public int Evening { get; set; }
    }
}
