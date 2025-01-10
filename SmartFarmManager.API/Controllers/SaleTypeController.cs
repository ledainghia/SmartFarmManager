using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.SaleType;
using SmartFarmManager.Service.BusinessModels.SaleType;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class SaleTypeController : ControllerBase
    {
        private readonly ISaleTypeService _saleTypeService;

        public SaleTypeController(ISaleTypeService saleTypeService)
        {
            _saleTypeService = saleTypeService;
        }
        [HttpGet]
        public async Task<IActionResult> GetSaleTypes([FromQuery] SaleTypeFilterPagingRequest filterRequest)
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
                // Map request sang service model
                var serviceFilter = new SaleTypeFilterModel
                {
                    StageTypeName = filterRequest.StageTypeName,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                // Gọi Service xử lý
                var result = await _saleTypeService.GetFilteredSaleTypesAsync(serviceFilter);

                // Trả về kết quả
                return Ok(ApiResult<PagedResult<SaleTypeItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }


    }
}
