using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FarmConfig
{
    public class UpdateTimeDifferenceRequest
    {
        [Required]
        public Guid FarmId { get; set; }

        [Required]
        public DateTime NewTime { get; set; }
    }
}
