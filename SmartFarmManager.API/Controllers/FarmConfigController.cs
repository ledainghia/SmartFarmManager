using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FarmConfig;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmConfigController : ControllerBase
    {
        private readonly IFarmConfigService _farmConfigService;

        public FarmConfigController(IFarmConfigService farmConfigService)
        {
            _farmConfigService = farmConfigService;
        }

        [HttpPost("update-time-difference")]
        public async Task<IActionResult> UpdateTimeDifference([FromBody] UpdateTimeDifferenceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<string>.Fail("Invalid request."));
            }

            try
            {
                await _farmConfigService.UpdateFarmTimeDifferenceAsync(request.FarmId, request.NewTime);
                return Ok(ApiResult<string>.Succeed("Time difference updated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpPost("reset-time")]
        public async Task<IActionResult> ResetTime([FromQuery] Guid farmId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<string>.Fail("Invalid request."));
            }

            try
            {
                await _farmConfigService.ResetTimeDifferenceAsync(farmId);
                return Ok(ApiResult<string>.Succeed("Time difference updated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

    }
}
