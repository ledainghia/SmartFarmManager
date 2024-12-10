using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public UserController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("{userId}/tasks")]
        public async Task<IActionResult> GetUserTasks(Guid userId)
        {
            try
            {
                // Gọi service để lấy dữ liệu
                var tasksGroupedBySession = await _taskService.GetUserTasksAsync(userId);

                // Nếu không có công việc nào được tìm thấy
                if (tasksGroupedBySession == null || tasksGroupedBySession.Count == 0)
                {
                    return NotFound(ApiResult<string>.Fail("No tasks found for the user."));
                }

                // Trả về danh sách công việc
                return Ok(ApiResult<List<SessionTaskGroupModel>>.Succeed(tasksGroupedBySession));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                // Log lỗi (nếu cần)
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred."));
            }
        }

    }
}
