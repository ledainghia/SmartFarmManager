using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class AnimalTemplateController : ControllerBase
    {
        private readonly IAnimalTemplateService _animalTemplateService;

        public AnimalTemplateController(IAnimalTemplateService animalTemplateService)
        {
            _animalTemplateService = animalTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredAnimalTemplates([FromQuery] AnimalTemplateFilterPagingRequest filterRequest)
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
                var serviceFilter = new AnimalTemplateFilterModel
                {
                    Name = filterRequest.Name,
                    Species = filterRequest.Species,
                    Status = filterRequest.Status,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                // Gọi Service xử lý
                var result = await _animalTemplateService.GetFilteredAnimalTemplatesAsync(serviceFilter);

                // Trả về kết quả
                return Ok(ApiResult<PagedResult<AnimalTemplateItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnimalTemplateDetail(Guid id)
        {
            try
            {
                var result = await _animalTemplateService.GetAnimalTemplateDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Animal Template not found."));
                }

                return Ok(ApiResult<AnimalTemplateDetailResponseModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }



        [HttpPost("")]
        public async Task<IActionResult> CreateAnimalTemplate([FromBody] CreateAnimalTemplateRequest request)
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
                var result = await _animalTemplateService.CreateAnimalTemplateAsync(model);

                if (!result)
                {
                    throw new Exception("Error while saving Animal Template!");
                }

                return Ok(ApiResult<string>.Succeed("Animal Template created successfully!"));
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
        public async Task<IActionResult> UpdateAnimalTemplate(Guid id, [FromBody] UpdateAnimalTemplateRequest request)
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
                var result = await _animalTemplateService.UpdateAnimalTemplateAsync(id, model);

                if (!result)
                {
                    throw new Exception("Error while updating Animal Template!");
                }

                return Ok(ApiResult<string>.Succeed("Animal Template updated successfully!"));
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

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeAnimalTemplateStatusRequest request)
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
                var result = await _animalTemplateService.ChangeStatusAsync(id, request.Status);

                if (!result)
                {
                    throw new Exception("Error while changing Animal Template status!");
                }

                return Ok(ApiResult<string>.Succeed("Animal Template status updated successfully!"));
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
        public async Task<IActionResult> DeleteAnimalTemplate(Guid id)
        {
            try
            {
                var result = await _animalTemplateService.DeleteAnimalTemplateAsync(id);

                if (!result)
                {
                    throw new Exception("Error while deleting Animal Template!");
                }

                return Ok(ApiResult<string>.Succeed("Animal Template deleted successfully!"));
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

    }
}
