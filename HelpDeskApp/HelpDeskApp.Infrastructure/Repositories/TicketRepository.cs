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
    public class TicketRepository: ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Project)
                    .ThenInclude(p => p.UsersProjects)
                .Include(t => t.Creator)
                .Include(t => t.Status)
                .ToListAsync();
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket?> GetWithRelatedDataAsync(int id)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Status)
                .Include(t => t.SubCategory)
                .ThenInclude(sc => sc.Category)
                .Include(t => t.Project)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<SubCategory>> GetAllSubCategoriesAsync()
        {
            return await _context.SubCategories.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<TicketStatus>> GetAllStatusesAsync()
        {
            return await _context.TicketStatus.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<UserProject>> GetAllUserProjectsAsync()
        {
            return await _context.UsersProjects
                .AsNoTracking()
                .Include(up => up.User)
                .ToListAsync();
        }

        public void Add(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
        }

        public void Remove(Ticket ticket)
        {
            _context.Tickets.Remove(ticket);
        }
        public async Task<int?> GetTicketProjectIdAsync(int ticketId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .Where(t => t.Id == ticketId)
                .Select(t => (int?)t.ProjectId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UserProjectExistsAsync(int projectId, string userId)
        {
            return await _context.UsersProjects
                .AsNoTracking()
                .AnyAsync(up =>
                    up.ProjectId == projectId &&
                    up.UserId == userId);
        }
        public async Task<bool> TicketCreatorExistsAsync(int ticketId, string userId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .AnyAsync(t =>
                    t.Id == ticketId &&
                    t.CreatorId == userId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
