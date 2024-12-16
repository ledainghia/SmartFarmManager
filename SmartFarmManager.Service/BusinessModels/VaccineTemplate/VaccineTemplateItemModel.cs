using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineTemplate
{
    public class VaccineTemplateItemModel
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string VaccineName { get; set; }
        public string ApplicationMethod { get; set; }
        public int? ApplicationAge { get; set; }
        public int Session { get; set; }
    }
}
