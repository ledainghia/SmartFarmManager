using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Symptom
{
    public class SymptomModel
    {
        public Guid? Id { get; set; }
        public string SymptomName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
