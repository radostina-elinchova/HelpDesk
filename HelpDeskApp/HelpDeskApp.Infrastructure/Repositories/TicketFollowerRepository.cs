using HelpDeskApp.Infrastructure.Data;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Repositories
{
    public class TicketFollowerRepository : ITicketFollowerRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketFollowerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int ticketId, string userId)
        {
            return await _context.TicketFollowers
                .AsNoTracking()
                .AnyAsync(tf => tf.TicketId == ticketId && tf.UserId == userId);
        }

        public async Task<TicketFollower?> GetAsync(int ticketId, string userId)
        {
            return await _context.TicketFollowers
                .FirstOrDefaultAsync(tf => tf.TicketId == ticketId && tf.UserId == userId);
        }

        public async Task<IEnumerable<Ticket>> GetFollowedTicketsAsync(string userId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.Creator)
                .Include(t => t.Status)
                .Where(t => t.TicketFollowers.Any(tf => tf.UserId == userId))
                .ToListAsync();
        }

        public async Task<ICollection<int>> GetFollowedTicketIdsAsync(string userId)
        {
            return await _context.TicketFollowers
                .AsNoTracking()
                .Where(tf => tf.UserId == userId)
                .Select(tf => tf.TicketId)
                .ToListAsync();
        }

        public void Add(TicketFollower ticketFollower)
        {
            _context.TicketFollowers.Add(ticketFollower);
        }

        public void Remove(TicketFollower ticketFollower)
        {
            _context.TicketFollowers.Remove(ticketFollower);
        }
        public async Task<IEnumerable<string>>
    GetFollowerUserIdsAsync(int ticketId)
        {
            return await _context.TicketFollowers
                .AsNoTracking()
                .Where(tf => tf.TicketId == ticketId)
                .Select(tf => tf.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
