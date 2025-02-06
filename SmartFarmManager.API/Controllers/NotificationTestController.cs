using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.DataAccessObject.Models;
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

        [HttpPost("notification-body")]
        [AllowAnonymous]
        public async Task<IActionResult> PushNotification(string Token, string Title)
        {
            // Tạo mẫu dữ liệu Notification
            var sampleNotification = new Notification
            {
                UserId = Guid.NewGuid(),
                NotiTypeId = Guid.NewGuid(),
                Content = "This is a sample notification",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                FarmId = 1,
                CageId = 2
            };

            var response = await _notificationService.SendNotification(Token, Title, sampleNotification);
            return Ok(new { MessageId = response });
        }
    }
}
