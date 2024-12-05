using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Cages;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using Sprache;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class CageController : ControllerBase
    {
        private readonly ICageService _cageService;
public CageController(ICageService cageService)
        {
            _cageService = cageService;
        }
        /// <summary>
        /// Get all cages with filter and pagination
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated list of cages</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CageFilterPagingRequest request)
        {
            try
            {
                var cages = await _cageService.GetCagesAsync(request.MapToModel());
                return Ok(ApiResult<PagedResult<CageResponseModel>>.Succeed(cages));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }


        }

        }
}
