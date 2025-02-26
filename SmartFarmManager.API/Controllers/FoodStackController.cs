using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FoodStack;
using SmartFarmManager.Service.BusinessModels.FoodStack;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
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
    }
}
