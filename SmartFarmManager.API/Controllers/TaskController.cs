using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Task;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;
using System.Security.Claims;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
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
            return Ok();
        }
        #endregion
    }
}
