using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;
using SmartFarmManager.API.Common;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Helpers;
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
        private readonly IUserService _userService;

        public UserController(ITaskService taskService, ICageService cageService, IUserService userService)
        {
            _taskService = taskService;
            _cageService = cageService;
            _userService = userService;
        }

        [HttpGet("{userId}/tasks")]
        public async Task<IActionResult> GetUserTasks(Guid userId, [FromQuery] DateTime? filterDate, [FromQuery] Guid? cageId)
        {
            try
            {
                // Gọi service với tham số cageId
                var tasksGroupedBySession = await _taskService.GetUserTasksAsync(userId, filterDate, cageId);

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


        [HttpGet("{userId:guid}/farms")]
        public async Task<IActionResult> GetFarmsByUserId(Guid userId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var paginatedFarms = await _userService.GetFarmsByAdminStaffIdAsync(userId, pageIndex, pageSize);

                if (paginatedFarms == null || !paginatedFarms.Items.Any())
                {
                    return NotFound(ApiResult<string>.Fail("No farms found for the given UserId."));
                }

                return Ok(ApiResult<PaginatedList<FarmModel>>.Succeed(paginatedFarms));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

    }
}
