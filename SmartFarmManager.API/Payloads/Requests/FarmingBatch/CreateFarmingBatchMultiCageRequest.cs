using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FarmingBatch
{
    public class CreateFarmingBatchMultiCageRequest
    {

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        public DateTime? EstimatedTimeStart { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int CleaningFrequency { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public List<CreateFarmingBatchMultiCageItem> FarmingBatchItems { get; set; }


        public CreateFarmingBatchMultiCageModel MapToModel()
        {
            return new CreateFarmingBatchMultiCageModel
            {
                TemplateId = this.TemplateId,
                EstimatedTimeStart = this.EstimatedTimeStart,
                Name = this.Name,
                CleaningFrequency = this.CleaningFrequency,
                Quantity = this.Quantity,
                FarmingBatchItems = this.FarmingBatchItems.Select(x => new CreateFarmingBatchMultiCageItemModel
                {
                    CageId = x.CageId,
                    Quantity = x.Quantity
                }).ToList()
            };
        }
    }

    public class CreateFarmingBatchMultiCageItem
    {
        [Required]
        public Guid CageId { get; set; }  // Mã chuồng

        [Required]
        public int Quantity { get; set; }  // Số lượng cho chuồng này
    }
}
