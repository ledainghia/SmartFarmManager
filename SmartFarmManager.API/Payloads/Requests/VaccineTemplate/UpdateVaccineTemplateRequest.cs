using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.VaccineTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.VaccineTemplate
{
    public class UpdateVaccineTemplateRequest
    {
        [MaxLength(100)]
        public string? VaccineName { get; set; }

        [MaxLength(50)]
        public string? ApplicationMethod { get; set; }

        public int? ApplicationAge { get; set; }
        [ValidSessionType]
        public int? Session { get; set; }

        public UpdateVaccineTemplateModel MapToModel(Guid id)
        {
            return new UpdateVaccineTemplateModel
            {
                Id = id,
                VaccineName = this.VaccineName,
                ApplicationMethod = this.ApplicationMethod,
                ApplicationAge = this.ApplicationAge,
                Session = this.Session
            };
        }
    }
}
