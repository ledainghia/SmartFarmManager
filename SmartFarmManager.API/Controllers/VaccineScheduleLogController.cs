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

        [HttpPost("{cageId:guid}")]
        public async Task<IActionResult> CreateVaccineScheduleLog(Guid cageId, [FromBody] CreateVaccineScheduleLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<string>.Fail("Yêu cầu không hợp lệ"));

            var result = await _vaccineScheduleLogService.CreateVaccineScheduleLogAsync(cageId, new VaccineScheduleLogModel
            {
                Notes = request.Notes,
                Photo = request.Photo
            });

            if (result == null)
                return NotFound(ApiResult<string>.Fail("Không tìm thấy VaccineSchedule tương ứng"));

            return Created("", ApiResult<Guid?>.Succeed(result));
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVaccineScheduleLogById(Guid id)
        {
            var log = await _vaccineScheduleLogService.GetVaccineScheduleLogByIdAsync(id);
            if (log == null)
                return NotFound(ApiResult<string>.Fail("Không tìm thấy log tiêm vắc-xin"));

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
