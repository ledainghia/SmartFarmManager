using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.AnimalTemplate
{
    public class UpdateAnimalTemplateRequest
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Species { get; set; }

        public int? DefaultCapacity { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public UpdateAnimalTemplateModel MapToModel()
        {
            return new UpdateAnimalTemplateModel
            {
                Name = this.Name,
                Species = this.Species,
                DefaultCapacity = this.DefaultCapacity,
                Notes = this.Notes
            };
        }
    }
}
