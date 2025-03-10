using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.ElectricityLog;
using SmartFarmManager.Service.BusinessModels.WaterLog;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaterLogController : ControllerBase
    {
        private readonly IWaterLogService _waterLogService;

        public WaterLogController(IWaterLogService waterLogService)
        {
            _waterLogService = waterLogService;
        }

        [HttpGet("get-by-day")]
        public async Task<IActionResult> GetWaterLogInDay([FromQuery] Guid farmId, [FromQuery] DateTime date)
        {
            try
            {
                var result = await _waterLogService.GetWaterLogInDayAsync(farmId, date);
                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("No data found for the selected date."));
                }

                return Ok(ApiResult<WaterLogInDayModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("get-by-daterange")]
        public async Task<IActionResult> GetWaterLogByDateRange([FromQuery] Guid farmId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // Gọi service để lấy log điện trong khoảng thời gian
                var logs = await _waterLogService.GetWaterLogsByDateRangeAsync(farmId, startDate, endDate);
                return Ok(ApiResult<List<WaterLogInDayModel>>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("get-by-month")]
        public async Task<IActionResult> GetWaterLogByMonth([FromQuery] Guid farmId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                // Gọi service để lấy log điện trong tháng và năm
                var logs = await _waterLogService.GetWaterLogsByMonthAsync(farmId, month, year);
                return Ok(ApiResult<WaterLogInMonthModel>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("get-by-year")]
        public async Task<IActionResult> GetWaterLogsByYear([FromQuery] Guid farmId, [FromQuery] int year)
        {
            try
            {
                // Gọi service để lấy log điện trong năm
                var logs = await _waterLogService.GetWaterLogsByYearAsync(farmId, year);
                return Ok(ApiResult<List<WaterLogInMonthModel>>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
