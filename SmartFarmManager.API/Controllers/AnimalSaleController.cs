using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.AnimalSale;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalSaleController : ControllerBase
    {
        private readonly IAnimalSaleService _animalSaleService;

        public AnimalSaleController(IAnimalSaleService animalSaleService)
        {
            _animalSaleService = animalSaleService;
        }

        /// <summary>
        /// Tạo mới một AnimalSale
        /// </summary>
        /// <param name="request">Dữ liệu AnimalSale cần tạo</param>
        /// <returns>Kết quả tạo AnimalSale</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAnimalSale([FromBody] CreateAnimalSaleRequest request)
        {
            try
            {
                if (request == null || request.GrowthStageId == Guid.Empty)
                    return BadRequest(ApiResult<string>.Fail("Invalid request data."));

                bool success = await _animalSaleService.CreateAnimalSaleAsync(request);

                if (!success)
                    return BadRequest(ApiResult<string>.Fail("Failed to create AnimalSale."));

                return Ok(ApiResult<string>.Succeed("AnimalSale successfully created."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
    }
}
