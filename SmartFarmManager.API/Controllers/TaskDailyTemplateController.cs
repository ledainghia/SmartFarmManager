using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.TaskDailyTemplate;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.TaskDailyTemplate;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class TaskDailyTemplateController : ControllerBase
    {
        private readonly ITaskDailyTemplateService _taskDailyTemplateService;

        public TaskDailyTemplateController(ITaskDailyTemplateService taskDailyTemplateService)
        {
            _taskDailyTemplateService = taskDailyTemplateService;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateTaskDailyTemplate([FromBody] CreateTaskDailyTemplateRequest request)
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
                var model = request.MapToModel();
                var result = await _taskDailyTemplateService.CreateTaskDailyTemplateAsync(model);

                if (!result)
                {
                    throw new Exception("Error while creating Task Daily Template!");
                }

                return Ok(ApiResult<string>.Succeed("Task Daily Template created successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskDailyTemplate(Guid id, [FromBody] UpdateTaskDailyTemplateRequest request)
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
                var model = request.MapToModel(id);
                var result = await _taskDailyTemplateService.UpdateTaskDailyTemplateAsync(model);

                if (!result)
                {
                    throw new Exception("Error while updating Task Daily Template!");
                }

                return Ok(ApiResult<string>.Succeed("Task Daily Template updated successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskDailyTemplate(Guid id)
        {
            try
            {
                var result = await _taskDailyTemplateService.DeleteTaskDailyTemplateAsync(id);

                if (!result)
                {
                    return NotFound(ApiResult<string>.Fail("Task Daily Template not found."));
                }

                return Ok(ApiResult<string>.Succeed("Task Daily Template deleted successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskDailyTemplateDetail(Guid id)
        {
            try
            {
                // Gọi service để lấy chi tiết Task Daily Template
                var result = await _taskDailyTemplateService.GetTaskDailyTemplateDetailAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResult<string>.Fail("Task Daily Template not found."));
                }

                return Ok(ApiResult<TaskDailyTemplateDetailModel>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> GetTaskDailyTemplates([FromQuery] TaskDailyTemplateFilterPagingRequest filterRequest)
        {
            try
            {
                // Map request sang model tầng service
                var filterModel = new TaskDailyTemplateFilterModel
                {
                    GrowthStageTemplateId = filterRequest.GrowthStageTemplateId,
                    TaskName = filterRequest.TaskName,
                    Session = filterRequest.Session,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                // Gọi service để lấy dữ liệu
                var result = await _taskDailyTemplateService.GetTaskDailyTemplatesAsync(filterModel);

                // Trả về kết quả
                return Ok(ApiResult<PagedResult<TaskDailyTemplateItemModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }





    }
}
