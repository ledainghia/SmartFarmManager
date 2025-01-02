using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.MedicalSymptom;
using SmartFarmManager.API.Payloads.Responses.MedicalSymptom;
using SmartFarmManager.API.Payloads.Responses.Picture;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.Picture;
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
                Symptoms = request.Symptoms,
                Status = request.Status,
                AffectedQuantity = request.AffectedQuantity,
                Notes = request.Notes,
                Pictures = request.Pictures.Select(p => new PictureModel
                {
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
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
                Symptoms = medicalSymptom.Symptoms,
                Diagnosis = medicalSymptom.Diagnosis,
                Treatment = medicalSymptom.Treatment,
                Status = medicalSymptom.Status,
                AffectedQuantity = medicalSymptom.AffectedQuantity,
                Notes = medicalSymptom.Notes,
                NameAnimal = medicalSymptom.NameAnimal,
                Pictures = medicalSymptom.Pictures.Select(p => new PictureResponse
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
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
                Symptoms = ms.Symptoms,
                Diagnosis = ms.Diagnosis,
                Treatment = ms.Treatment,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Notes = ms.Notes,
                Quantity = ms.Quantity,
                NameAnimal = ms.NameAnimal,
                Pictures = ms.Pictures.Select(p => new PictureResponse
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            });

            return Ok(ApiResult<IEnumerable<MedicalSymptomResponse>>.Succeed(response));
        }

        // PUT: api/medical-symptoms/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicalSymptom(Guid id, [FromBody] UpdateMedicalSymptomRequest request)
        {
            if (request == null)
            {
                return BadRequest(ApiResult<object>.Fail("Invalid request data."));
            }

            var updatedModel = new MedicalSymptomModel
            {
                Id = id,
                Diagnosis = request.Diagnosis,
                Treatment = request.Treatment,
                Status = request.Status,
                Notes = request.Notes
            };

            var result = await _medicalSymptomService.UpdateMedicalSymptomAsync(updatedModel);

            if (!result)
            {
                return NotFound(ApiResult<object>.Fail($"MedicalSymptom with ID {id} not found."));
            }

            return Ok(ApiResult<object>.Succeed("MedicalSymptom updated successfully."));
        }
        // GET: api/medical-symptoms/by-staff-and-batch
        [HttpGet("by-staff-and-batch")]
        public async Task<IActionResult> GetMedicalSymptomsByStaffAndBatch([FromQuery] Guid staffId, [FromQuery] Guid farmBatchId)
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

