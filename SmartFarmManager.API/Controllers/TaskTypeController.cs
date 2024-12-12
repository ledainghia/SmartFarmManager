using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.TaskType;
using SmartFarmManager.Service.BusinessModels.TaskType;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskTypeController : ControllerBase
    {
        private readonly ITaskTypeService _taskTypeService;

        public TaskTypeController(ITaskTypeService taskTypeService)
        {
            _taskTypeService = taskTypeService;
        }

        // POST: api/task-types
        [HttpPost]
        public async Task<IActionResult> CreateTaskType([FromBody] CreateTaskTypeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.Fail("Invalid request data."));
            }

            var taskTypeModel = new TaskTypeModel
            {
                TaskTypeName = request.TaskTypeName,
                PriorityNum = request.PriorityNum
            };

            var id = await _taskTypeService.CreateTaskTypeAsync(taskTypeModel);

            return CreatedAtAction(nameof(GetTaskTypeById), new { id }, ApiResult<object>.Succeed(new { id }));
        }

        // GET: api/task-types
        [HttpGet]
        public async Task<IActionResult> GetTaskTypes()
        {
            var taskTypes = await _taskTypeService.GetTaskTypesAsync();

            if (taskTypes == null || !taskTypes.Any())
            {
                return NotFound(ApiResult<object>.Fail("No task types found."));
            }

            return Ok(ApiResult<IEnumerable<TaskTypeModel>>.Succeed(taskTypes));
        }

        // GET: api/task-types/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTaskTypeById(Guid id)
        {
            var taskType = await _taskTypeService.GetTaskTypeByIdAsync(id);

            if (taskType == null)
            {
                return NotFound(ApiResult<object>.Fail($"TaskType with ID {id} not found."));
            }

            return Ok(ApiResult<TaskTypeModel>.Succeed(taskType));
        }

        // PUT: api/task-types/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTaskType(Guid id, [FromBody] UpdateTaskTypeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.Fail("Invalid request data."));
            }

            var taskTypeModel = new TaskTypeModel
            {
                Id = id,
                TaskTypeName = request.TaskTypeName,
                PriorityNum = request.PriorityNum
            };

            var result = await _taskTypeService.UpdateTaskTypeAsync(taskTypeModel);

            if (!result)
            {
                return NotFound(ApiResult<object>.Fail($"TaskType with ID {id} not found."));
            }

            return Ok(ApiResult<object>.Succeed("TaskType updated successfully."));
        }

        // DELETE: api/task-types/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTaskType(Guid id)
        {
            var result = await _taskTypeService.DeleteTaskTypeAsync(id);

            if (!result)
            {
                return NotFound(ApiResult<object>.Fail($"TaskType with ID {id} not found."));
            }

            return Ok(ApiResult<object>.Succeed("TaskType deleted successfully."));
        }
    }
}
