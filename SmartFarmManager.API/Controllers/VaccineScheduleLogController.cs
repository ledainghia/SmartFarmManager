using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.VaccineScheduleLog;
using SmartFarmManager.API.Payloads.Responses.VaccineScheduleLog;
using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineScheduleLogController : ControllerBase
    {
        private readonly IVaccineScheduleLogService _vaccineScheduleLogService;

        public VaccineScheduleLogController(IVaccineScheduleLogService service)
        {
            _vaccineScheduleLogService = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVaccineScheduleLog([FromBody] CreateVaccineScheduleLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<string>.Fail("Invalid request"));

            var id = await _vaccineScheduleLogService.CreateVaccineScheduleLogAsync(new VaccineScheduleLogModel
            {
                ScheduleId = request.ScheduleId,
                Date = request.Date,
                Notes = request.Notes,
                Photo = request.Photo,
                TaskId = request.TaskId
            });

            return CreatedAtAction(nameof(GetVaccineScheduleLogById), new { id }, ApiResult<Guid>.Succeed(id));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVaccineScheduleLogById(Guid id)
        {
            var log = await _vaccineScheduleLogService.GetVaccineScheduleLogByIdAsync(id);
            if (log == null)
                return NotFound(ApiResult<string>.Fail("Vaccine schedule log not found"));

            var response = new VaccineScheduleLogResponse
            {
                Id = log.Id,
                ScheduleId = log.ScheduleId,
                Date = log.Date,
                Notes = log.Notes,
                Photo = log.Photo,
                TaskId = log.TaskId
            };

            return Ok(ApiResult<VaccineScheduleLogResponse>.Succeed(response));
        }
    }
}
