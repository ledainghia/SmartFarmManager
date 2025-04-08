using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Cages;
using SmartFarmManager.API.Payloads.Responses.Cage;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.Prescription;
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
            if (!ModelState.IsValid)
            {
                // Collect validation errors
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
                var result = await _cageService.GetCagesAsync(request.MapToModel());
                return Ok(ApiResult<PagedResult<CageResponseModel>>.Succeed(result));
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
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            try
            {
                var cageId = await _cageService.CreateCageAsync(new CageModel
                {
                    FarmId = request.FarmId,
                    Name = request.Name,
                    Area = request.Area,
                    Capacity = request.Capacity,
                    Location = request.Location,
                });

                var response = ApiResult<object>.Succeed(new { Id = cageId });

                return CreatedAtAction(nameof(GetCageById), new { id = cageId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCages([FromQuery] string? search)
        {
            try
            {
                var cageModels = await _cageService.GetAllCagesAsync(search);

                var responses = cageModels.Select(c => new CageResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Area = c.Area,
                    Capacity = c.Capacity,
                    Location = c.Location,
                });

                return Ok(ApiResult<IEnumerable<CageResponse>>.Succeed(responses));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCage(Guid id, [FromBody] UpdateCageRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                return BadRequest(ApiResult<List<string>>.Error(errors));
            }

            try
            {
                var updated = await _cageService.UpdateCageAsync(id, new CageModel
                {
                    FarmId = request.FarmId,
                    Name = request.Name,
                    Area = request.Area,
                    Capacity = request.Capacity,
                    Location = request.Location,
                });

                if (!updated)
                {
                    return NotFound(ApiResult<string>.Fail("Cage not found."));
                }

                return Ok(ApiResult<string>.Succeed("Cage successfully updated."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCage(Guid id)
        {
            try
            {
                var deleted = await _cageService.DeleteCageAsync(id);
                if (!deleted)
                {
                    return NotFound(ApiResult<string>.Fail("Cage not found."));
                }

                return Ok(ApiResult<string>.Succeed("Cage successfully deleted."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("{cageId}/prescriptions/tasks")]
        public async Task<IActionResult> GetTasksForCage(Guid cageId)
        {
            try
            {
                var result = await _cageService.GetPrescriptionsWithTasksAsync(cageId);
                return Ok(ApiResult<IEnumerable<PrescriptionResponseModel>>.Succeed(result)); ;
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
            }
        }
    }
}
