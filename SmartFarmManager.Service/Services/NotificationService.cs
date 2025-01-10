﻿using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using SmartFarmManager.DataAccessObject.Models;
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
    }
}
