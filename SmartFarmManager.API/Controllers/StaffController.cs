using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Staff;
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
            if (cageId == Guid.Empty)
            {
                return BadRequest(ApiResult<object>.Fail("CageId is required."));
            }

            var result = await _staffService.GetStaffSortedByPendingTasksAsync(cageId);

            if (result == null || !result.Any())
            {
                return NotFound(ApiResult<object>.Fail("No staff found for the given CageId."));
            }

            return Ok(ApiResult<IEnumerable<StaffPendingTasksModel>>.Succeed(result));
        }

    }
}
