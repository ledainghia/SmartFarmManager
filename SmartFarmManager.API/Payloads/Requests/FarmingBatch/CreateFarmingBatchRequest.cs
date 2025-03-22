using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FarmingBatch
{
    public class CreateFarmingBatchRequest
    {
        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        public Guid CageId { get; set; }
        [Required]
        public DateTime? EstimatedTimeStart { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int CleaningFrequency { get; set; }

        [Required]
        public int Quantity { get; set; }

        public CreateFarmingBatchModel MapToModel()
        {
            return new CreateFarmingBatchModel
            {
                TemplateId = this.TemplateId,
                CageId = this.CageId,
                Name = this.Name,
                CleaningFrequency = this.CleaningFrequency,
                Quantity = this.Quantity,
                EstimatedTimeStart = this.EstimatedTimeStart,
            };
        }
    }

}
