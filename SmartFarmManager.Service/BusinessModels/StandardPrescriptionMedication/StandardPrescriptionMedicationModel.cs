using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.StandardPrescriptionMedication
{
    public class StandardPrescriptionMedicationModel
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; }
        public int Morning { get; set; }
        public int Afternoon { get; set; }
        public int Evening { get; set; }
        public int Noon { get; set; }
    }

}
