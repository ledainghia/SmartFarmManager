using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using Sprache;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;

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
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
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
                var response = await _farmingBatchService.GetFarmingBatchesAsync(request.CageName, request.Name, request.Name, request.StartDateFrom, request.StartDateTo, request.PageNumber, request.PageSize, request.CageId, request.isCancel);

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

        [HttpGet("cage/{cageId:guid}")]
        public async Task<IActionResult> GetActiveFarmingBatchByCageId(Guid cageId)
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
                var farmingBatch = await _farmingBatchService.GetActiveFarmingBatchByCageIdAsync(cageId);

                if (farmingBatch == null)
                    return NotFound(ApiResult<string>.Fail("No active farming batch found for the given CageId"));

                return Ok(ApiResult<FarmingBatchModel>.Succeed(farmingBatch));
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
        [HttpGet("active-batches-by-user")]
        public async Task<IActionResult> GetActiveBatchesByUser([FromQuery] Guid userId)
        {
            var activeBatches = await _farmingBatchService.GetActiveFarmingBatchesByUserAsync(userId);
            return Ok(ApiResult<List<FarmingBatchModel>>.Succeed(activeBatches));
        }

        [HttpGet("{farmingBatchId}/report")]
        public async Task<IActionResult> GetFarmingBatchReport(Guid farmingBatchId)
        {
            var report = await _farmingBatchService.GetFarmingBatchReportAsync(farmingBatchId);
            if (report == null)
                return NotFound(ApiResult<object>.Fail("Farming batch not found."));

            return Ok(ApiResult<FarmingBatchReportResponse>.Succeed(report));
        }
    }
}
