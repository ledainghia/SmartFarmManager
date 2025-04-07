﻿using Microsoft.AspNetCore.Http;
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
        [HttpPost("create-recurring-task")]
        public async Task<IActionResult> CreateTaskRecurring([FromBody] CreateTaskRecurringRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
        {
            { "Lỗi", errors.ToArray() }
        }));
            }

            try
            {
                var model = request.MapToModel();
                var result = await _taskService.CreateTaskRecurringAsync(model);

                if (!result)
                {
                    return BadRequest(ApiResult<string>.Fail("Không thể tạo nhiệm vụ lặp lại. Vui lòng thử lại."));
                }

                return Ok(ApiResult<string>.Succeed("Nhiệm vụ lặp lại đã được tạo thành công!"));
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
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Thu thập lỗi xác thực
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();

                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
        {
            { "Lỗi", errors.ToArray() }
        }));
            }

            try
            {
                var taskModel = request.MapToModel();
                var result = await _taskService.CreateTaskAsync(taskModel);
                if (!result)
                {
                    throw new Exception("Có lỗi xảy ra trong quá trình lưu nhiệm vụ!");
                }

                return Ok(ApiResult<string>.Succeed("Tạo nhiệm vụ thành công!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message)); // Trả về lỗi trùng lặp
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
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
        [HttpPut("{taskId}/status/{status}")]
        public async Task<IActionResult> ChangeTaskStatus(Guid taskId, string status)
        {
            try
            {
                var result = await _taskService.ChangeTaskStatusAsync(taskId, status);
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
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
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
                    
                    KeySearch = filterRequest.KeySearch,
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
        [HttpPost("generate-tasks")]
        public async Task<IActionResult> GenerateTasksForToday()
        {
            try
            {
                var result = await _taskService.GenerateTasksForTomorrowAsync();

                if (!result)
                {
                    return StatusCode(500, ApiResult<string>.Fail("Failed to generate tasks for today."));
                }

                return Ok(ApiResult<string>.Succeed("Tasks generated successfully for today."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
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
        [HttpPost("update-task-status")]
        public async Task<IActionResult> UpdateAllTaskStatuses()
        {
            try
            {
                await _taskService.UpdateAllTaskStatusesAsync();
                return Ok(ApiResult<string>.Succeed("Task statuses updated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("tasks/count-by-status")]
        public async Task<IActionResult> GetTaskCountByStatus([FromQuery] DateTime startDate,[FromQuery] DateTime endDate,[FromQuery] Guid? assignedToUserId = null,[FromQuery] Guid? farmId = null)
        {
            try
            {
                var taskCounts = await _taskService.GetTaskCountByStatusAsync(startDate, endDate, assignedToUserId, farmId);
                return Ok(ApiResult<Dictionary<string, int>>.Succeed(taskCounts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpPut("{taskId}/set-treatment")]
        public async Task<IActionResult> MarkAsTreatmentTask(Guid taskId, [FromQuery]Guid medicalSymptomId)
        {
            var result = await _taskService.SetIsTreatmentTaskTrueAsync(taskId,medicalSymptomId);

            if (!result)
                return NotFound(ApiResult<object>.Fail("Task not found or update failed."));

            return Ok(ApiResult<object>.Succeed("Task marked as treatment task successfully."));
        }

        [HttpGet("{taskId}/logs")]
        public async Task<IActionResult> GetLogsByTaskId(Guid taskId)
        {
            try
            {
                var logs = await _taskService.GetLogsByTaskIdAsync(taskId);
                return Ok(ApiResult<TaskLogResponse>.Succeed(logs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"{ex.Message}"));
            }
        }

    }
}
