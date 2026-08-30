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
    public class ProjectRepository: IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.UsersProjects)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetWithRelatedDataAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.UsersProjects)
                .ThenInclude(up => up.User)
                .Include(p => p.Tickets)
                .ThenInclude(t => t.Status)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<UserProject?> GetUserProjectAsync(int projectId, string userId)
        {
            return await _context.UsersProjects
                .FirstOrDefaultAsync(up => up.ProjectId == projectId && up.UserId == userId);
        }

        public async Task<bool> UserProjectExistsAsync(int projectId, string userId)
        {
            return await _context.UsersProjects
                .AsNoTracking()
                .AnyAsync(up => up.ProjectId == projectId && up.UserId == userId);
        }

        public void Add(Project project)
        {
            _context.Projects.Add(project);
        }

        public void AddUserProject(UserProject userProject)
        {
            _context.UsersProjects.Add(userProject);
        }

        public void Remove(Project project)
        {
            _context.Projects.Remove(project);
        }

        public void RemoveUserProject(UserProject userProject)
        {
            _context.UsersProjects.Remove(userProject);
        }
        public async Task<IEnumerable<Project>> GetFavoriteProjectsAsync(string userId)
        {
            return await _context.UsersProjects
                .AsNoTracking()
                .Where(up => up.UserId == userId && up.IsFavorite)
                .Select(up => up.Project)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Project>> GetFilteredAsync(string? userId, bool isAdmin, string? searchTerm, bool favoritesOnly, int currentPage, int pageSize)
        {
            IQueryable<Project> query = BuildFilteredQuery(
                userId,
                isAdmin,
                searchTerm,
                favoritesOnly);

            return await query
                .AsNoTracking()
                .Include(p => p.UsersProjects.Where(up => up.UserId == userId))
                .OrderBy(p => p.ProjectName)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredCountAsync(
            string? userId,
            bool isAdmin,
            string? searchTerm,
            bool favoritesOnly)
        {
            return await BuildFilteredQuery(
                    userId,
                    isAdmin,
                    searchTerm,
                    favoritesOnly)
                .CountAsync();
        }
        private IQueryable<Project> BuildFilteredQuery(
        string? userId,
        bool isAdmin,
        string? searchTerm,
        bool favoritesOnly)
        {
            IQueryable<Project> query = _context.Projects;

            if (!isAdmin)
            {
                query = query.Where(p =>
                    p.UsersProjects.Any(up => up.UserId == userId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearchTerm = searchTerm.Trim();

                query = query.Where(p =>
                    p.ProjectName.Contains(normalizedSearchTerm) ||
                    (p.Description != null &&
                     p.Description.Contains(normalizedSearchTerm)));
            }

            if (favoritesOnly && !string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(p =>
                    p.UsersProjects.Any(up =>
                        up.UserId == userId &&
                        up.IsFavorite));
            }

            return query;
        }
        public async Task<bool> HasTicketsAsync(int projectId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .AnyAsync(t => t.ProjectId == projectId);
        }
    }
}

