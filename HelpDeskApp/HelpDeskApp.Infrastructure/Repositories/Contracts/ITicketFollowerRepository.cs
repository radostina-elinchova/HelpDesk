using HelpDeskApp.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Repositories.Contracts
{
    public interface ITicketFollowerRepository
    {
        Task<bool> ExistsAsync(int ticketId, string userId);

        Task<TicketFollower?> GetAsync(int ticketId, string userId);

        Task<IEnumerable<Ticket>> GetFollowedTicketsAsync(string userId);

        void Add(TicketFollower ticketFollower);

        void Remove(TicketFollower ticketFollower);
        Task<ICollection<int>> GetFollowedTicketIdsAsync(string userId);
        Task<IEnumerable<string>> GetFollowerUserIdsAsync(int ticketId);
        Task SaveChangesAsync();
    }
}
