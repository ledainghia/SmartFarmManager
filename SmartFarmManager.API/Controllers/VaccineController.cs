using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Responses.Vaccine;
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

    }
}
