using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineTemplate
{
    public class VaccineTemplateDetailModel
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string VaccineName { get; set; }
        public string ApplicationMethod { get; set; }
        public int ApplicationAge { get; set; }
        public int Session { get; set; }

        // Thông tin AnimalTemplate liên kết
        public AnimalTemplateResponse? AnimalTemplate { get; set; }
    }
    public class AnimalTemplateResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public int? DefaultCapacity { get; set; }
        public string? Notes { get; set; }
    }
}
