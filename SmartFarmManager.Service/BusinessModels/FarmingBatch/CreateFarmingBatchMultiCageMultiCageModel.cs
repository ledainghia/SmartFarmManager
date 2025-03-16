using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmingBatch
{
    public class CreateFarmingBatchMultiCageModel
    {
        public Guid TemplateId { get; set; }
        public DateTime? EstimatedTimeStart { get; set; }
        public string Name { get; set; }
        public int CleaningFrequency { get; set; }
        public int Quantity { get; set; }
        public List<CreateFarmingBatchMultiCageItemModel> FarmingBatchItems { get; set; }
    }
    public class CreateFarmingBatchMultiCageItemModel
    {
        [Required]
        public Guid CageId { get; set; }  // Mã chuồng

        [Required]
        public int Quantity { get; set; }  // Số lượng cho chuồng này
    }

}
