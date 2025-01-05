using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.AnimalTemplate
{
    public class CreateAnimalTemplateRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Species { get; set; }

        public int? DefaultCapacity { get; set; }

        [MaxLength(255)]
        public string Notes { get; set; }

        public CreateAnimalTemplateModel MapToModel()
        {
            return new CreateAnimalTemplateModel
            {
                Name = this.Name,
                Species = this.Species,
                Notes = this.Notes
            };
        }
    }
}
