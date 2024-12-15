using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Settings
{
    public class NotificationHub : Hub
    {
        // ConcurrentDictionary để lưu danh sách ConnectionId theo UserId
        private static readonly ConcurrentDictionary<string, List<string>> UserConnections = new();

        public override Task OnConnectedAsync()
        {
            // Lấy UserId từ Context.UserIdentifier (tự động ánh xạ từ token)
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                // Thêm ConnectionId vào danh sách của UserId
                UserConnections.AddOrUpdate(
                    userId,
                    new List<string> { Context.ConnectionId },
                    (key, existingConnections) =>
                    {
                        existingConnections.Add(Context.ConnectionId);
                        return existingConnections;
                    }
                );

                Console.WriteLine($"User connected: {userId} with ConnectionId: {Context.ConnectionId}");
            }
            else
            {
                Console.WriteLine("Kết nối không có UserId!");
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (!connections.Any())
                    {
                        UserConnections.TryRemove(userId, out _);
                    }
                }

                Console.WriteLine($"User disconnected: {userId} with ConnectionId: {Context.ConnectionId}");
            }

            return base.OnDisconnectedAsync(exception);
        }

        public static List<string>? GetConnectionIds(string userId)
        {
            return UserConnections.TryGetValue(userId, out var connectionIds) ? connectionIds : null;
        }
    }
}
