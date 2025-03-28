using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FoodStack;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.FoodStack;
using SmartFarmManager.Service.BusinessModels.StockLog;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/foodstock")]
    [ApiController]
    public class FoodStackController : ControllerBase
    {
        private readonly IFoodStackService _foodStackService;

        public FoodStackController(IFoodStackService foodStackService)
        {
            _foodStackService = foodStackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFoodStack([FromBody] FoodStackCreateRequest foodStackCreateRequest)
        {
            if (!ModelState.IsValid)
            {
                // Collect validation errors
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
                var result = await _foodStackService.CreateFoodStackAsync(foodStackCreateRequest.MapToModel());

                if (result)
                {
                    return Ok(ApiResult<string>.Succeed("Food stack created successfully!"));
                }

                return BadRequest(ApiResult<string>.Fail("Failed to create Food stack."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An error occurred while creating Food stack: " + ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodStack(Guid id, [FromBody] UpdateFoodStackRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                   .Select(e => e.ErrorMessage)
                                                   .ToList();

                    return BadRequest(new { Errors = errors });
                }

                var result = await _foodStackService.UpdateFoodStackAsync(id, request.MapToModel());

                if (!result)
                {
                    return NotFound(new { Message = "FoodStock not found or could not be updated." });
                }

                return Ok(new { Message = "FoodStack updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodStack(Guid id)
        {
            try
            {
                var result = await _foodStackService.DeleteFoodStackAsync(id);

                if (!result)
                {
                    return NotFound(new { Message = "FoodStack not found or could not be deleted." });
                }

                return Ok(new { Message = "FoodStack deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetFoodStacks([FromQuery] FoodStackFilterPagingRequest filterRequest)
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
                var filterModel = new FoodStackFilterModel
                {
                    FarmId = filterRequest.FarmId,
                    FoodType = filterRequest.FoodType,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                var result = await _foodStackService.GetFoodStacksAsync(filterModel);
                return Ok(ApiResult<PagedResult<FoodStackItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStockLogHistory(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize=20)
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
                var result = await _foodStackService.GetStockLogHistoryAsync(id, pageNumber,pageSize);
                return Ok(ApiResult<PagedResult<StockLogItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }


    }
}
