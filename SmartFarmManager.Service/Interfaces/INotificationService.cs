using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponse>> GetNotificationsByUserIdAsync(Guid userId);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
        Task<NotificationResponse> CreateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(Guid notificationId);
    }

}
