using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.ElectricityLog;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectricityLogController : ControllerBase
    {
        private readonly IElectricityLogService _electricityLogService;

        public ElectricityLogController(IElectricityLogService electricityLogService)
        {
            _electricityLogService = electricityLogService;
        }


        [HttpGet("get-by-date")]
        public async Task<IActionResult> GetElectricityLogByDate([FromQuery] Guid farmId, [FromQuery] DateTime? date)
        {
            try
            {
                // Nếu không có ngày thì mặc định là ngày hôm nay
                var targetDate = date ?? DateTimeUtils.GetServerTimeInVietnamTime().Date;

                var log = await _electricityLogService.GetElectricityLogByDateAsync(farmId, targetDate);
                return Ok(ApiResult<ElectricityLogInDayModel>.Succeed(log));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("get-today")]
        public async Task<IActionResult> GetElectricityLogForToday([FromQuery] Guid farmId)
        {
            try
            {
                var log = await _electricityLogService.GetElectricityLogForTodayAsync(farmId);
                return Ok(ApiResult<ElectricityLogInDayModel>.Succeed(log));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("get-by-daterange")]
        public async Task<IActionResult> GetElectricityLogByDateRange([FromQuery] Guid farmId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                // Gọi service để lấy log điện trong khoảng thời gian
                var logs = await _electricityLogService.GetElectricityLogsByDateRangeAsync(farmId, startDate, endDate);
                return Ok(ApiResult<List<ElectricityLogInDayModel>>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("get-by-month")]
        public async Task<IActionResult> GetElectricityLogByMonth([FromQuery] Guid farmId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                // Gọi service để lấy log điện trong tháng và năm
                var logs = await _electricityLogService.GetElectricityLogsByMonthAsync(farmId, month, year);
                return Ok(ApiResult<ElectricityLogInMonthModel>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("get-by-year")]
        public async Task<IActionResult> GetElectricityLogsByYear([FromQuery] Guid farmId, [FromQuery] int year)
        {
            try
            {
                // Gọi service để lấy log điện trong năm
                var logs = await _electricityLogService.GetElectricityLogsByYearAsync(farmId, year);
                return Ok(ApiResult<List<ElectricityLogInMonthModel>>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }


    }
}
