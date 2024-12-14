using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Auth;
using SmartFarmManager.API.Payloads.Responses.Auth;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;

        public AuthController(IAuthenticationService authenticationService, IUserService userService)
        {
            _authenticationService = authenticationService;
            _userService = userService;
        }

        // POST: api/auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
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
                var handler = new JwtSecurityTokenHandler();
                var res = await _authenticationService.Login(loginRequest.Username, loginRequest.Password);
                var result = new LoginResponse
                {
                    AccessToken = handler.WriteToken(res.Token),
                    RefreshToken = handler.WriteToken(res.RefreshToken)
                };

                return Ok(ApiResult<LoginResponse>.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        // POST: api/auth/create
        //[Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
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
                var userModel = new CreateUserModel
                {
                    Username = request.Username,
                    Password = request.Password,
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    RoleId = request.RoleId
                };

                var userId = await _userService.CreateUserAsync(userModel);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, ApiResult<object>.Succeed(new { id = userId }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResult<string>.Fail("Unauthorized."));
            }

            var userId = Guid.Parse(userIdClaim);

            try
            {
                var userProfile = await _userService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return NotFound(ApiResult<string>.Fail("User not found."));
                }

                var response = new UserProfileResponse
                {
                    Id = userProfile.Id,
                    Username = userProfile.Username,
                    FullName = userProfile.FullName,
                    Email = userProfile.Email,
                    PhoneNumber = userProfile.PhoneNumber,
                    Address = userProfile.Address,
                    Role = userProfile.Role,
                    CreatedAt = userProfile.CreatedAt
                };

                return Ok(ApiResult<UserProfileResponse>.Succeed(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        // GET: api/auth
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] bool? isActive, [FromQuery] string? search)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(role, isActive, search);

                var response = users.Select(user => new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive
                });

                return Ok(ApiResult<IEnumerable<UserResponse>>.Succeed(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        // GET: api/auth/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResult<string>.Fail("User not found."));
                }

                var response = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive
                };

                return Ok(ApiResult<UserResponse>.Succeed(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpGet("details/{userId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetUserDetails(Guid userId)
        {
            try
            {
                var userDetails = await _userService.GetUserDetailsAsync(userId);
                if (userDetails == null)
                {
                    return NotFound(ApiResult<string>.Fail("User not found or insufficient permissions."));
                }

                return Ok(ApiResult<UserDetailsModel>.Succeed(userDetails));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

    }
}
