using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineScheduleController : ControllerBase
    {
        private readonly IVaccineScheduleService _vaccineScheduleService;

        public VaccineScheduleController(IVaccineScheduleService vaccineScheduleService)
        {
            _vaccineScheduleService = vaccineScheduleService;
        }

        [HttpGet("vaccine-schedules")]
        public async Task<IActionResult> GetVaccineSchedules([FromQuery] Guid? stageId, [FromQuery] DateTime? date, [FromQuery] string status)
        {
            try
            {
                var result = await _vaccineScheduleService.GetVaccineSchedulesAsync(stageId, date, status);
                return Ok(ApiResult<List<VaccineScheduleResponse>>.Succeed(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message)); // Trả về lỗi Conflict nếu trùng lặp
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }

    }
}
