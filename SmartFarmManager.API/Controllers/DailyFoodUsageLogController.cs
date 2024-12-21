using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.DailyFoodUsageLog;
using SmartFarmManager.API.Payloads.Responses.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyFoodUsageLogController : ControllerBase
    {
        private readonly IDailyFoodUsageLogService _dailyFoodUsageLogService;

        public DailyFoodUsageLogController(IDailyFoodUsageLogService service)
        {
            _dailyFoodUsageLogService = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDailyFoodUsageLog([FromBody] CreateDailyFoodUsageLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<string>.Fail("Invalid request"));

            var id = await _dailyFoodUsageLogService.CreateDailyFoodUsageLogAsync(new DailyFoodUsageLogModel
            {
                StageId = request.StageId,
                RecommendedWeight = request.RecommendedWeight,
                ActualWeight = request.ActualWeight,
                Notes = request.Notes,
                LogTime = request.LogTime,
                Photo = request.Photo,
                TaskId = request.TaskId
            });

            return CreatedAtAction(nameof(GetDailyFoodUsageLogById), new { id }, ApiResult<Guid>.Succeed(id));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDailyFoodUsageLogById(Guid id)
        {
            var log = await _dailyFoodUsageLogService.GetDailyFoodUsageLogByIdAsync(id);
            if (log == null)
                return NotFound(ApiResult<string>.Fail("Daily food usage log not found"));

            var response = new DailyFoodUsageLogResponse
            {
                Id = log.Id,
                StageId = log.StageId,
                RecommendedWeight = log.RecommendedWeight,
                ActualWeight = log.ActualWeight,
                Notes = log.Notes,
                LogTime = log.LogTime,
                Photo = log.Photo,
                TaskId = log.TaskId
            };

            return Ok(ApiResult<DailyFoodUsageLogResponse>.Succeed(response));
        }
    }
}
