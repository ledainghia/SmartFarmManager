using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.GrowthStageTemplate
{
    public class UpdateGrowthStageTemplateRequest
    {
        [MaxLength(50)]
        public string? StageName { get; set; }

        public decimal? WeightAnimal { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }

        public Guid? SaleTypeId { get; set; } // Thêm SaleTypeId

        public UpdateGrowthStageTemplateModel MapToModel()
        {
            return new UpdateGrowthStageTemplateModel
            {
                StageName = this.StageName,
                WeightAnimal = this.WeightAnimal,
                AgeStart = this.AgeStart,
                AgeEnd = this.AgeEnd,
                Notes = this.Notes,
                SaleTypeId = this.SaleTypeId // Gán SaleTypeId
            };
        }
    }

}
