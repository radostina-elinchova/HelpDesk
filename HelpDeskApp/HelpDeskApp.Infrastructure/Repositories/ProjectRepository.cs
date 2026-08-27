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
    }
}

