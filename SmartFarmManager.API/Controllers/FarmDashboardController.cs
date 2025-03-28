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
        [HttpGet("task-statistics")]
        public async Task<IActionResult> GetTaskStatistics([FromQuery] Guid farmId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
                var taskStats = await _farmDashboardService.GetTaskStatisticsAsync(farmId, startDate, endDate);
                return Ok(ApiResult<TaskStatisticModel>.Succeed(taskStats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("cage-statistics")]
        public async Task<IActionResult> GetCageStatistics([FromQuery] Guid farmId)
        {
            try
            {

                var cageStats = await _farmDashboardService.GetCageStatisticsAsync(farmId);
                return Ok(ApiResult<CageStatisticModel>.Succeed(cageStats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("farmingbatch-statistics")]
        public async Task<IActionResult> GetFarmingBatchStatistics([FromQuery] Guid farmId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
                var farmingBatchStats = await _farmDashboardService.GetFarmingBatchStatisticsAsync(farmId, startDate, endDate);
                return Ok(ApiResult<FarmingBatchStatisticModel>.Succeed(farmingBatchStats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("staff-statistics")]
        public async Task<IActionResult> GetStaffStatistics([FromQuery] Guid farmId)
        {
            try
            {
                var staffStats = await _farmDashboardService.GetStaffStatisticsAsync(farmId);
                return Ok(ApiResult<StaffStatisticModel>.Succeed(staffStats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("vaccineschedule-statistics")]
        public async Task<IActionResult> GetVaccineScheduleStatistics([FromQuery] Guid farmId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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
                var vaccineScheduleStats = await _farmDashboardService.GetVaccineScheduleStatisticsAsync(farmId, startDate, endDate);
                return Ok(ApiResult<VaccineScheduleStatisticModel>.Succeed(vaccineScheduleStats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }






    }
}
