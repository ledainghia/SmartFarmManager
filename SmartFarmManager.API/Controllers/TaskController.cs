using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Task;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
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
        [HttpGet]
        public async Task<IActionResult> GetFilteredTasks([FromQuery] TaskFilterPagingRequest filterRequest)
        {
            try
            {
                // Map sang model tầng Service
                var serviceFilter = new TaskFilterModel
                {
                    
                    TaskName = filterRequest.TaskName,
                    Status = filterRequest.Status,
                    TaskTypeId = filterRequest.TaskTypeId,
                    CageId = filterRequest.CageId,
                    AssignedToUserId = filterRequest.AssignedToUserId,
                    DueDateFrom = filterRequest.DueDateFrom,
                    DueDateTo = filterRequest.DueDateTo,
                    PriorityNum = filterRequest.PriorityNum,
                    Session = filterRequest.Session,
                    CompletedAt = filterRequest.CompletedAt,
                    CreatedAt = filterRequest.CreatedAt,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize,
                    

                };

                // Gọi tầng Service để xử lý
                var result = await _taskService.GetFilteredTasksAsync(serviceFilter);

                // Trả về kết quả
                return Ok(ApiResult<PagedResult<TaskDetailModel>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }


        ////get tasks filter
        //[HttpGet]
        //public async Task<IActionResult> GetTasks([FromQuery] TaskFilterRequest filter)
        //{
        //    try
        //    {
        //        var result = await _taskService.GetTasksAsync(filter.MapToModel());
        //        return Ok(ApiResult<string>.Succeed("Change Task status successfully!"));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(ApiResult<string>.Fail(ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
        //    }
        //}

        [HttpGet("user-tasks-with-priority")]
        public async Task<IActionResult> GetUserTasksWithPriority([FromQuery] Guid userId, [FromQuery] Guid cageId, [FromQuery] DateTime? specificDate = null)
        {
            var tasks = await _taskService.GetTasksForUserWithStateAsync(userId, cageId, specificDate);
            return Ok(tasks);
        }

        [HttpGet("next-task")]
        public async Task<IActionResult> GetNextTask([FromQuery] Guid userId)
        {
            var task = await _taskService.GetNextTasksForCagesWithStatsAsync(userId);

            if (task == null)
            {
                return NotFound(new { message = "No next task found for this user." });
            }

            return Ok(task);
        }
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskDetail(Guid taskId)
        {
            try
            {
                var taskDetail = await _taskService.GetTaskDetailAsync(taskId);

                if (taskDetail == null)
                {
                    return NotFound(ApiResult<string>.Fail("Task not found."));
                }

                return Ok(ApiResult<TaskDetailModel>.Succeed(taskDetail));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }


        [HttpPut("update-priorities")]
        public async Task<IActionResult> UpdateTaskPriorities([FromBody] List<TaskPriorityUpdateRequest> taskPriorityUpdates)
        {
            // Kiểm tra tính hợp lệ của request (ở controller)
            if (taskPriorityUpdates == null || !taskPriorityUpdates.Any())
                return BadRequest("The request list cannot be null or empty.");

            try
            {
                // Map request sang DTO của tầng service
                var serviceRequests = taskPriorityUpdates.Select(t => new TaskPriorityUpdateModel
                {
                    TaskId = t.TaskId,
                    PriorityNum = t.PriorityNum
                }).ToList();

                // Gọi service để xử lý logic
                await _taskService.UpdateTaskPrioritiesAsync(serviceRequests);

                return Ok(new { Message = "Task priorities updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

    }
}
