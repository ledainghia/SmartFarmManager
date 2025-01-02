using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.HealthLog;
using SmartFarmManager.API.Payloads.Responses.HealthLog;
using SmartFarmManager.Service.BusinessModels.HealthLog;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthLogController : ControllerBase
    {
        private readonly IHealthLogService _healthLogService;

        public HealthLogController(IHealthLogService healthLogService)
        {
            _healthLogService = healthLogService;
        }

        // POST: api/healthlogs
        [HttpPost("{cageId:guid}/health-log")]
        public async Task<IActionResult> CreateHealthLog(Guid cageId, [FromBody] CreateHealthLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<string>.Fail("Yêu cầu không hợp lệ"));

            var result = await _healthLogService.CreateHealthLogAsync(cageId, new HealthLogModel
            {
                Date = request.Date,
                Notes = request.Notes,
                Photo = request.Photo,
                TaskId = request.TaskId
            });

            if (result == null)
                return NotFound(ApiResult<string>.Fail("Không tìm thấy đơn thuốc tương ứng"));

            return Created("", ApiResult<Guid?>.Succeed(result));
        }


        // GET: api/healthlogs/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetHealthLogById(Guid id)
        {
            var healthLog = await _healthLogService.GetHealthLogByIdAsync(id);
            if (healthLog == null)
                return NotFound(ApiResult<string>.Fail("Health log not found"));

            var response = new HealthLogResponse
            {
                Id = healthLog.Id,
                PrescriptionId = healthLog.PrescriptionId.Value,
                Date = healthLog.Date.Value,
                Notes = healthLog.Notes,
                Photo = healthLog.Photo,
                TaskId = healthLog.TaskId
            };

            return Ok(ApiResult<HealthLogResponse>.Succeed(response));
        }

        // GET: api/healthlogs
        [HttpGet]
        public async Task<IActionResult> GetHealthLogs([FromQuery] Guid? prescriptionId)
        {
            var healthLogs = await _healthLogService.GetHealthLogsAsync(prescriptionId);

            var responses = healthLogs.Select(hl => new HealthLogResponse
            {
                Id = hl.Id,
                PrescriptionId = hl.PrescriptionId.Value,
                Date = hl.Date.Value,
                Notes = hl.Notes,
                Photo = hl.Photo,
                TaskId = hl.TaskId
            });

            return Ok(ApiResult<IEnumerable<HealthLogResponse>>.Succeed(responses));
        }
        [HttpGet("task/{taskId:guid}")]
        public async Task<IActionResult> GetHealthLogByTaskId(Guid taskId)
        {
            var healthLog = await _healthLogService.GetHealthLogByTaskIdAsync(taskId);

            if (healthLog == null)
                return NotFound(ApiResult<string>.Fail("Health log not found for the given TaskId"));

            var response = new HealthLogResponse
            {
                Id = healthLog.Id,
                PrescriptionId = healthLog.PrescriptionId.Value,
                Date = healthLog.Date.Value,
                Notes = healthLog.Notes,
                Photo = healthLog.Photo,
                TaskId = healthLog.TaskId
            };

            return Ok(ApiResult<HealthLogResponse>.Succeed(response));
        }

    }
}
