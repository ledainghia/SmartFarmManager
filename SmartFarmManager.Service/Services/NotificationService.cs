using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public class NotificationService 
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
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
        //public async Task<string> SendNotification(string token, string title, object customData)
        //{
        //    // Serialize custom object thành JSON string
        //    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customData);

        //    var message = new Message()
        //    {
        //        Token = token,
        //        Notification = new FirebaseAdmin.Messaging.Notification()
        //        {
        //            Title = title,
        //            Body = jsonData // Đưa toàn bộ customData vào Body
        //        },
        //    };

        //    // Gửi thông báo qua Firebase
        //    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        //    return response; // Trả về ID của message đã gửi
        //}

        public async Task<string> SendNotification(string token, string title, object customData)
        {
            try
            {

                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customData);

                var message = new Message()
                {
                    Token = token,
                    Data = new Dictionary<string, string>()
            {
                { "title", title },
                { "customData", jsonData }
            }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return response;
            }
            catch (FirebaseException ex)
            {
                Console.WriteLine($"⛔ Lỗi gửi Notification: {ex.Message}");
                throw;
            }
        }

    }
}
