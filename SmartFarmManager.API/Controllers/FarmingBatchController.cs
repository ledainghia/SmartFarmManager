using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using Sprache;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.Cages;

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

        [HttpPost("create-multi-cage")]
        public async Task<IActionResult> CreateFarmingBatchMultiCage([FromBody] CreateFarmingBatchMultiCageRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { Errors = errors });
            }

            try
            {
                var result = await _farmingBatchService.CreateFarmingBatchMultiCageAsync(request.MapToModel());

                if (result)
                {
                    return Ok(new { Message = "Farming batches for multiple cages created successfully." });
                }

                return BadRequest(new { Message = "Failed to create farming batches." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
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
        [HttpPost("update-status-today")]
        public async Task<IActionResult> UpdateFarmingBatchStatusToday()
        {
            try
            {
                // Gọi hàm kiểm tra và cập nhật trạng thái vụ nuôi có ngày bắt đầu là hôm nay
                await _farmingBatchService.RunUpdateFarmingBatchesStatusAsync();

                return Ok(new { Message = "Farming batches status updated successfully for today." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
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
                var response = await _farmingBatchService.GetFarmingBatchesAsync(request.KeySearch, request.FarmId, request.CageName, request.Name, request.Name, request.StartDateFrom, request.StartDateTo, request.PageNumber, request.PageSize, request.CageId, request.isCancel);

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

        /// 📌 **API: Báo cáo chi tiết Farming Batch**
        [HttpGet("{farmingBatchId}/detailed-report")]
        public async Task<IActionResult> GetDetailedFarmingBatchReport(Guid farmingBatchId)
        {
            var report = await _farmingBatchService.GetDetailedFarmingBatchReportAsync(farmingBatchId);
            if (report == null)
                return NotFound(ApiResult<object>.Fail("Farming batch not found."));

            return Ok(ApiResult<DetailedFarmingBatchReportResponse>.Succeed(report));
        }

        [HttpGet("{cageId}/current-farming-stage")]
        public async Task<IActionResult> GetCurrentFarmingStage(Guid cageId)
        {
            try
            {
                var result = await _farmingBatchService.GetCurrentFarmingStageWithCageAsync(cageId);

                if (result == null)
                    return NotFound(ApiResult<object>.Fail("No active farming batch found for this cage."));

                return Ok(ApiResult<CageFarmingStageModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }


        [HttpPost("check-upcoming-farming-batches")]
        public async Task<IActionResult> CheckAndNotifyAdminForUpcomingFarmingBatches()
        {
            try
            {
                await _farmingBatchService.CheckAndNotifyAdminForUpcomingFarmingBatchesAsync();

                return Ok(new { Message = "Checked and notified admins about upcoming farming batches." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{farmingBatchId}/update-start-date")]
        public async Task<IActionResult> UpdateStartDate(Guid farmingBatchId, DateTime newStartDate)
        {
            try
            {
                var result = await _farmingBatchService.UpdateStartDateAsync(farmingBatchId, newStartDate);
                return Ok(new { Message = "Start date updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{farmingBatchId}")]
        public async Task<IActionResult> GetFarmingBatchDetail(Guid farmingBatchId)
        {
            try
            {
                var result = await _farmingBatchService.GetFarmingBatchDetailAsync(farmingBatchId);
                return Ok(ApiResult<FarmingBatchDetailModel>.Succeed(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
