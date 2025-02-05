using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.User
{
    public class UpdateDeviceIdRequest
    {
        [Required]
        public string DeviceId { get; set; }
    }
}
