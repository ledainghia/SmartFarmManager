using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.EggHarvest;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EggHarvestController : ControllerBase
    {
        private readonly IEggHarvestService _eggHarvestService;

        public EggHarvestController(IEggHarvestService eggHarvestService)
        {
            _eggHarvestService = eggHarvestService;
        }

        /// 📌 **API: Tạo EggHarvest**
        [HttpPost]
        public async Task<IActionResult> CreateEggHarvest([FromBody] CreateEggHarvestRequest request)
        {
            try
            {
                var result = await _eggHarvestService.CreateEggHarvestAsync(request);
                if (!result)
                {
                    return BadRequest(ApiResult<string>.Fail("Failed to create EggHarvest."));
                }

                return Ok(ApiResult<string>.Succeed("EggHarvest created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        /// 📌 **API: Lấy danh sách EggHarvest theo TaskId**
        [HttpGet("get-by-task")]
        public async Task<IActionResult> GetEggHarvestsByTaskId([FromQuery] Guid taskId)
        {
            try
            {
                var eggHarvests = await _eggHarvestService.GetEggHarvestsByTaskIdAsync(taskId);
                if (eggHarvests == null || !eggHarvests.Any())
                {
                    return NotFound(ApiResult<string>.Fail("No EggHarvests found for the given TaskId."));
                }

                return Ok(ApiResult<IEnumerable<EggHarvestResponse>>.Succeed(eggHarvests));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
