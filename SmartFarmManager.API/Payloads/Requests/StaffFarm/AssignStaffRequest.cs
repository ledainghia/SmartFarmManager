using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.StaffFarm
{
    public class AssignStaffRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CageId { get; set; }
    }

}
