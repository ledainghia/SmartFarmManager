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

        [HttpPost("{cageId:guid}")]
        public async Task<IActionResult> CreateDailyFoodUsageLog(Guid cageId, [FromBody] CreateDailyFoodUsageLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<string>.Fail("Yêu cầu không hợp lệ"));

            var result = await _dailyFoodUsageLogService.CreateDailyFoodUsageLogAsync(cageId, new DailyFoodUsageLogModel
            {
                RecommendedWeight = request.RecommendedWeight,
                ActualWeight = request.ActualWeight,
                Notes = request.Notes,
                Photo = request.Photo
            });

            if (result == null)
                return NotFound(ApiResult<string>.Fail("Không tìm thấy GrowthStage tương ứng"));

            return Created("", ApiResult<Guid?>.Succeed(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDailyFoodUsageLogById(Guid id)
        {
            var log = await _dailyFoodUsageLogService.GetDailyFoodUsageLogByIdAsync(id);
            if (log == null)
                return NotFound(ApiResult<string>.Fail("Không tìm thấy log cho ăn hàng ngày"));

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
