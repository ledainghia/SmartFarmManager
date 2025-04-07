using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.VaccineSchedule;
using SmartFarmManager.Service.BusinessModels;
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

        [HttpPost("")]
        public async Task<IActionResult> CreateVaccineSchedule([FromBody] CreateVaccineScheduleRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
            {
                { "Errors", errors.ToArray() }
            }));
            }

            try
            {
                var model = request.MapToModel();
                var result = await _vaccineScheduleService.CreateVaccineScheduleAsync(model);

                if (!result)
                {
                    throw new Exception("Error while creating Vaccine Schedule!");
                }

                return Ok(ApiResult<string>.Succeed("Vaccine Schedule created successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaccineScheduleById(Guid id)
        {
            try
            {
                var vaccineSchedule = await _vaccineScheduleService.GetVaccineScheduleByIdAsync(id);
                if (vaccineSchedule == null)
                {
                    return NotFound(ApiResult<string>.Fail("Vaccine Schedule not found"));
                }

                return Ok(ApiResult<VaccineScheduleResponse>.Succeed(vaccineSchedule));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("get-by-task")]
        public async Task<IActionResult> GetVaccineScheduleByTaskId([FromQuery] Guid taskId)
        {
            try
            {
                var vaccineSchedule = await _vaccineScheduleService.GetVaccineScheduleByTaskIdAsync(taskId);
                if (vaccineSchedule == null)
                {
                    return NotFound(ApiResult<string>.Fail("Vaccine Schedule not found for the given TaskId"));
                }

                return Ok(ApiResult<VaccineScheduleWithLogsResponse>.Succeed(vaccineSchedule));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> GetVaccineSchedules([FromQuery] VaccineScheduleFilterPagingRequest filterRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
            {
                { "Errors", errors.ToArray() }
            }));
            }

            try
            {
                var filterModel = new VaccineScheduleFilterKeySearchModel
                {
                    KeySearch = filterRequest.KeySearch,
                    VaccineId = filterRequest.VaccineId,
                    StageId = filterRequest.StageId,
                    Date = filterRequest.Date,
                    Status = filterRequest.Status,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                var result = await _vaccineScheduleService.GetVaccineSchedulesAsync(filterModel);
                return Ok(ApiResult<PagedResult<VaccineScheduleItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

    }
}
