using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.MedicalSymptom;
using SmartFarmManager.API.Payloads.Responses.MedicalSymptom;
using SmartFarmManager.API.Payloads.Responses.Picture;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.MedicalSymptomDetail;
using SmartFarmManager.Service.BusinessModels.Picture;
using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalSymptomController : ControllerBase
    {
        private readonly IMedicalSymptomService _medicalSymptomService;

        public MedicalSymptomController(IMedicalSymptomService medicalSymptomService)
        {
            _medicalSymptomService = medicalSymptomService;
        }

        // POST: api/medical-symptoms
        [HttpPost]
        public async Task<IActionResult> CreateMedicalSymptom([FromBody] CreateMedicalSymptomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.Fail("Invalid request data."));
            }

            var medicalSymptomModel = new MedicalSymptomModel
            {
                FarmingBatchId = request.FarmingBatchId,
                PrescriptionId = request.PrescriptionId,
                Status = request.Status,
                AffectedQuantity = request.AffectedQuantity,
                Notes = request.Notes,
                CreateAt = DateTime.UtcNow,
                Pictures = request.Pictures.Select(p => new PictureModel
                {
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList(),
                MedicalSymptomDetails = request.MedicalSymptomDetails.Select(d => new MedicalSymptomDetailModel
                {
                    SymptomId = d.SymptomId,
                    Notes = d.Notes
                }).ToList()
            };

            var id = await _medicalSymptomService.CreateMedicalSymptomAsync(medicalSymptomModel);
            return CreatedAtAction(nameof(GetMedicalSymptomById), new { id }, ApiResult<object>.Succeed(new { id }));
        }

        // GET: api/medical-symptoms/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicalSymptomById(Guid id)
        {
            var medicalSymptom = await _medicalSymptomService.GetMedicalSymptomByIdAsync(id);

            if (medicalSymptom == null)
            {
                return NotFound(ApiResult<object>.Fail($"MedicalSymptom with ID {id} not found."));
            }

            var response = new MedicalSymptomResponse
            {
                Id = medicalSymptom.Id,
                FarmingBatchId = medicalSymptom.FarmingBatchId,
                Diagnosis = medicalSymptom.Diagnosis,
                Status = medicalSymptom.Status,
                AffectedQuantity = medicalSymptom.AffectedQuantity,
                Notes = medicalSymptom.Notes,
                NameAnimal = medicalSymptom.NameAnimal,
                CreateAt = medicalSymptom.CreateAt,
                Pictures = medicalSymptom.Pictures.Select(p => new PictureResponse
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList(),
                Prescriptions = medicalSymptom.Prescriptions.Select(p => new Payloads.Responses.Prescription.PrescriptionResponse
                {
                    Id = p.Id,
                    PrescribedDate = p.PrescribedDate,
                    Status = p.Status,
                    QuantityAnimal = p.QuantityAnimal,
                    Notes = p.Notes,
                    Price = p.Price,
                    DaysToTake = p.DaysToTake,
                    EndDate = p.EndDate,
                }).ToList(),
            };

            return Ok(ApiResult<MedicalSymptomResponse>.Succeed(response));
        }

        // GET: api/medical-symptoms
        [HttpGet]
        public async Task<IActionResult> GetMedicalSymptoms([FromQuery] string? status)
        {
            var medicalSymptoms = await _medicalSymptomService.GetMedicalSymptomsAsync(status);

            if (medicalSymptoms == null || !medicalSymptoms.Any())
            {
                return NotFound(ApiResult<object>.Fail("No medical symptoms found."));
            }

            var response = medicalSymptoms.Select(ms => new MedicalSymptomResponse
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
                Diagnosis = ms.Diagnosis,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Notes = ms.Notes,
                Quantity = ms.Quantity,
                NameAnimal = ms.NameAnimal,
                CreateAt = ms.CreateAt,
                Pictures = ms.Pictures.Select(p => new PictureResponse
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList(),
                Prescriptions = ms.Prescriptions.Select(p => new Payloads.Responses.Prescription.PrescriptionResponse
                {
                    Id = p.Id,
                    PrescribedDate = p.PrescribedDate,
                    Status = p.Status,
                    QuantityAnimal = p.QuantityAnimal,
                    Notes = p.Notes,
                    Price = p.Price,
                    DaysToTake = p.DaysToTake,
                    EndDate = p.EndDate,
                }).ToList(),

            });

            return Ok(ApiResult<IEnumerable<MedicalSymptomResponse>>.Succeed(response));
        }

        // PUT: api/medical-symptoms/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicalSymptom(Guid id, [FromBody] UpdateMedicalSymptomRequest request)
        {
            if (!ModelState.IsValid)
            {
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
                if (request == null)
                {
                    return BadRequest(ApiResult<object>.Fail("Invalid request data."));
                }

                var updatedModel = new UpdateMedicalSymptomModel
                {
                    Id = id,
                    Diagnosis = request.Diagnosis,
                    Status = request.Status,
                    Notes = request.Notes,
                    Prescriptions = new Service.BusinessModels.Prescription.PrescriptionModel
                    {
                        RecordId = request.CreatePrescriptionRequest.MedicalSymptomId,
                        PrescribedDate = request.CreatePrescriptionRequest.PrescribedDate,
                        Notes = request.CreatePrescriptionRequest.Notes,
                        DaysToTake = request.CreatePrescriptionRequest.DaysToTake,
                        Status = request.CreatePrescriptionRequest.Status,
                        QuantityAnimal = request.CreatePrescriptionRequest.QuantityAnimal,
                        Medications = request.CreatePrescriptionRequest.Medications.Select(m => new PrescriptionMedicationModel
                        {
                            MedicationId = m.MedicationId,
                            Dosage = m.Dosage,
                            Morning = m.Morning,
                            Afternoon = m.Afternoon,
                            Evening = m.Evening,
                            Noon = m.Noon
                        }).ToList()
                    }
                };
                var result = await _medicalSymptomService.UpdateMedicalSymptomAsync(updatedModel);

                if (!result)
                {
                    return NotFound(ApiResult<object>.Fail($"MedicalSymptom with ID {id} not found."));
                }

                return Ok(ApiResult<object>.Succeed("MedicalSymptom updated successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message)); // Trả về lỗi Conflict nếu trùng lặp
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }
        // GET: api/medical-symptoms/by-staff-and-batch
        [HttpGet("by-staff-and-batch")]
        public async Task<IActionResult> GetMedicalSymptomsByStaffAndBatch([FromQuery] Guid? staffId, [FromQuery] Guid? farmBatchId)
        {
            if (staffId == Guid.Empty || farmBatchId == Guid.Empty)
            {
                return BadRequest(ApiResult<object>.Fail("Staff ID and Farm Batch ID are required."));
            }

            var result = await _medicalSymptomService.GetMedicalSymptomsByStaffAndBatchAsync(staffId, farmBatchId);

            if (result == null || !result.Any())
            {
                return NotFound(ApiResult<object>.Fail("No medical symptoms found for the given Staff ID and Farm Batch ID."));
            }

            return Ok(ApiResult<IEnumerable<MedicalSymptomModel>>.Succeed(result));
        }
    }
}

