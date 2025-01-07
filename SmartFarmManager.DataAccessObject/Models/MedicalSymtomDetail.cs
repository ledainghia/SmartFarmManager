using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class MedicalSymtomDetail:EntityBase
    {
        public Guid MedicalSymptomId { get; set; }
        public Guid SymptomId { get; set; }
        public virtual MedicalSymptom MedicalSymptom { get; set; }
        public virtual Symptom Symptom { get; set; }

    }
}
