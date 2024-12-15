using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Services;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("send-test-notification/{userId}")]
        public async Task<IActionResult> SendTestNotification(string userId)
        {
            try
            {
                string testMessage = "Đây là thông báo test từ SignalR!";
                await _notificationService.SendNotificationToUser(userId, testMessage);
                return Ok(new { Success = true, Message = $"Thông báo đã gửi tới userId: {userId}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }
    }
}
