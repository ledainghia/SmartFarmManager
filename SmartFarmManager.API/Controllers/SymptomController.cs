using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Symptom;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Symptom;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SymptomController : ControllerBase
    {
        private readonly ISymptomService _symptomService;

        public SymptomController(ISymptomService symptomService)
        {
            _symptomService = symptomService;
        }

        [HttpGet("/getAll")]
        public async Task<IActionResult> GetAllSymptoms()
        {
            var symptoms = await _symptomService.GetAllSymptomsAsync();
            return Ok(ApiResult<List<SymptomModel>>.Succeed(symptoms));
        }

        [HttpGet]
        public async Task<IActionResult> GetSymptoms([FromQuery] SymptomFilterModel filter)
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
                var result = await _symptomService.GetSymptomsAsync(filter);
                return Ok(ApiResult<PagedResult<SymptomModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSymptomById(Guid id)
        {
            var symptom = await _symptomService.GetSymptomByIdAsync(id);
            if (symptom == null)
            {
                return NotFound(ApiResult<object>.Fail("Symptom not found."));
            }
            return Ok(ApiResult<SymptomModel>.Succeed(symptom));
        }

        [HttpPost]
        public async Task<IActionResult> CreateSymptom([FromBody] CreateSymptomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.Fail("Invalid request data."));
            }

            var symptomModel = new SymptomModel
            {
                SymptomName = request.SymptomName
            };

            var id = await _symptomService.CreateSymptomAsync(symptomModel);
            return CreatedAtAction(nameof(GetSymptomById), new { id }, ApiResult<object>.Succeed(new { id }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSymptom(Guid id, [FromBody] UpdateSymptomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.Fail("Invalid request data."));
            }

            var symptomModel = new SymptomModel
            {
                Id = id,
                SymptomName = request.SymptomName
            };

            var result = await _symptomService.UpdateSymptomAsync(symptomModel);
            if (!result)
            {
                return NotFound(ApiResult<object>.Fail("Symptom not found."));
            }

            return Ok(ApiResult<object>.Succeed("Symptom updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSymptom(Guid id)
        {
            var result = await _symptomService.DeleteSymptomAsync(id);
            if (!result)
            {
                return NotFound(ApiResult<object>.Fail("Symptom not found."));
            }

            return Ok(ApiResult<object>.Succeed("Symptom deleted successfully."));
        }

    }
}
