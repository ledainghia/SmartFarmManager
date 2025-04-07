using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Sensor;
using SmartFarmManager.Service.BusinessModels;
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

        [HttpGet]
        public async Task<IActionResult> GetSensors([FromQuery] SensorFilterRequest filter)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();

                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
            {
                { "Errors", errors.ToArray() }
            }));
            }
            try
            {
                var filterModel = new SensorFilterModel
                {
                    
                    FarmId = filter.FarmId,
                    KeySearch = filter.KeySearch,
                    NodeId = filter.NodeId,
                    SensorTypeId = filter.SensorTypeId,
                    Status = filter.Status,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
                // Lấy danh sách cảm biến với các tham số lọc và phân trang
                var result = await _sensorService.GetSensorsAsync(filterModel);
                return Ok(ApiResult<PagedResult<SensorItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                // Trả về lỗi nếu có lỗi xảy ra trong quá trình lấy dữ liệu
                return StatusCode(500, ApiResult<string>.Fail($"An unexpected error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensor(Guid id, [FromBody] UpdateSensorRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();

                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
        {
            { "Errors", errors.ToArray() }
        }));
            }

            try
            {
                var model = request.MapToModel(); 
                var result = await _sensorService.UpdateSensorAsync(id, model);

                if (!result)
                {
                    throw new Exception("Lỗi đã xảy ra trong quá tình cập nhật !");
                }

                return Ok(ApiResult<string>.Succeed("Cảm biến được cập nhật thành công!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(Guid id)
        {
            try
            {
                var result = await _sensorService.DeleteSensorAsync(id);

                if (!result)
                {
                    throw new Exception("Lỗi xảy ra khi xóa cảm biến.");
                }

                return Ok(ApiResult<string>.Succeed("Cảm biến được xóa thành công!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }


    }
}
