using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineTemplate
{
    public class VaccineTemplateFilterModel
    {
        public Guid? TemplateId { get; set; }
        public string? VaccineName { get; set; }
        public int? Session { get; set; }
        public int? ApplicationAge { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
