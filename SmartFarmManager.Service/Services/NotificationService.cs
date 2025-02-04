using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Notification;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IHubContext<NotificationHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        public async System.Threading.Tasks.Task SendNotificationToUser(string userId, string message)
        {
            var connectionIds = NotificationHub.GetConnectionIds(userId);
            if (connectionIds != null && connectionIds.Any())
            {
                foreach (var connectionId in connectionIds)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", message);
                    Console.WriteLine($"Đã gửi thông báo tới UserId: {userId}, ConnectionId: {connectionId}, Message: {message}");
                }
            }
            else
            {
                Console.WriteLine($"Không tìm thấy ConnectionId cho UserId: {userId}");
            }
        }
        public async Task<string> SendNotification(string token, string titile, string body)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = titile,
                    Body = body
                },
                Data = new Dictionary<string, string>()
                {
                    { "key1", "value1" }
                }
            };
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }
        public async Task<string> SendNotification(string token, string title, string body, object customData)
        {
            // Serialize custom object thành JSON string
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customData);

            var message = new Message()
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
        {
            { "custom_data", jsonData } // Thêm object vào payload
        }
            };

            // Gửi thông báo qua Firebase
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response; // Trả về ID của message đã gửi
        }

        // Get notifications by userId
        public async Task<IEnumerable<NotificationResponse>> GetNotificationsByUserIdAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications
                .FindAll()
                .Where(n => n.UserId == userId)
                .ToListAsync();

            return notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                UserId = n.UserId,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            });
        }

        // Mark a single notification as read
        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            var notification = await _unitOfWork.Notifications.FindByCondition(n => n.Id == notificationId).FirstOrDefaultAsync();
            if (notification == null) return false;

            notification.IsRead = true;
            await _unitOfWork.Notifications.UpdateAsync(notification);
            return await _unitOfWork.CommitAsync() > 0;
        }

        // Mark all notifications for a user as read
        public async Task<bool> MarkAllNotificationsAsReadAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications
                .FindAll()
                .Where(n => n.UserId == userId && n.IsRead == false)
                .ToListAsync();

            if (!notifications.Any()) return false;

            foreach (var noti in notifications)
            {
                noti.IsRead = true;
            }

            await _unitOfWork.Notifications.UpdateListAsync(notifications);
            return await _unitOfWork.CommitAsync() > 0;
        }

        // Create a new notification
        public async Task<NotificationResponse> CreateNotificationAsync(DataAccessObject.Models.Notification notification)
        {
            notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Notifications.CreateAsync(notification);
            await _unitOfWork.CommitAsync();

            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Content = notification.Content,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }

        // Delete a notification
        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await _unitOfWork.Notifications.FindByCondition(n => n.Id == notificationId).FirstOrDefaultAsync();
            if (notification == null) return false;

            await _unitOfWork.Notifications.DeleteAsync(notification);
            return await _unitOfWork.CommitAsync() > 0;
        }

    }
}
