using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.GrowthStageTemplate
{
    public class CreateGrowthStageTemplateRequest
    {

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StageName { get; set; }

        public decimal? WeightAnimal { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public CreateGrowthStageTemplateModel MapToModel()
        {
            return new CreateGrowthStageTemplateModel
            {
                TemplateId = this.TemplateId,
                StageName = this.StageName,
                WeightAnimal = this.WeightAnimal,
                AgeStart = this.AgeStart,
                AgeEnd = this.AgeEnd,
                Notes = this.Notes
            };
        }
    }
}
