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
            var projects = _context.Projects.AsNoTracking();

            //if (!string.IsNullOrEmpty(userId))
            //{
            //    projects = projects.Where(p => p.UsersProjects.Any(up => up.UserId == userId));
            //}

            return await projects
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
                .AsNoTracking()
                .Where(p => p.Id == projectId)
                .Select(p => new ProjectDetailsVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    AssignedUsers = p.UsersProjects.Select(up => new ProjectUserSelectVM
                    {
                        Id = up.User.Id,
                        FullName = up.User.UserName ?? up.User.Email
                    }).ToList(),
                    Tickets = p.Tickets.Select(t => new TicketDetailsVM
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Status = t.Status.TicketStatusName,
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            
            if (project == null)
            {
                return null; 
            }
                       
            var assignedUserIds = project.AssignedUsers.Select(a => a.Id).ToList();

            project.AvailableUsers = await _context.Users
                .AsNoTracking()
                .Where(u => !assignedUserIds.Contains(u.Id))
                .Select(u => new ProjectUserSelectVM
                {
                    Id = u.Id,
                    FullName = u.UserName ?? u.Email
                })
                .ToListAsync();

            return project;
        }
        public async Task<Project> CreateProjectAsync(ProjectCreateVM model)
        {
            Project item = new Project
            {
                ProjectName = model.ProjectName,
                Description = model.Description
            };
            // To Do: Consistent load of the items (for ticket categories and project users) - return view model

            foreach (var userId in model.SelectedUserIds)
            {
                item.UsersProjects.Add(new UserProject { UserId = userId });
            }

            _context.Projects.Add(item);

            await _context.SaveChangesAsync();


            return item;
        }
        public async Task EditProjectAsync(ProjectEditVM model)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(r => r.Id == model.Id);

            if (project == null)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this project.");
            }
            project.ProjectName = model.ProjectName;
            project.Description = model.Description;

            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await GetProjectByIdAsync(id);

            if (project != null)
            {

                _context.Remove(project);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        // To Do: Consistent load of the items (for ticket categories and project users) - return view model
        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(r => r.Id == id);
            if (project == null)
            {
                throw new InvalidOperationException("Destination not found");
            }
            return project;
        }
        

        public async Task AssignUserToProjectAsync(int projectId, string userId)
        {
            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId
            };

            _context.UsersProjects.Add(userProject);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserFromProjectAsync(int projectId, string userId)
        {
            var entry = await _context.UsersProjects
                .FirstOrDefaultAsync(up => up.ProjectId == projectId && up.UserId == userId);

            if (entry != null)
            {
                _context.UsersProjects.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<ProjectUserSelectVM>> GetAvailableUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new ProjectUserSelectVM
                {
                    Id = u.Id,
                    FullName = u.UserName ?? u.Email
                })
                .ToListAsync();
        }


    }
}
