using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationTestController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationTestController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        //post push notification
        [HttpPost("notification")]
        [AllowAnonymous]
        public async Task<IActionResult> PushNoti(string Token, string Title, string Body)
        {
            var response = await _notificationService.SendNotification(Token, Title, Body);
            return Ok(response);
        }
    }
}
