using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.Auth;
using SmartFarmManager.API.Payloads.Responses.Auth;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;
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

        // Implement API endpoints for Authentication operations
        // 
        #region login
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
                var res =  await _authenticationService.Login(loginRequest.Username, loginRequest.Password);
                var result = new LoginResponse
                {
                    AccessToken = handler.WriteToken(res.Token),
                    RefreshToken = handler.WriteToken(res.RefreshToken)
                };

                return Ok(ApiResult<LoginResponse>.Succeed(result));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex));

            }

        }
        #endregion

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);

            try
            {
                var userProfile = await _userService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return NotFound(ApiResult<string>.Fail("User not found."));
                }

                return Ok(ApiResult<UserProfileModel>.Succeed(userProfile));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex));
            }
        }

    }
}
