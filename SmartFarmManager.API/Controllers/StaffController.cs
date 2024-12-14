using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.StaffFarm;
using SmartFarmManager.Service.BusinessModels.Staff;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

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

            return Ok(ApiResult<List<StaffPendingTasksModel>>.Succeed(result));
        }
        [HttpPost("assign-staff")]
        public async Task<IActionResult> AssignStaffToCage([FromBody] AssignStaffRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            try
            {
                var (success, message) = await _staffService.AssignStaffToCageAsync(request.UserId, request.CageId);

                if (!success)
                {
                    return BadRequest(ApiResult<string>.Error(message));
                }

                return Ok(ApiResult<string>.Succeed("Staff successfully assigned to the cage."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }

}
