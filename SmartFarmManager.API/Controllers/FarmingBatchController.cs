using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FarmingBatch;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class FarmingBatchController : ControllerBase
    {
        private readonly IFarmingBatchService _farmingBatchService;

        public FarmingBatchController(IFarmingBatchService farmingBatchService)
        {
            _farmingBatchService = farmingBatchService;
        }


        [HttpPost()]
        public async Task<IActionResult> CreateFarmingBatch([FromBody] CreateFarmingBatchRequest request)
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
                var model = request.MapToModel();
                var result = await _farmingBatchService.CreateFarmingBatchAsync(model);

                if (!result)
                {
                    return BadRequest(ApiResult<string>.Fail("Failed to create farming batch. Please try again."));
                }

                return Ok(ApiResult<string>.Succeed("Farming batch created successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }


        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateFarmingBatchStatus(Guid id, [FromBody] UpdateFarmingBatchStatusRequest request)
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
                var result = await _farmingBatchService.UpdateFarmingBatchStatusAsync(id, request.NewStatus);

                if (!result)
                {
                    return BadRequest(ApiResult<string>.Fail("Failed to update farming batch status. Please try again."));
                }

                return Ok(ApiResult<string>.Succeed("Farming batch status updated successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }
<<<<<<< Updated upstream
=======

        [HttpGet]
        public async Task<IActionResult> GetFarmingBatches([FromQuery] FarmingBatchFilterPagingRequest request)
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
                var response = await _farmingBatchService.GetFarmingBatchesAsync(request.Status, request.CageName, request.Name, request.Species, request.StartDateFrom, request.StartDateTo, request.PageNumber, request.PageSize, request.CageId);
                return Ok(ApiResult<PagedResult<FarmingBatchModel>>.Succeed(response));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }

>>>>>>> Stashed changes
    }
}
