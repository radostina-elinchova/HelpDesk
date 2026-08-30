using HelpDeskApp.ViewModels.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface INotificationService
    {
        Task NotifyTicketFollowersAsync(int ticketId, string message);
        Task<IEnumerable<NotificationListVM>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadNotificationsCountAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId);
    }
}
