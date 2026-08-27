using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Services
{
    public class TicketFollowerService: ITicketFollowerService
    {
        private readonly ITicketFollowerRepository _ticketFollowerRepository;
        private readonly ITicketService _ticketService;

        public TicketFollowerService(
            ITicketFollowerRepository ticketFollowerRepository,
            ITicketService ticketService)
        {
            _ticketFollowerRepository = ticketFollowerRepository;
            _ticketService = ticketService;
        }

        public async Task<bool> FollowAsync(int ticketId, string userId, bool isAdmin)
        {
            if (!isAdmin)
            {
                bool canAccess = await _ticketService
                    .CanUserAccessTicketAsync(ticketId, userId);

                if (!canAccess)
                {
                    return false;
                }
            }

            bool exists = await _ticketFollowerRepository
                .ExistsAsync(ticketId, userId);

            if (exists)
            {
                return true;
            }

            var ticketFollower = new TicketFollower
            {
                TicketId = ticketId,
                UserId = userId
            };

            _ticketFollowerRepository.Add(ticketFollower);

            await _ticketFollowerRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnfollowAsync(int ticketId, string userId)
        {
            var ticketFollower = await _ticketFollowerRepository
                .GetAsync(ticketId, userId);

            if (ticketFollower == null)
            {
                return false;
            }

            _ticketFollowerRepository.Remove(ticketFollower);

            await _ticketFollowerRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<TicketListVM>> GetFollowedTicketsAsync(string userId)
        {
            var tickets = await _ticketFollowerRepository
                .GetFollowedTicketsAsync(userId);

            return tickets.Select(t => new TicketListVM
            {
                Id = t.Id,
                Title = t.Title,
                ProjectName = t.Project.ProjectName,
                CreatorName = t.Creator.LastName,
                IsCteator = userId != null && t.CreatorId == userId,
                IsFollowing = true
            }).ToList();
        }
    }
}
