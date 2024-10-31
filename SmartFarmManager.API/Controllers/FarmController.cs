using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.User;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class FarmController : ControllerBase
    {
        private readonly IFarmService _farmService;

        public FarmController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        // Implement API endpoints for Farm operations here
        #region api get users staff of farm
        [HttpGet("{farmId}/users")]
        public async Task<IActionResult> GetUsersByFarmId(int farmId)
        {
            try
            {
                var users = await _farmService.GetUsersByFarmIdAsync(farmId);
                return Ok(ApiResult<List<UserResponseModel>>.Succeed(users));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        #endregion
    }
}
