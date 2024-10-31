using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Task;
using SmartFarmManager.API.Payloads.Responses.Task;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.QueryParameters;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;
using System.Security.Claims;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // Implement API endpoints for Task operations here

        #region api create task
        [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest createTaskRequest)
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
                // Lấy ID người dùng từ token
                var createdByIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (createdByIdClaim == null)
                {
                    return Unauthorized();
                }

                var createdById = int.Parse(createdByIdClaim.Value);

                // Mapping từ CreateTaskRequest sang CreateTaskModel
                var createTaskModel = new CreateTaskModel
                {
                    TaskName = createTaskRequest.TaskName,
                    Description = createTaskRequest.Description,
                    DueDate = createTaskRequest.DueDate,
                    TaskType = createTaskRequest.TaskType,
                    FarmId = createTaskRequest.FarmId,
                    AssignedToUserId = createTaskRequest.AssignedToUserId,
                    CreatedBy = createdById
                };

                var newTask = await _taskService.CreateTaskAsync(createTaskModel);
                return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, ApiResult<DataAccessObject.Models.Task>.Succeed(newTask));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        #endregion
        #region api get task detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            // Implementation to get task by ID
            try
            {
                var taskDetailModel = await _taskService.GetTaskDetailAsync(id);
                if (taskDetailModel == null)
                {
                    return NotFound();
                }

                // Mapping từ TaskDetailModel sang TaskDetailResponse
                var taskDetailResponse = new TaskDetailResponse
                {
                    Task = taskDetailModel
                };

                return Ok(ApiResult<TaskDetailResponse>.Succeed(taskDetailResponse));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        #endregion
        #region api get all tasks
        [HttpGet]
        public async Task<IActionResult> GetAllTasks([FromQuery] TasksQuery query)
        {
            try
            {
                var paginatedTasks = await _taskService.GetAllTasksAsync(query);
                return Ok(ApiResult<PagedResult<TaskDetailModel>>.Succeed(paginatedTasks));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        #endregion

        #region api update task
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest updateTaskRequest)
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
                // Lấy ID người dùng từ token
                var modifiedByIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (modifiedByIdClaim == null)
                {
                    return Unauthorized();
                }

                var modifiedById = int.Parse(modifiedByIdClaim.Value);

                // Mapping từ UpdateTaskRequest sang UpdateTaskModel
                var updateTaskModel = new UpdateTaskModel
                {
                    TaskName = updateTaskRequest.TaskName,
                    Description = updateTaskRequest.Description,
                    DueDate = updateTaskRequest.DueDate,
                    TaskType = updateTaskRequest.TaskType,
                    FarmId = updateTaskRequest.FarmId,
                    AssignedToUserId = updateTaskRequest.AssignedToUserId,
                    Status = updateTaskRequest.Status,
                    ModifiedBy = modifiedById
                };

                 await _taskService.UpdateTaskAsync(id, updateTaskModel);
                return Ok(ApiResult<string>.Succeed("Update successfully!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        #endregion
        #region api update status task
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusRequest updateTaskStatusRequest)
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
                // Lấy ID người dùng từ token
                var modifiedByIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (modifiedByIdClaim == null)
                {
                    return Unauthorized();
                }

                var modifiedById = int.Parse(modifiedByIdClaim.Value);
                await _taskService.UpdateTaskStatusAsync(id, updateTaskStatusRequest.Status, modifiedById);
                return Ok(ApiResult<string>.Succeed("Update status successfully!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        #endregion
    }
}
