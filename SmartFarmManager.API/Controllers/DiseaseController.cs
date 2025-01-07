using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Disease;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiseaseController : ControllerBase
    {
        private readonly IDiseaseService _diseaseService;

        public DiseaseController(IDiseaseService diseaseService)
        {
            _diseaseService = diseaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDiseases([FromQuery] string? name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10000)
        {
            var pagedDiseases = await _diseaseService.GetPagedDiseasesAsync(name, page, pageSize);
            return Ok(ApiResult<PagedResult<DiseaseModel>>.Succeed(pagedDiseases));
        }
    }
}
