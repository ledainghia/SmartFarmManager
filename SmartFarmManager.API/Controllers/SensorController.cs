using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Sensor;
using SmartFarmManager.Service.BusinessModels.SensorDataLog;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensorController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        [HttpGet("get-by-sensor-id")]
        public async Task<IActionResult> GetSensorDataBySensorId([FromQuery] Guid sensorId, [FromQuery] DateTime? date)
        {
            try
            {
                var targetDate = date ?? DateTimeUtils.GetServerTimeInVietnamTime().Date;
                var sensorData = await _sensorService.GetSensorDataBySensorIdAsync(sensorId, targetDate);
                return Ok(ApiResult<List<SensorDataInDayModel>>.Succeed(sensorData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        
        }

        [HttpGet("get-by-sensor-id-range")]
        public async Task<IActionResult> GetSensorDataBySensorIdRange([FromQuery] Guid sensorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var sensorData = await _sensorService.GetSensorDataBySensorIdRangeAsync(sensorId, startDate, endDate);
                return Ok(ApiResult<List<SensorDataInDayModel>>.Succeed(sensorData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("get-by-sensor-id-month")]
        public async Task<IActionResult> GetSensorDataBySensorIdMonth([FromQuery] Guid sensorId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var sensorData = await _sensorService.GetSensorDataBySensorIdMonthAsync(sensorId, month, year);
                return Ok(ApiResult<List<SensorDataInMonthModel>>.Succeed(sensorData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("get-by-sensor-id-year")]
        public async Task<IActionResult> GetSensorDataBySensorIdYear([FromQuery] Guid sensorId, [FromQuery] int year)
        {
            try
            {
                var sensorData = await _sensorService.GetSensorDataBySensorIdYearAsync(sensorId, year);
                return Ok(ApiResult<List<SensorDataInMonthModel>>.Succeed(sensorData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("get-by-cage-id")]
        public async Task<IActionResult> GetSensorsByCageId([FromQuery] Guid cageId)
        {
            try
            {
                var sensorData = await _sensorService.GetSensorsByCageIdAsync(cageId);
                return Ok(ApiResult<List<SensorGroupByNodeModel>>.Succeed(sensorData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

    }
}
