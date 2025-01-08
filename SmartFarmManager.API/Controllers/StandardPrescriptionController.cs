using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.StandardPrescription;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StandardPrescriptionController : ControllerBase
    {
        private readonly IStandardPrescriptionService _service;

        public StandardPrescriptionController(IStandardPrescriptionService service)
        {
            _service = service;
        }

        [HttpGet("{diseaseId:guid}")]
        public async Task<IActionResult> GetStandardPrescriptionsByDiseaseId(Guid diseaseId)
        {
            try
            {
                var prescriptions = await _service.GetStandardPrescriptionsByDiseaseIdAsync(diseaseId);

                if (!prescriptions.Any())
                {
                    return NotFound(ApiResult<object>.Fail($"No StandardPrescriptions found for DiseaseId: {diseaseId}"));
                }

                return Ok(ApiResult<List<StandardPrescriptionModel>>.Succeed(prescriptions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An unexpected error occurred: {ex.Message}"));
            }
        }
    }
}
