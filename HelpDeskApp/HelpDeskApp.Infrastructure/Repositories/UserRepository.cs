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
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync(
            string? searchTerm,
            string? role,
            int currentPage,
            int pageSize)
        {
            IQueryable<ApplicationUser> query = BuildQuery(searchTerm, role);

            return await query
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? searchTerm, string? role)
        {
            return await BuildQuery(searchTerm, role).CountAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<int> GetProjectsCountAsync(string userId)
        {
            return await _context.UsersProjects.CountAsync(up => up.UserId == userId);
        }

        public async Task<int> GetCreatedTicketsCountAsync(string userId)
        {
            return await _context.Tickets.CountAsync(t => t.CreatorId == userId);
        }

        public async Task<int> GetAssignedTicketsCountAsync(string userId)
        {
            return await _context.Tickets.CountAsync(t => t.AssigneeId == userId);
        }

        public async Task<int> GetFollowedTicketsCountAsync(string userId)
        {
            return await _context.TicketFollowers.CountAsync(tf => tf.UserId == userId);
        }

        public async Task PrepareForDeletionAsync(string userId)
        {
            var assignedTickets = await _context.Tickets
                .Where(t => t.AssigneeId == userId)
                .ToListAsync();

            foreach (Ticket ticket in assignedTickets)
            {
                ticket.AssigneeId = null;
            }

            var userProjects = await _context.UsersProjects
                .Where(up => up.UserId == userId)
                .ToListAsync();

            var followedTickets = await _context.TicketFollowers
                .Where(tf => tf.UserId == userId)
                .ToListAsync();

            _context.UsersProjects.RemoveRange(userProjects);
            _context.TicketFollowers.RemoveRange(followedTickets);
        }

        public void Remove(ApplicationUser user)
        {
            _context.Users.Remove(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private IQueryable<ApplicationUser> BuildQuery(
            string? searchTerm,
            string? role)
        {
            IQueryable<ApplicationUser> query = _context.Users;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearchTerm = searchTerm.Trim();

                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(normalizedSearchTerm)) ||
                    (u.Email != null && u.Email.Contains(normalizedSearchTerm)) ||
                    u.FirstName.Contains(normalizedSearchTerm) ||
                    u.LastName.Contains(normalizedSearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query =
                    from user in query
                    join userRole in _context.UserRoles
                        on user.Id equals userRole.UserId
                    join identityRole in _context.Roles
                        on userRole.RoleId equals identityRole.Id
                    where identityRole.Name == role
                    select user;
            }

            return query;
        }
    }
}
