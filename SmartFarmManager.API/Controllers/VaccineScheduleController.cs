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
        private readonly ITaskService _taskService;

        public VaccineScheduleController(IVaccineScheduleService vaccineScheduleService, ITaskService taskService)
        {
            _vaccineScheduleService = vaccineScheduleService;
            _taskService = taskService;
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

        /// <summary>
        /// Redo Vaccine Schedule (Chỉnh sửa lịch tiêm vắc xin bị Missed)
        /// </summary>
        /// <param name="request">Thông tin lịch tiêm cần redo</param>
        /// <returns>ApiResult<string> thông báo kết quả</returns>
        [HttpPost("redo")]
        public async Task<IActionResult> RedoVaccineSchedule([FromBody] RedoVaccineScheduleRequest request)
        {
            try
            {
                if (request == null || request.VaccineScheduleId == Guid.Empty || request.Date == default)
                {
                    return BadRequest(ApiResult<string>.Fail("Invalid request data."));
                }

                bool success = await _taskService.RedoVaccineScheduleAsync(request);

                if (!success)
                    return BadRequest(ApiResult<string>.Fail("Failed to redo vaccine schedule. Check request details."));

                return Ok(ApiResult<string>.Succeed("Vaccine schedule successfully redone."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
