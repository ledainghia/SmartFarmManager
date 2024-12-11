using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Cages;
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
        private readonly ICageService _cageService;

        public UserController(ITaskService taskService, ICageService cageService)
        {
            _taskService = taskService;
            _cageService = cageService;
        }

        [HttpGet("{userId}/tasks")]
        public async Task<IActionResult> GetUserTasks(Guid userId, [FromQuery] DateTime? filterDate)
        {
            try
            {
                var tasksGroupedBySession = await _taskService.GetUserTasksAsync(userId, filterDate);

                if (tasksGroupedBySession == null || tasksGroupedBySession.Count == 0)
                {
                    return NotFound(ApiResult<string>.Fail("No tasks found for the user."));
                }

                return Ok(ApiResult<List<SessionTaskGroupModel>>.Succeed(tasksGroupedBySession));
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

        [HttpGet("{userId}/cages")]
        public async Task<IActionResult> GetUserCages(Guid userId)
        {
            try
            {
                // Gọi service để lấy danh sách cages
                var userCages = await _cageService.GetUserCagesAsync(userId);

                return Ok(ApiResult<List<CageResponseModel>>.Succeed(userCages));
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



    }
}
