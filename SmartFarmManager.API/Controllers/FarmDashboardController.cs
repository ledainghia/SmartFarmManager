using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Dashboard;
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

        [HttpGet("{farmId:Guid}")]
        public async Task<IActionResult> GetFarmDashboardOverview( Guid farmId)
        {
            try
            {
                // Get the Dashboard Statistics data for the farm
                var dashboardData = await _farmDashboardService.GetFarmDashboardStatisticsAsync(farmId);
                return Ok(ApiResult<DashboardStatisticsModel>.Succeed(dashboardData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
