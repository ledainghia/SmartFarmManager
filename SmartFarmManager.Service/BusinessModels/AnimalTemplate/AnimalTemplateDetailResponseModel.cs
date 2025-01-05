using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalTemplate
{
    public class AnimalTemplateDetailResponseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public List<GrowthStageTemplateResponse> GrowthStageTemplates { get; set; }
        public List<VaccineTemplateResponse> VaccineTemplates { get; set; }
    }

    public class GrowthStageTemplateResponse
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string StageName { get; set; }
        public decimal? WeightAnimal { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public string Notes { get; set; }
    }


    public class VaccineTemplateResponse
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string VaccineName { get; set; }
        public string ApplicationMethod { get; set; }
        public int? ApplicationAge { get; set; }
        public int Session { get; set; }
    }

}
