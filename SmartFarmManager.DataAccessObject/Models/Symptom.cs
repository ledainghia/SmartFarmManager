using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class Symptom:EntityBase
    {
        public string SymptomName { get; set; }
        public virtual ICollection<MedicalSymtomDetail> MedicalSymptomDetails { get; set; } = new List<MedicalSymtomDetail>();
    }
}
