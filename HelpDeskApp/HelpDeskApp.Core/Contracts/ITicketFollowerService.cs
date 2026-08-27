using HelpDeskApp.ViewModels.Models.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface ITicketFollowerService
    {
        Task<bool> FollowAsync(int ticketId, string userId, bool isAdmin);

        Task<bool> UnfollowAsync(int ticketId, string userId);

        Task<IEnumerable<TicketListVM>> GetFollowedTicketsAsync(string userId);
    }
}
