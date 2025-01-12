using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FoodTemplate;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class FoodTemplateController : ControllerBase
    {
        private readonly IFoodTemplateService _foodTemplateService;

        public FoodTemplateController(IFoodTemplateService foodTemplateService)
        {
            _foodTemplateService = foodTemplateService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateFoodTemplate([FromBody] CreateFoodTemplateRequest request)
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
                var model = request.MapToModel();
                var result = await _foodTemplateService.CreateFoodTemplateAsync(model);

                if (!result)
                {
                    throw new Exception("Error while creating Food Template!");
                }

                return Ok(ApiResult<string>.Succeed("Food Template created successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodTemplate(Guid id, [FromBody] UpdateFoodTemplateRequest request)
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
                var model = request.MapToModel(id);
                var result = await _foodTemplateService.UpdateFoodTemplateAsync(model);

                if (!result)
                {
                    return NotFound(ApiResult<string>.Fail("Food Template not found."));
                }

                return Ok(ApiResult<string>.Succeed("Food Template updated successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpDelete("food-template/{id}")]
        public async Task<IActionResult> DeleteFoodTemplate(Guid id)
        {
            try
            {
                var result = await _foodTemplateService.DeleteFoodTemplateAsync(id);

                if (!result)
                {
                    return NotFound(ApiResult<string>.Fail("Food Template not found."));
                }

                return Ok(ApiResult<string>.Succeed("Food Template deleted successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("food-templates")]
        public async Task<IActionResult> GetFoodTemplates([FromQuery] FoodTemplateFilterPagingRequest filterRequest)
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
                var filterModel = new FoodTemplateFilterModel
                {
                    StageTemplateId = filterRequest.StageTemplateId,
                    FoodType = filterRequest.FoodType,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                var result = await _foodTemplateService.GetFoodTemplatesAsync(filterModel);
                return Ok(ApiResult<PagedResult<FoodTemplateItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodTemplateDetail(Guid id)
        {
            try
            {
                // Gọi service để lấy chi tiết Food Template
                var result = await _foodTemplateService.GetFoodTemplateDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Food Template not found."));
                }

                return Ok(ApiResult<FoodTemplateDetailModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }


    }
}
