using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet("pending-tasks")]
        public async Task<IActionResult> GetStaffSortedByPendingTasksAsync([FromQuery] Guid cageId)
        {
            var result = await _staffService.GetStaffSortedByPendingTasksAsync(cageId);
            return Ok(result);
        }
    }
}
