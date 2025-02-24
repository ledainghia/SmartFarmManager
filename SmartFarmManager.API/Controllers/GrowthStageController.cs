using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.GrowthStage;
using SmartFarmManager.Service.BusinessModels.GrowthStage;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.BusinessModels.TaskDaily;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using Sprache;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrowthStageController : ControllerBase
    {
        private readonly IGrowthStageService _growthStageService;

        public GrowthStageController(IGrowthStageService growthStageService)
        {
            _growthStageService = growthStageService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetGrowthStages([FromQuery] GrowthStageFilterPagingRequest filterRequest)
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
                var filterModel = new GrowthStageFilterModel
                {
                    FarmingBatchId = filterRequest.FarmingBatchId,
                    Name = filterRequest.Name,
                    AgeStart = filterRequest.AgeStart,
                    AgeEnd = filterRequest.AgeEnd,
                    Status = filterRequest.Status,
                    PageNumber = (int)filterRequest.PageNumber,
                    PageSize = (int)filterRequest.PageSize,
                    OrderBy = filterRequest.OrderBy,
                    Order = filterRequest.Order
                };

                var result = await _growthStageService.GetGrowthStagesAsync(filterModel);
                return Ok(ApiResult<PagedResult<GrowthStageItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGrowthStageDetail(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(ApiResult<string>.Fail("Invalid GrowthStage ID."));
            }

            try
            {
                var result = await _growthStageService.GetGrowthStageDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("GrowthStage not found."));
                }

                return Ok(ApiResult<GrowthStageDetailModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/taskdailies")]
        public async Task<IActionResult> GetTaskDailiesByGrowthStageId(
    Guid id,
    [FromQuery] TaskDailyFilterPagingRequest filterRequest)
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
                var filterModel = new TaskDailyFilterModel
                {
                    GrowthStageId = id,
                    TaskName = filterRequest.TaskName,
                    Session = filterRequest.Session,
                    PageNumber = (int)filterRequest.PageNumber,
                    PageSize = (int)filterRequest.PageSize
                };

                var result = await _growthStageService.GetTaskDailiesByGrowthStageIdAsync(filterModel);
                return Ok(ApiResult<PagedResult<TaskDailyModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/vaccineschedules")]
        public async Task<IActionResult> GetVaccineSchedulesByGrowthStageId(
    Guid id,
    [FromQuery] VaccineScheduleFilterPagingRequest filterRequest)
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
                var filterModel = new VaccineScheduleFilterModel
                {
                    GrowthStageId = id,
                    Status = filterRequest.Status,
                    PageNumber = (int)filterRequest.PageNumber,
                    PageSize = (int)filterRequest.PageSize
                };

                var result = await _growthStageService.GetVaccineSchedulesByGrowthStageIdAsync(filterModel);
                return Ok(ApiResult<PagedResult<VaccineScheduleModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("cage/{cageId:guid}/active-growth-stage")]
        public async Task<IActionResult> GetActiveGrowthStageByCageId(Guid cageId)
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
                var growthStage = await _growthStageService.GetActiveGrowthStageByCageIdAsync(cageId);

                if (growthStage == null)
                    return NotFound(ApiResult<string>.Fail("No active growth stage found for this cage"));

                return Ok(ApiResult<GrowthStageDetailModel>.Succeed(growthStage));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPut("growth-stage/update-weight")]
        public async Task<IActionResult> UpdateWeightAnimal([FromBody] UpdateGrowthStageRequest request)
        {
            try
            {
                bool isUpdated = await _growthStageService.UpdateWeightAnimalAsync(request);
                return isUpdated
        ? Ok(ApiResult<string>.Succeed("Weight animal updated successfully."))
        : BadRequest(ApiResult<string>.Fail("Failed to update weight animal."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message)); // Trả về lỗi Conflict nếu trùng lặp
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }

    }
}
