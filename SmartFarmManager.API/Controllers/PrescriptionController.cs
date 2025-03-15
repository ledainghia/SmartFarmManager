using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Prescription;
using SmartFarmManager.API.Payloads.Responses.Prescription;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }
        [HttpGet("{id:guid}/prescription")]
        public async Task<IActionResult> GetPrescriptionById(Guid id)
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
                var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
                if (prescription == null)
                    return NotFound(ApiResult<string>.Fail("Prescription not found"));

                var response = new PrescriptionResponse
                {
                    Id = prescription.Id,
                    PrescribedDate = prescription.PrescribedDate,
                    Notes = prescription.Notes,
                    QuantityAnimal = prescription.QuantityAnimal,
                    Status = prescription.Status,
                    Price = prescription.Price,
                    CageId = prescription.CageId,
                    DaysToTake = prescription.DaysToTake,
                    Symptoms = prescription.Symptoms,
                    Medications = prescription.Medications.Select(m => new PrescriptionMedicationResponse
                    {
                        MedicationId = m.MedicationId,
                        MedicationName = m.Medication?.Name,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon
                    }).ToList()
                };

                return Ok(ApiResult<PrescriptionResponse>.Succeed(response));
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
        [HttpPost("{symptomId:guid}/prescription")]
        public async Task<IActionResult> CreatePrescription(Guid symptomId, [FromBody] CreatePrescriptionRequest request)
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
                var id = await _prescriptionService.CreatePrescriptionAsync(new PrescriptionModel
                {
                    RecordId = symptomId,
                    PrescribedDate = DateTime.UtcNow,
                    Notes = request.Notes,
                    CageId = request.CageId,
                    DaysToTake = request.DaysToTake,
                    QuantityAnimal = request.QuantityAnimal,
                    Status = request.Status,
                    Medications = request.Medications.Select(m => new PrescriptionMedicationModel
                    {
                        MedicationId = m.MedicationId,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon,
                        Notes = m.Notes
                    }).ToList()
                });

                return CreatedAtAction(nameof(GetPrescriptionById), new { id }, ApiResult<Guid?>.Succeed(id));
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
        [HttpGet("cage/{cageId:guid}/active-prescriptions")]
        public async Task<IActionResult> GetActivePrescriptionsByCageId(Guid cageId)
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
                var prescriptions = await _prescriptionService.GetActivePrescriptionsByCageIdAsync(cageId);

                if (!prescriptions.Any())
                    return NotFound(ApiResult<string>.Fail("No active prescriptions found for this cage"));

                // Duyệt qua danh sách đơn thuốc và map về response
                var response = prescriptions.Select(prescription => new PrescriptionResponse
                {
                    Id = prescription.Id,
                    PrescribedDate = prescription.PrescribedDate,
                    Notes = prescription.Notes,
                    QuantityAnimal = prescription.QuantityAnimal,
                    Status = prescription.Status,
                    Price = prescription.Price,
                    CageId = prescription.CageId,
                    DaysToTake = prescription.DaysToTake,
                    Medications = prescription.Medications.Select(m => new PrescriptionMedicationResponse
                    {
                        MedicationId = m.MedicationId,
                        MedicationName = m.Medication?.Name,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon
                    }).ToList()
                }).ToList();

                return Ok(ApiResult<IEnumerable<PrescriptionResponse>>.Succeed(response));
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
        [HttpPut("{prescriptionId:guid}")]
        public async Task<IActionResult> UpdatePrescription(Guid prescriptionId, [FromBody] UpdatePrescriptionRequest request)
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
                var model = new PrescriptionModel
                {
                    Id = prescriptionId,
                    PrescribedDate = request.PrescribedDate,
                    Notes = request.Notes,
                    CageId = request.CageId,
                    DaysToTake = request.DaysToTake,
                    QuantityAnimal = request.QuantityAnimal,
                    CaseType = request.CaseType,
                    Status = request.Status,
                    Price = request.Price,
                };

                var result = await _prescriptionService.UpdatePrescriptionAsync(model);

                if (!result)
                    return NotFound(ApiResult<string>.Fail("Đơn thuốc không được tìm thấy."));

                return Ok(ApiResult<bool>.Succeed(true));
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

        [HttpGet("{prescriptionId}/is-last-session")]
        public async Task<IActionResult> CheckLastPrescriptionSession(Guid prescriptionId)
        {
            try
            {
                var result = await _prescriptionService.IsLastPrescriptionSessionAsync(prescriptionId);
                return Ok(ApiResult<bool>.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<bool>.Fail(ex.Message));
            }
        }
        [HttpPut("{prescriptionId}/status")]
        public async Task<IActionResult> UpdatePrescriptionStatus(Guid prescriptionId, [FromBody] UpdatePrescriptionModel request)
        {
            try
            {
                var result = await _prescriptionService.UpdatePrescriptionStatusAsync(prescriptionId, request);
                if (!result)
                    return BadRequest(ApiResult<bool>.Fail("Failed to update prescription status."));

                return Ok(ApiResult<bool>.Succeed(true));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<bool>.Fail(ex.Message));
            }
        }
        [HttpPost("{medicalSymptomId}/create-new-prescription")]
        public async Task<IActionResult> CreateNewPrescription([FromBody] CreateNewPrescriptionRequest request, Guid medicalSymptomId)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResult<object>.Fail("Invalid request data."));

                // Chuyển đổi từ CreatePrescriptionRequest sang PrescriptionModel
                var prescriptionModel = new PrescriptionModel
                {
                    Id = Guid.NewGuid(),
                    RecordId = request.RecordId,
                    CageId = request.CageId,
                    PrescribedDate = request.PrescribedDate,
                    Notes = request.Notes,
                    QuantityAnimal = request.QuantityAnimal,
                    Status = request.Status,
                    DaysToTake = request.DaysToTake,
                    Disease = request.Disease,
                    CageAnimalName = request.CageAnimalName,
                    Symptoms = request.Symptoms,
                    Medications = request.Medications?.Select(m => new PrescriptionMedicationModel
                    {
                        MedicationId = m.MedicationId,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon,
                        Notes = m.Notes
                    }).ToList() ?? new List<PrescriptionMedicationModel>()
                };
                var result = await _prescriptionService.CreateNewPrescriptionAsync(prescriptionModel, medicalSymptomId);

                if (!result)
                    return BadRequest(ApiResult<object>.Fail("Failed to create new prescription."));

                return Ok(ApiResult<object>.Succeed("New prescription created successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("vet")]
        public async Task<IActionResult> GetPrescriptions(
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] string? status,
    [FromQuery] string? cageName,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _prescriptionService.GetPrescriptionsAsync(startDate, endDate, status, cageName, pageNumber, pageSize);
                return Ok(ApiResult<PagedResult<PrescriptionModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }



    }
}
