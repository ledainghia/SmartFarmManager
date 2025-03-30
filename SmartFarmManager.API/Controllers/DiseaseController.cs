using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Disease;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Disease;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiseaseController : ControllerBase
    {
        private readonly IDiseaseService _diseaseService;

        public DiseaseController(IDiseaseService diseaseService)
        {
            _diseaseService = diseaseService;
        }



     [HttpPost()]
        public async Task<IActionResult> CreateDisease([FromBody] CreateDiseaseRequest request)
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
                var result = await _diseaseService.CreateDiseaseAsync(model);

                if (!result)
                {
                    throw new Exception("Error while creating Disease!");
                }

                return Ok(ApiResult<string>.Succeed("Disease created successfully!"));
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
        public async Task<IActionResult> UpdateDisease(Guid id, [FromBody] UpdateDiseaseRequest request)
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

                var result = await _diseaseService.UpdateDiseaseAsync(id, model);

                if (!result)
                {
                    throw new Exception("Error while updating Disease!");
                }

                return Ok(ApiResult<string>.Succeed("Disease updated successfully!"));
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
        public async Task<IActionResult> DeleteDisease(Guid id)
        {
            try
            {
                var result = await _diseaseService.DeleteDiseaseAsync(id);

                if (!result)
                {
                    throw new Exception("Error while deleting Disease!");
                }

                return Ok(ApiResult<string>.Succeed("Disease deleted successfully!"));
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
        public async Task<IActionResult> GetDiseases([FromQuery] DiseaseFilterPagingRequest filterRequest)
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
                var filterModel = new DiseaseFilterModel
                {
                    KeySearch = filterRequest.KeySearch,
                    IsDeleted = filterRequest.IsDeleted,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                var result = await _diseaseService.GetDiseasesAsync(filterModel);
                return Ok(ApiResult<PagedResult<DiseaseItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiseaseDetail(Guid id)
        {
            try
            {
                var result = await _diseaseService.GetDiseaseDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Disease not found."));
                }

                return Ok(ApiResult<DiseaseDetailResponseModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }


    }
}
