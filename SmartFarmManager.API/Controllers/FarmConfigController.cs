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

        [HttpPut("{farmId}")]
        public async Task<IActionResult> UpdateFarmConfig(Guid farmId, [FromBody] FarmConfigUpdateRequest request)
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
                // Gọi Service để cập nhật Farm Config
                var result = await _farmConfigService.UpdateFarmConfigAsync(farmId, request.MapToModel());

                if (result)
                {
                    return Ok(ApiResult<string>.Succeed("Farm configuration updated successfully."));
                }
                else
                {
                    return BadRequest(ApiResult<string>.Fail("Failed to update farm configuration."));
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An error occurred while updating farm configuration."));
            }
        }

    }
}
