using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Prescription;
using SmartFarmManager.API.Payloads.Responses.Prescription;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
using SmartFarmManager.Service.Interfaces;

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

    }
}
