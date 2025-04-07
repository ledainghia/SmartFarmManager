using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailTestController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly  IQuartzService _quartzService;

        public MailTestController(EmailService emailService,IQuartzService  quartzService)
        {
            _emailService = emailService;
            _quartzService = quartzService;
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
        [AllowAnonymous]
        [HttpPost("check-quartz")]
        public async Task<IActionResult> CheckQuartz()
        {
            var result = await _quartzService.CheckSchedulerRunningAsync();
            if (!result)
            {
                return BadRequest("Send mail fail");
            }
            return Ok("Send mail success");
        }
    }
}
