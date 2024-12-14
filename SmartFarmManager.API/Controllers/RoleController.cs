using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Responses.Role;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("roles")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRolesAsync();

                var response = roles.Select(role => new RoleResponse
                {
                    RoleId = role.Id,
                    RoleName = role.RoleName
                });

                return Ok(ApiResult<IEnumerable<RoleResponse>>.Succeed(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

    }
}
