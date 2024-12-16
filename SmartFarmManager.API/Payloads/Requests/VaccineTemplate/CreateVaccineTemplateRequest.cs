using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.VaccineTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.VaccineTemplate
{
    public class CreateVaccineTemplateRequest
    {
        [Required]
        public Guid TemplateId { get; set; } // ID của AnimalTemplate

        [Required]
        [MaxLength(100)]
        public string VaccineName { get; set; } // Tên vaccine

        [Required]
        [MaxLength(50)]
        public string ApplicationMethod { get; set; } // Phương pháp tiêm chủng

        public int ApplicationAge { get; set; } // Tuổi áp dụng
        [ValidSessionType]
        public int Session { get; set; } // Buổi (sáng/chiều/tối)

        public CreateVaccineTemplateModel MapToModel()
        {
            return new CreateVaccineTemplateModel
            {
                TemplateId = this.TemplateId,
                VaccineName = this.VaccineName,
                ApplicationMethod = this.ApplicationMethod,
                ApplicationAge = this.ApplicationAge,
                Session = this.Session
            };
        }
    }
}
