using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Hubs;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ITicketFollowerRepository
            _ticketFollowerRepository;

        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ITicketFollowerRepository ticketFollowerRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _ticketFollowerRepository =  ticketFollowerRepository;
            _hubContext = hubContext;
        }

        public async Task NotifyTicketFollowersAsync(
            int ticketId,
            string message)
        {
            IEnumerable<string> followerIds =
                await _ticketFollowerRepository
                    .GetFollowerUserIdsAsync(ticketId);

            string[] users = followerIds.ToArray();

            if (users.Length == 0)
            {
                return;
            }

            await _hubContext.Clients
                .Users(users)
                .SendAsync("ReceiveTicketNotification", ticketId, message);
        }
    }
}
