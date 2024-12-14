using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Cages;
using SmartFarmManager.API.Payloads.Responses.Cage;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using Sprache;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class CageController : ControllerBase
    {
        private readonly ICageService _cageService;
        public CageController(ICageService cageService)
        {
            _cageService = cageService;
        }
        /// <summary>
        /// Get all cages with filter and pagination
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated list of cages</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CageFilterPagingRequest request)
        {
            try
            {
                var cages = await _cageService.GetCagesAsync(request.MapToModel());
                return Ok(ApiResult<PagedResult<CageResponseModel>>.Succeed(cages));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }


        }

        /// <summary>
        /// Lấy thông tin chi tiết của một cage theo ID.
        /// </summary>
        /// <param name="id">ID của Cage</param>
        /// <returns>Thông tin chi tiết Cage</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCageById(Guid id)
        {
            try
            {
                var result = await _cageService.GetCageByIdAsync(id);
                return Ok(ApiResult<CageDetailModel>.Succeed(result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Cage not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCage([FromBody] CreateCageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cageId = await _cageService.CreateCageAsync(new CageModel
            {
                FarmId = request.FarmId,
                Name = request.Name,
                Area = request.Area,
                Capacity = request.Capacity,
                Location = request.Location,
                AnimalType = request.AnimalType
            });

            return CreatedAtAction(nameof(GetCageById), new { id = cageId }, new { id = cageId });
        }

        

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCages([FromQuery] string? search)
        {
            var cageModels = await _cageService.GetAllCagesAsync(search);

            var responses = cageModels.Select(c => new CageResponse
            {
                Id = c.Id,
                //FarmId = c.FarmId,
                Name = c.Name,
                Area = c.Area,
                Capacity = c.Capacity,
                Location = c.Location,
                AnimalType = c.AnimalType
            });

            return Ok(responses);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCage(Guid id, [FromBody] UpdateCageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _cageService.UpdateCageAsync(id, new CageModel
            {
                FarmId = request.FarmId,
                Name = request.Name,
                Area = request.Area,
                Capacity = request.Capacity,
                Location = request.Location,
                AnimalType = request.AnimalType
            });

            if (!updated)
            {
                return NotFound("Cage not found.");
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCage(Guid id)
        {
            var deleted = await _cageService.DeleteCageAsync(id);
            if (!deleted)
            {
                return NotFound("Cage not found.");
            }

            return NoContent();
        }
    }
}
