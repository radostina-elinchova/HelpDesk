using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Hubs;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Notification;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Services
{
    public class NotificationService
        : INotificationService
    {
        private readonly ITicketFollowerRepository _ticketFollowerRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ITicketFollowerRepository ticketFollowerRepository,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _ticketFollowerRepository = ticketFollowerRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task NotifyTicketFollowersAsync(int ticketId, string message)
        {
            string[] followerIds = (await _ticketFollowerRepository.GetFollowerUserIdsAsync(ticketId))
                .Distinct()
                .ToArray();

            if (followerIds.Length == 0)
            {
                return;
            }

            var notifications = followerIds
                .Select(userId => new Notification
                {
                    UserId = userId,
                    TicketId = ticketId,
                    Message = message,
                    CreatedOn = DateTime.UtcNow,
                    IsRead = false
                })
                .ToList();

            _notificationRepository.AddRange(notifications);

            await _notificationRepository.SaveChangesAsync();

            await _hubContext.Clients
                .Users(followerIds)
                .SendAsync("ReceiveTicketNotification", ticketId, message);
        }

        public async Task<IEnumerable<NotificationListVM>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetAllByUserIdAsync(userId);

            return notifications
                .Select(n => new NotificationListVM
                {
                    Id = n.Id,
                    TicketId = n.TicketId,
                    Message = n.Message,
                    CreatedOn = n.CreatedOn,
                    IsRead = n.IsRead
                })
                .ToList();
        }

        public async Task<int> GetUnreadNotificationsCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            Notification? notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null || notification.UserId != userId)
            {
                return false;
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadOn = DateTime.UtcNow;

                await _notificationRepository.SaveChangesAsync();
            }

            return true;
        }
    }
}
