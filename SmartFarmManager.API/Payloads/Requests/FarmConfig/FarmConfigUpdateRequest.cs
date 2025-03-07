using SmartFarmManager.Service.BusinessModels.FarmConfig;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FarmConfig
{
    public class FarmConfigUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "MaxCagesPerStaff phải lớn hơn 0.")]
        public int? MaxCagesPerStaff { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "MaxFarmingBatchesPerCage phải lớn hơn 0.")]
        public int? MaxFarmingBatchesPerCage { get; set; }
        public FarmConfigUpdateModel MapToModel()
        {
            return new FarmConfigUpdateModel
            {
                MaxCagesPerStaff = MaxCagesPerStaff,
                MaxFarmingBatchesPerCage = MaxFarmingBatchesPerCage
            };
        }
    }
}
