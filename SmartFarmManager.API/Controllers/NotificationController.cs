using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Common;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Notification;
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

        // Get notifications by UserId
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotificationsByUserId(Guid userId)
        {
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            if (!notifications.Any())
                return NotFound(ApiResult<object>.Fail("No notifications found."));

            return Ok(ApiResult<IEnumerable<NotificationResponse>>.Succeed(notifications));
        }

        // Mark a single notification as read
        [HttpPut("{notificationId}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(notificationId);
            if (!success)
                return BadRequest(ApiResult<object>.Fail("Notification not found or already read."));

            return Ok(ApiResult<object>.Succeed("Notification marked as read."));
        }

        // Mark all notifications as read for a user
        [HttpPut("{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead(Guid userId)
        {
            var success = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            if (!success)
                return BadRequest(ApiResult<object>.Fail("No unread notifications found."));

            return Ok(ApiResult<object>.Succeed("All notifications marked as read."));
        }

        // Create a new notification
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
        {
            if (notification == null)
                return BadRequest(ApiResult<object>.Fail("Invalid notification data."));

            var createdNotification = await _notificationService.CreateNotificationAsync(notification);
            return Ok(ApiResult<NotificationResponse>.Succeed(createdNotification));
        }

        // Delete a notification
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var success = await _notificationService.DeleteNotificationAsync(notificationId);
            if (!success)
                return NotFound(ApiResult<object>.Fail("Notification not found."));

            return Ok(ApiResult<object>.Succeed("Notification deleted successfully."));
        }
    }
}
