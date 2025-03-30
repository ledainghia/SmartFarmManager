using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Symptom
{
    public class SymptomFilterModel
    {
        public string? KeySearch { get; set; } 
        public bool? IsDeleted { get; set; } 
        public int PageNumber { get; set; } = 1;  
        public int PageSize { get; set; } = 20;
    }
}
