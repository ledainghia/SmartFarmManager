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
                Medications = prescription.Medications.Select(m => new PrescriptionMedicationResponse
                {
                    MedicationId = m.MedicationId,
                    MedicationName = m.Medication?.Name,
                    Dosage = m.Dosage,
                    Morning = m.Morning,
                    Afternoon = m.Afternoon,
                    Evening = m.Evening,
                    Night = m.Night
                }).ToList()
            };

            return Ok(ApiResult<PrescriptionResponse>.Succeed(response));
        }
        [HttpPost("{symptomId:guid}/prescription")]
        public async Task<IActionResult> CreatePrescription(Guid symptomId, [FromBody] CreatePrescriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<string>.Fail("Invalid request"));

            var id = await _prescriptionService.CreatePrescriptionAsync(new PrescriptionModel
            {
                RecordId = symptomId,
                PrescribedDate = request.PrescribedDate,
                Notes = request.Notes,
                CageId = request.CageId,
                Medications = request.Medications.Select(m => new PrescriptionMedicationModel
                {
                    MedicationId = m.MedicationId,
                    Dosage = m.Dosage,
                    Morning = m.Morning,
                    Afternoon = m.Afternoon,
                    Evening = m.Evening,
                    Night = m.Night
                }).ToList()
            });

            return CreatedAtAction(nameof(GetPrescriptionById), new { id }, ApiResult<Guid>.Succeed(id));
        }

    }
}
