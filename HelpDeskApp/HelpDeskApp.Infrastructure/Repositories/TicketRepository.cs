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
        public async Task<IEnumerable<Ticket>> GetFilteredAsync(string? userId, bool isAdmin, string? searchTerm,  int? projectId, int? statusId,  int currentPage, int pageSize)
        {
            IQueryable<Ticket> query = BuildFilteredQuery(
                userId,
                isAdmin,
                searchTerm,
                projectId,
                statusId);

            return await query
                .AsNoTracking()
                .Include(t => t.Project)
                .Include(t => t.Creator)
                .Include(t => t.Status)
                .OrderByDescending(t => t.CreatedOn)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredCountAsync(string? userId,bool isAdmin,string? searchTerm, int? projectId, int? statusId)
        {
            return await BuildFilteredQuery(userId,isAdmin,searchTerm,projectId,statusId)
                .CountAsync();
        }

        public async Task<IEnumerable<Project>> GetFilterProjectsAsync(
            string? userId,
            bool isAdmin)
        {
            IQueryable<Project> query = _context.Projects.AsNoTracking();

            if (!isAdmin)
            {
                query = query.Where(p =>
                    p.UsersProjects.Any(up => up.UserId == userId));
            }

            return await query
                .OrderBy(p => p.ProjectName)
                .ToListAsync();
        }
        private IQueryable<Ticket> BuildFilteredQuery(
            string? userId,
            bool isAdmin,
            string? searchTerm,
            int? projectId,
            int? statusId)
        {
            IQueryable<Ticket> query = _context.Tickets;

            if (!isAdmin)
            {
                query = query.Where(t =>
                    t.Project.UsersProjects.Any(up => up.UserId == userId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearchTerm = searchTerm.Trim();

                query = query.Where(t =>
                    t.Title.Contains(normalizedSearchTerm) ||
                    t.Description.Contains(normalizedSearchTerm));
            }

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            if (statusId.HasValue)
            {
                query = query.Where(t => t.StatusId == statusId.Value);
            }

            return query;
        }

    }
}
