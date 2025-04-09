using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Vaccine;
using SmartFarmManager.API.Payloads.Responses.Vaccine;
using SmartFarmManager.Service.BusinessModels.Vaccine;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineController : ControllerBase
    {
        private readonly IVaccineService _vaccineService;

        public VaccineController(IVaccineService vaccineSerrvice)
        {
            _vaccineService = vaccineSerrvice;
        }

        [HttpGet("cage/{cageId:guid}/active-vaccine")]
        public async Task<IActionResult> GetActiveVaccineByCageId(Guid cageId)
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
                var vaccine = await _vaccineService.GetActiveVaccineByCageIdAsync(cageId);

                if (vaccine == null)
                    return NotFound(ApiResult<string>.Fail("No active vaccine found for this cage"));

                // Map vaccine sang response
                var response = new VaccineResponse
                {
                    Id = vaccine.Id,
                    Name = vaccine.Name,
                    Method = vaccine.Method,
                    AgeStart = vaccine.AgeStart,
                    AgeEnd = vaccine.AgeEnd
                };

                return Ok(ApiResult<VaccineResponse>.Succeed(response));
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

        [HttpPost()]
        public async Task<IActionResult> CreateVaccine([FromBody] CreateVaccineRequest request)
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
                var result = await _vaccineService.CreateVaccineAsync(model);

                if (!result)
                {
                    throw new Exception("Error while creating Vaccine!");
                }

                return Ok(ApiResult<string>.Succeed("Vaccine created successfully!"));
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
        public async Task<IActionResult> UpdateVaccine(Guid id, [FromBody] UpdateVaccineRequest request)
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
                var result = await _vaccineService.UpdateVaccineAsync(id, model);

                if (!result)
                {
                    throw new Exception("Error while updating Vaccine!");
                }

                return Ok(ApiResult<string>.Succeed("Vaccine updated successfully!"));
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
        public async Task<IActionResult> DeleteVaccine(Guid id)
        {
            try
            {
                var result = await _vaccineService.DeleteVaccineAsync(id);

                if (!result)
                {
                    throw new Exception("Error while deleting Vaccine!");
                }

                return Ok(ApiResult<string>.Succeed("Vaccine deleted successfully!"));
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
        public async Task<IActionResult> GetVaccines([FromQuery] VaccineFilterRequest filterRequest)
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
                var filterModel = new VaccineFilterModel
                {
                    KeySearch = filterRequest.KeySearch,
                    AgeStart = filterRequest.AgeStart,
                    AgeEnd = filterRequest.AgeEnd,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };
                var result = await _vaccineService.GetVaccinesAsync(filterModel);
                return Ok(ApiResult<PagedResult<VaccineItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVaccineDetail(Guid id)
        {
            try
            {
                var result = await _vaccineService.GetVaccineDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Vaccine not found."));
                }

                return Ok(ApiResult<VaccineDetailResponseModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

    }
}
