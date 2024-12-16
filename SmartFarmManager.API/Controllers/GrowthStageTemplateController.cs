using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrowthStageTemplateController : ControllerBase
    {
        private readonly IGrowthStageTemplateService _growthStageTemplateService;

        public GrowthStageTemplateController(IGrowthStageTemplateService growthStageTemplateService)
        {
            _growthStageTemplateService = growthStageTemplateService;
        }

        [HttpPost()]
        public async Task<IActionResult> CreateGrowthStageTemplate([FromBody] CreateGrowthStageTemplateRequest request)
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
                var result = await _growthStageTemplateService.CreateGrowthStageTemplateAsync(model);

                if (!result)
                {
                    throw new Exception("Error while creating Growth Stage Template!");
                }

                return Ok(ApiResult<string>.Succeed("Growth Stage Template created successfully!"));
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
        public async Task<IActionResult> UpdateGrowthStageTemplate(Guid id, [FromBody] UpdateGrowthStageTemplateRequest request)
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
                var model = request.MapToModel();

                // Gọi service với id từ route
                var result = await _growthStageTemplateService.UpdateGrowthStageTemplateAsync(id, model);

                if (!result)
                {
                    throw new Exception("Error while updating Growth Stage Template!");
                }

                return Ok(ApiResult<string>.Succeed("Growth Stage Template updated successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrowthStageTemplate(Guid id)
        {


            try
            {
                var result = await _growthStageTemplateService.DeleteGrowthStageTemplateAsync(id);

                if (!result)
                {
                    throw new Exception("Error while deleting Growth Stage Template!");
                }

                return Ok(ApiResult<string>.Succeed("Growth Stage Template deleted successfully!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> GetGrowthStageTemplates([FromQuery] GrowthStageTemplateFilterPagingRequest filterRequest)
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
                var filterModel = new GrowthStageTemplateFilterModel
                {
                    TemplateId = filterRequest.TemplateId,
                    StageName = filterRequest.StageName,
                    AgeStart = filterRequest.AgeStart,
                    AgeEnd = filterRequest.AgeEnd,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };
                var result = await _growthStageTemplateService.GetGrowthStageTemplatesAsync(filterModel);
                return Ok(ApiResult<PagedResult<GrowthStageTemplateItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrowthStageTemplateDetail(Guid id)
        {
            try
            {
                var result = await _growthStageTemplateService.GetGrowthStageTemplateDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Growth Stage Template not found."));
                }

                return Ok(ApiResult<GrowthStageTemplateDetailResponseModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }



    }
}
