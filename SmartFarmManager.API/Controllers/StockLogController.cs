using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.StockLog;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockLogController : ControllerBase
    {
        private readonly IStockLogService _stockLogService;
        private readonly ILogger<StockLogController> _logger;

        public StockLogController(IStockLogService stockLogService, ILogger<StockLogController> logger)
        {
            _stockLogService = stockLogService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] StockLogRequest stockLogRequest)
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
                var result = await _stockLogService.AddStockAsync(stockLogRequest.MapToModel());
                if (!result)
                {
                    return BadRequest(ApiResult<string>.Fail("Error while adding stock"));
                }

                return Ok(ApiResult<string>.Succeed("Stock added successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
