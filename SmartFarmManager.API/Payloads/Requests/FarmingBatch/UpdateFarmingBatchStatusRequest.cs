using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FarmingBatch
{
    public class UpdateFarmingBatchStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string NewStatus { get; set; }
    }
}
