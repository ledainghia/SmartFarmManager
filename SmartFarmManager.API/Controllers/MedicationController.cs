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
        public async Task<IActionResult> GetMedications([FromQuery] MedicationFilterModel filter)
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
                var result = await _medicationService.GetMedicationsAsync(filter);
                return Ok(ApiResult<PagedResult<MedicationModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedication(Guid id, [FromBody] UpdateMedicationRequest request)
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
                var result = await _medicationService.UpdateMedicationAsync(id, model);

                if (!result)
                {
                    throw new Exception("Error while updating Medication!");
                }

                return Ok(ApiResult<string>.Succeed("Medication updated successfully!"));
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicationDetail(Guid id)
        {
            try
            {
                var result = await _medicationService.GetMedicationDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Medication not found."));
                }

                return Ok(ApiResult<MedicationDetailResponseModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(Guid id)
        {
            try
            {
                var result = await _medicationService.DeleteMedicationAsync(id);

                if (!result)
                {
                    return Ok(ApiResult<string>.Succeed("Khôi phục thành công!"));
                }

                return Ok(ApiResult<string>.Succeed("Xóa thành công!"));
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
