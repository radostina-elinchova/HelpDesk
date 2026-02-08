using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.EntityFrameworkCore;


namespace HelpDeskApp.Core.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectIndexVM>> GetAllProjectsAsync(string? userId)
        {
            return await _context.Projects
                .Select(p => new ProjectIndexVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description ?? string.Empty

                }).ToListAsync();
        }
        public async Task<ProjectDetailsVM> GetProjectDetailsAsync(int projectId)
        {
            var project = await _context.Projects
                .Where(p => p.Id == projectId)
                .Include(p => p.Tickets)
                .ThenInclude(t => t.Status)
                .Select(p => new ProjectDetailsVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description,

                    Tickets = p.Tickets.Select(t => new TicketDetailsVM
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Status = t.Status.TicketStatusName,
                        Category = t.SubCategory.Category.CategoryName
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new InvalidOperationException("Project not found");
            }

            return project;
        }
        public async Task<Project> ProjectCreateAsync(string name, string? description)
        {
            Project item = new Project
            {
                ProjectName = name,
                Description = description
            };

            _context.Projects.Add(item);

            await _context.SaveChangesAsync();

            return item;
        }
        public async Task EditProjectAsync(int id, string name, string? description)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(r => r.Id == id);

            if (project == null)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this project.");
            }
            project.ProjectName = name;
            project.Description = description;

            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await GetProjectByIdAsync(id);

            if (project != null)            {

                _context.Remove(project);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }    
        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(r => r.Id == id);
            if (project == null)
            {
                throw new InvalidOperationException("Destination not found");
            }
            return project;
        }
    }
}
