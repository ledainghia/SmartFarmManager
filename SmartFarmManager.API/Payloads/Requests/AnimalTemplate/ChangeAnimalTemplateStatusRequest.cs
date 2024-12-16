using SmartFarmManager.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.AnimalTemplate
{
    public class ChangeAnimalTemplateStatusRequest
    {
        [Required]
        [AnimalTemplateStatusValidator]
        public string Status { get; set; }
    }
}
