using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                return BadRequest(ModelState);
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
            return CreatedAtAction(nameof(GetMedicalSymptomById), new { id }, new { id });
        }

        // GET: api/medical-symptoms/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetMedicalSymptomById(Guid id)
        {
            var medicalSymptom = await _medicalSymptomService.GetMedicalSymptomByIdAsync(id);

            if (medicalSymptom == null)
            {
                return NotFound();
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
                Pictures = medicalSymptom.Pictures.Select(p => new PictureResponse
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            };

            return Ok(response);
        }

        // GET: api/medical-symptoms
        [HttpGet]
        public async Task<IActionResult> GetMedicalSymptoms([FromQuery] string? status)
        {
            // Gọi service để lấy danh sách medical symptoms
            var medicalSymptoms = await _medicalSymptomService.GetMedicalSymptomsAsync(status);

            if (medicalSymptoms == null || !medicalSymptoms.Any())
            {
                return NotFound("No medical symptoms found.");
            }

            // Chuyển đổi kết quả từ model service sang response model
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
                Pictures = ms.Pictures.Select(p => new PictureResponse
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            });

            return Ok(response);
        }


        // PUT: api/medical-symptoms/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateMedicalSymptom(Guid id, [FromBody] UpdateMedicalSymptomRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid data.");
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
                return NotFound($"MedicalSymptom with ID {id} not found.");
            }

            return NoContent();
        }

    }
}

