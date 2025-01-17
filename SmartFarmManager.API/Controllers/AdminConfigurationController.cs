using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Configuration;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminConfigurationController : ControllerBase
    {
        private readonly SystemConfigurationService _configurationService;

        public AdminConfigurationController()
        {
            _configurationService = new SystemConfigurationService();
        }

        // Lấy cấu hình hiện tại
        [HttpGet("get-config")]
        public async Task<IActionResult> GetConfiguration()
        {
            var config = await _configurationService.GetConfigurationAsync();
            return Ok(new { Success = true, Data = config });
        }

        // Cập nhật cấu hình
        [HttpPost("update-config")]
        public async Task<IActionResult> UpdateConfiguration([FromBody] SystemConfiguration request)
        {
            if (request.MaxFarmingBatchPerCage <= 0)
            {
                return BadRequest(new { Success = false, Message = "Số lần tạo vụ nuôi tối đa phải lớn hơn 0." });
            }

            await _configurationService.UpdateConfigurationAsync(request);
            return Ok(new { Success = true, Message = "Cập nhật cấu hình thành công." });
        }
    }
}
