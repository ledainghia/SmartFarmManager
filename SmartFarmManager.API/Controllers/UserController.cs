using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MimeKit.Utils;
using Org.BouncyCastle.Security;
using SmartFarmManager.API.Common;
using SmartFarmManager.API.Payloads.Requests.User;
using SmartFarmManager.API.Payloads.Responses;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.BusinessModels.Users;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;
using System.Text.RegularExpressions;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ICageService _cageService;
        private readonly IUserService _userService;
        private readonly IMemoryCache _cache;
        private readonly EmailService _emailService;
        private readonly OTPPhoneService _otpPhoneService;

        public UserController(ITaskService taskService, ICageService cageService, IUserService userService, IMemoryCache cache, EmailService emailService, OTPPhoneService otpPhoneService)
        {
            _taskService = taskService;
            _cageService = cageService;
            _userService = userService;
            _cache = cache;
            _emailService = emailService;
            _otpPhoneService = otpPhoneService;
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

                return Ok(ApiResult<PagedResult<FarmModel>>.Succeed(paginatedFarms));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet("server-time")]
        public IActionResult GetServerTime()
        {
            //var serverTime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));
            var serverTime = DateTimeUtils.GetServerTimeInVietnamTime();
            var currentSession = SessionTime.GetCurrentSession(serverTime.TimeOfDay);
            var response = new
            {
                ServerTime = serverTime,
                CurrentSession = currentSession
            };
            return Ok(ApiResult<object>.Succeed(response));
        }
        [HttpGet("check-timezone")]
        public IActionResult CheckTimeZone()
        {
            return Ok(new
            {
                ServerTime = DateTime.Now,
                UTCTime = DateTime.UtcNow,
                TimeZone = TimeZoneInfo.Local.Id
            });
        }


        [HttpPut("{userId}/device")]
        public async Task<IActionResult> UpdateUserDeviceId(Guid userId, [FromBody] UpdateDeviceIdRequest deviceIdRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResult<Dictionary<string, string[]>>.Error(new Dictionary<string, string[]>
                {
                    { "Errors", errors.ToArray() }
                }));
            }

            try
            {
                var result = await _userService.UpdateUserDeviceIdAsync(userId, deviceIdRequest.DeviceId);

                if (!result)
                {
                    return BadRequest(ApiResult<string>.Fail("Lỗi không thể cập nhật. Vui lòng thử lại"));
                }

                return Ok(ApiResult<string>.Succeed("Cập nhật id thiết bị thành công"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateModel request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return Ok(ApiResult<UserModel>.Succeed(user));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail("An unexpected error occurred. Please contact support."));
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserUpdateModel request)
        {
            var success = await _userService.UpdateUserAsync(userId, request);
            if (!success)
                return BadRequest(ApiResult<object>.Fail("Update failed."));

            return Ok(ApiResult<object>.Succeed("User updated successfully."));
        }

        [HttpPut("{userId}/update-password")]
        public async Task<IActionResult> UpdatePassword(Guid userId, [FromBody] PasswordUpdateModel request)
        {
            var success = await _userService.UpdatePasswordAsync(userId, request);
            if (!success)
                return BadRequest(ApiResult<object>.Fail("Password update failed."));

            return Ok(ApiResult<object>.Succeed("Password updated successfully."));
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var success = await _userService.DeleteUserAsync(userId);
            if (!success)
                return NotFound(ApiResult<object>.Fail("User not found."));

            return Ok(ApiResult<object>.Succeed("User deleted successfully."));
        }

        //[HttpGet]
        //public async Task<IActionResult> GetUsers()
        //{
        //    var users = await _userService.GetUsersAsync();
        //    return Ok(ApiResult<IEnumerable<UserModel>>.Succeed(users));
        //}

        private async Task<bool> SendOtpAsync(string email, string subject)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set(email, otp, TimeSpan.FromMinutes(10));
            var mailData = new MailData
            {
                EmailToId = email,
                EmailToName = email,
                EmailSubject = subject,
                EmailBody = $"Your OTP is: {otp}"
            };

            return await _emailService.SendEmailAsync(mailData);
        }

        [HttpPost("otp/send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (!regex.IsMatch(request.Email))
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Invalid email format."));
                return BadRequest(result);
            }
            var checkCustomer = await _userService.CheckUserByEmail(request.Email, request.UserName);
            if (checkCustomer.Value)
            {
                if (request.IsResend)
                {
                    if (!_cache.TryGetValue(request.Email, out string _))
                    {
                        var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Email not found. Please initiate the forget password process first."));
                        return NotFound(result);
                    }
                }

                var isSend = await SendOtpAsync(request.Email, request.IsResend ? "Resend OTP" : "Reset Password OTP");
                if (isSend)
                {
                    var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP sent successfully." });
                    return Ok(response);
                }
                else
                {
                    return BadRequest(ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "Something wrong!!!" }));
                }

            }
            return NotFound(ApiResult<Dictionary<string, string[]>>.Fail(new Exception("User is not found")));
        }
        [HttpPost("otp/sms/send")]
        public async Task<IActionResult> SendOtpSms([FromBody] SendOptPhoneRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                return BadRequest(ApiResult<SendOtpResponse>.Fail(new Exception("Phone number is required.")));
            }
            var checkCustomer = await _userService.CheckUserByPhone(request.PhoneNumber, request.UserName);
            if (checkCustomer.Value)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                _cache.Set(request.PhoneNumber, otp, TimeSpan.FromMinutes(10));

                bool isSent = await _otpPhoneService.SendOtpViaSmsAsync(request.PhoneNumber, otp);

                if (isSent)
                {
                    var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP sent successfully via SMS." });
                    return Ok(response);
                }
                else
                {
                    return BadRequest(ApiResult<SendOtpResponse>.Fail(new Exception("Failed to send OTP via SMS.")));
                }
            }
            return NotFound(ApiResult<Dictionary<string, string[]>>.Fail(new Exception("User is not found")));
        }


        //[HttpPost("otp/send")]
        //public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        //{
        //    Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        //    if (!regex.IsMatch(request.Email))
        //    {
        //        var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Invalid email format."));
        //        return BadRequest(result);
        //    }

        //    var checkCustomer = await _userService.CheckUserByEmail(request.Email);
        //    if (checkCustomer.Value)
        //    {
        //        string otp;
        //        if (_cache.TryGetValue(request.Email, out otp))
        //        {
        //            if (!request.IsResend)
        //            {
        //                // If not resend but OTP exists, consider invalid scenario
        //                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("OTP already generated. Please wait for it to expire or request a resend."));
        //                return BadRequest(result);
        //            }
        //        }
        //        else
        //        {
        //            // OTP does not exist or expired, generate new one
        //            otp = new Random().Next(100000, 999999).ToString();
        //            _cache.Set(request.Email, otp, TimeSpan.FromMinutes(10));
        //        }

        //        var isSend = await SendOtpAsync(request.Email, request.IsResend ? "Resend OTP" : "Reset Password OTP", otp);

        //        if (isSend)
        //        {
        //            var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP sent successfully." });
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            return BadRequest(ApiResult<SendOtpResponse>.Fail(new Exception("Something went wrong!!!")));
        //        }
        //    }

        //    return NotFound(ApiResult<Dictionary<string, string[]>>.Fail(new Exception("User is not found")));
        //}

        //private async Task<bool> SendOtpAsync(string email, string subject, string otp)
        //{
        //    var mailData = new MailData
        //    {
        //        EmailToId = email,
        //        EmailToName = email,
        //        EmailSubject = subject,
        //        EmailBody = $"Your OTP is: {otp}"
        //    };

        //    return await _emailService.SendEmailAsync(mailData);
        //}
        [HttpPost("otp/verify")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (!regex.IsMatch(request.Email))
            {
                var result = ApiResult<Dictionary<string, string[]>>.Fail(new Exception("Invalid email format."));
                return BadRequest(result);
            }
            if (_cache.TryGetValue(request.Email, out string otp) && otp == request.Otp)
            {
                _cache.Remove(request.Email);
                var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP verified. You can now reset your password." });
                return Ok(response);
            }
            return Unauthorized(ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "Invalid OTP" }));
        }
        [HttpPost("otp/sms/verify")]
        public IActionResult VerifyOtpSms([FromBody] VerifyOtpPhoneRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                return BadRequest(ApiResult<SendOtpResponse>.Fail(new Exception("Phone number is required.")));
            }

            if (_cache.TryGetValue(request.PhoneNumber, out string otp) && otp == request.Otp)
            {
                _cache.Remove(request.PhoneNumber); // Xóa OTP sau khi xác thực thành công
                var response = ApiResult<SendOtpResponse>.Succeed(new SendOtpResponse { Message = "OTP verified successfully via SMS." });
                return Ok(response);
            }

            return Unauthorized(ApiResult<SendOtpResponse>.Fail(new Exception("Invalid OTP.")));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterModel filter)
        {
            var users = await _userService.GetUsersAsync(filter);
            return Ok(ApiResult<PagedResult<UserModel>>.Succeed(users));
        }


        [HttpGet("/filter")]
        public async Task<IActionResult> GetUsers(

        [FromQuery] string? roleName,
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
        {
            var users = await _userService.GetUsersAsync(roleName, isActive, search);
            return Ok(ApiResult<IEnumerable<UserModel>>.Succeed(users));
        }

        /// <summary>
        /// Kiểm tra mật khẩu có đúng không
        /// </summary>
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] UserPasswordRequest request)
        {
            try
            {
                bool isValid = await _userService.VerifyPasswordAsync(request);
                if (!isValid)
                    return BadRequest(ApiResult<string>.Fail("Incorrect password."));

                return Ok(ApiResult<string>.Succeed("Password is correct."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu mới
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserPasswordRequest request)
        {
            try
            {
                bool isReset = await _userService.ResetPasswordAsync(request);
                if (!isReset)
                    return BadRequest(ApiResult<string>.Fail("Failed to reset password."));

                return Ok(ApiResult<string>.Succeed("Password reset successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }
        [HttpPost("checkPhoneCall")]
        public async Task<IActionResult> RequestOtp([FromQuery] string phoneNumber)
        {
            //var otp = _otpService.GenerateOtp();
            var otp = new Random().Next(100000, 999999).ToString();
            Console.WriteLine(otp);
            //await _otpService.SaveOtpAsync(model.PhoneNumber, otp, model.UserName);
            await _otpPhoneService.SendOtpViaSmsAsync(phoneNumber, otp);
            return Ok(new { message = "OTP sent successfully." });
        }

        [HttpPost("assign-staffFarm-cages")]
        public async Task<IActionResult> AssignCages([FromBody] AssignStaffToCagesRequest request)
        {
            try
            {
                var result = await _userService.AssignCagesToStaffAsync(request.StaffId, request.CageIds);
                if (result)
                    return Ok(ApiResult<object>.Succeed("Gán chuồng cho nhân viên thành công."));
                else
                    return BadRequest(ApiResult<object>.Fail("Gán chuồng thất bại."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<object>.Fail(ex));
            }
        }
        [HttpPost("active-inactive/{id}")]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            try
            {
                var result = await _userService.ToggleUserStatusAsync(id);

                if (!result)
                {
                    return BadRequest("Error occurred while toggling user status.");
                }

                return Ok(ApiResult<string>.Succeed("User status updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResult<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<string>.Fail(ex.Message));
            }
        }

    }
}
