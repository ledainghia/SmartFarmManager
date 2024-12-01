using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Task;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        public readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
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
                var taskModel = request.MapToModel();
                var result = await _taskService.CreateTaskAsync(taskModel);
                if (!result)
                {
                    throw new Exception("Error while saving Task!");
                }
               

                return Ok(ApiResult<string>.Succeed("Create Task successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }

        [HttpPost("{taskId}/priority")]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskPriorityRequest request)
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
                
                var updateTaskModel = request.MapToModel();

                // Call the service to update the task
                var result = await _taskService.UpdateTaskPriorityAsync(taskId, updateTaskModel);
                if (!result)
                {
                    throw new Exception("Error while updating Task!");
                }

                return Ok(ApiResult<string>.Succeed("Update Task successfully!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
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


        //change status of task by task id and status id
        [HttpPut("{taskId}/status/{statusId}")]
        public async Task<IActionResult> ChangeTaskStatus(Guid taskId, Guid statusId)
        {
            try
            {
                var result = await _taskService.ChangeTaskStatusAsync(taskId, statusId);
                if (!result)
                {
                    throw new Exception("Error while changing Task status!");
                }

                return Ok(ApiResult<string>.Succeed("Change Task status successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }


        //get tasks filter
        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskFilterRequest filter)
        {
            try
            {
                var result = await _taskService.GetTasksAsync(filter.MapToModel());
                return Ok(ApiResult<string>.Succeed("Change Task status successfully!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }

        [HttpGet("user-tasks-with-priority")]
        public async Task<IActionResult> GetUserTasksWithPriority([FromQuery] Guid userId, [FromQuery] Guid cageId, [FromQuery] DateTime? specificDate = null)
        {
            var tasks = await _taskService.GetTasksForUserWithStateAsync(userId, cageId, specificDate);
            return Ok(tasks);
        }

        [HttpGet("next-task")]
        public async Task<IActionResult> GetNextTask([FromQuery] Guid userId)
        {
            var task = await _taskService.GetNextTaskForUserAsync(userId);

            if (task == null)
            {
                return NotFound(new { message = "No next task found for this user." });
            }

            return Ok(task);
        }

    }
}
