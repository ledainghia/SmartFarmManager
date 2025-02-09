using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailTestController : ControllerBase
    {
        private readonly EmailService _emailService;

        public MailTestController(EmailService emailService)
        {
            _emailService = emailService;
        }
        [AllowAnonymous]
        [HttpPost("send-mail")]
        public async Task<IActionResult> SendMail([FromBody] MailData mailData)
        {
            var result = await _emailService.SendEmailAsync(mailData);
            if (!result)
            {
                return BadRequest("Send mail fail");
            }
            return Ok("Send mail success");
        }
    }
}
