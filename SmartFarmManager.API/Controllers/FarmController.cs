using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Payloads.Requests.Farm;
using SmartFarmManager.API.Payloads.Responses.Farm;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmController : ControllerBase
    {
        private readonly IFarmService _farmService;

        public FarmController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFarm([FromBody] CreateFarmRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var farmId = await _farmService.CreateFarmAsync(new FarmModel
            {
                Name = request.Name,
                Address = request.Address,
                Area = request.Area,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            });

            return CreatedAtAction(nameof(GetFarmById), new { id = farmId }, new { id = farmId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFarmById(Guid id)
        {
            var farmModel = await _farmService.GetFarmByIdAsync(id);
            if (farmModel == null)
            {
                return NotFound("Farm not found.");
            }

            var response = new FarmResponse
            {
                Id = farmModel.Id,
                Name = farmModel.Name,
                Address = farmModel.Address,
                Area = farmModel.Area,
                PhoneNumber = farmModel.PhoneNumber,
                Email = farmModel.Email
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFarms([FromQuery] string? search)
        {
            var farmModels = await _farmService.GetAllFarmsAsync(search);

            var responses = farmModels.Select(f => new FarmResponse
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                Area = f.Area,
                PhoneNumber = f.PhoneNumber,
                Email = f.Email
            });

            return Ok(responses);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFarm(Guid id, [FromBody] UpdateFarmRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _farmService.UpdateFarmAsync(id, new FarmModel
            {
                Name = request.Name,
                Address = request.Address,
                Area = request.Area,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            });

            if (!updated)
            {
                return NotFound("Farm not found.");
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFarm(Guid id)
        {
            var deleted = await _farmService.DeleteFarmAsync(id);
            if (!deleted)
            {
                return NotFound("Farm not found.");
            }

            return NoContent();
        }
    }

}
