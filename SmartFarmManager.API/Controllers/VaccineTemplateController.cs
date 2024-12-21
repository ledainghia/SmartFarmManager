using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.VaccineTemplate;
using SmartFarmManager.Service.BusinessModels.VaccineTemplate;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class VaccineTemplateController : ControllerBase
    {
        private readonly IVaccineTemplateService _vaccineTemplateService;

        public VaccineTemplateController(IVaccineTemplateService vaccineTemplateService)
        {
            _vaccineTemplateService = vaccineTemplateService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateVaccineTemplate([FromBody] CreateVaccineTemplateRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Collect validation errors
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
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
                var result = await _vaccineTemplateService.CreateVaccineTemplateAsync(model);

                if (!result)
                {
                    // Xử lý lỗi cụ thể nếu service trả về false
                    return BadRequest(ApiResult<string>.Fail("Failed to create Vaccine Template. Please try again."));
                }

                return Ok(ApiResult<string>.Succeed("Vaccine Template created successfully!"));
            }
            catch (ArgumentException ex)
            {
                // Log lỗi để dễ dàng debug
                _logger.LogWarning(ex, "Validation error while creating Vaccine Template");
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                // Log lỗi server
                _logger.LogError(ex, "Unexpected error while creating Vaccine Template");
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVaccineTemplate(Guid id, [FromBody] UpdateVaccineTemplateRequest request)
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
                var result = await _vaccineTemplateService.UpdateVaccineTemplateAsync(model);

                if (!result)
                {
                    return NotFound(ApiResult<string>.Fail("Vaccine Template not found."));
                }

                return Ok(ApiResult<string>.Succeed("Vaccine Template updated successfully!"));
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVaccineTemplate(Guid id)
        {
            try
            {
                var result = await _vaccineTemplateService.DeleteVaccineTemplateAsync(id);

                if (!result)
                {
                    return NotFound(ApiResult<string>.Fail("Vaccine Template not found."));
                }

                return Ok(ApiResult<string>.Succeed("Vaccine Template deleted successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaccineTemplateDetail(Guid id)
        {
            try
            {
                var result = await _vaccineTemplateService.GetVaccineTemplateDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Vaccine Template not found."));
                }

                return Ok(ApiResult<VaccineTemplateDetailModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }




    }
}
