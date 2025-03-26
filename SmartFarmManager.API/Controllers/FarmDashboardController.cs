using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Dashboard;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmDashboardController : ControllerBase
    {
        private readonly IFarmDashboardService _farmDashboardService;

        public FarmDashboardController(IFarmDashboardService farmDashboardService)
        {
            _farmDashboardService = farmDashboardService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetFarmDashboardOverview([FromQuery] Guid farmId,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate)
        {
            try
            {
                if (!startDate.HasValue)
                {
                    startDate = DateTimeUtils.GetServerTimeInVietnamTime().AddYears(-1);  // 1 năm trước ngày hiện tại
                }

                // Nếu không có endDate, gán endDate là ngày hiện tại
                if (!endDate.HasValue)
                {
                    endDate = DateTimeUtils.GetServerTimeInVietnamTime();  // Ngày hiện tại
                }
                // Get the Dashboard Statistics data for the farm
                var dashboardData = await _farmDashboardService.GetFarmDashboardStatisticsAsync(farmId, startDate, endDate);
                return Ok(ApiResult<DashboardStatisticsModel>.Succeed(dashboardData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
