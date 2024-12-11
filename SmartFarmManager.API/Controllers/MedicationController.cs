using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Medication;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        [HttpPost()]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationRequest medication)
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

            if (medication == null) return BadRequest("Invalid medication data.");
            var medicationExist = await _medicationService.GetMedicationByName(medication.Name);
            if (medicationExist != null)
            {
                return BadRequest(ApiResult<object>.Fail($"The medicine's name {medicationExist.Name} already exists."));
            }
            var createdMedication = await _medicationService.CreateMedicationAsync(medication.MapToModel());

            return CreatedAtAction(nameof(GetMedications), null, createdMedication);
        }

        // GET: api/medications
        [HttpGet]
        public async Task<IActionResult> GetMedications([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagedMedications = await _medicationService.GetPagedMedicationsAsync(name, minPrice, maxPrice, page, pageSize);

            return Ok(ApiResult<PagedResult<MedicationModel>>.Succeed(pagedMedications));
        }

    }
}
