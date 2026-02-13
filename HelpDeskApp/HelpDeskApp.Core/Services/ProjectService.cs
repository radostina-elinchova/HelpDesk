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
            var query = _context.Projects.AsNoTracking();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.UsersProjects.Any(up => up.UserId == userId));
            }

            return await query
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
              .Select(p => new ProjectDetailsVM
              {
                  Id = p.Id,
                  ProjectName = p.ProjectName,
                  Description = p.Description,
                  AssignedUsers = p.UsersProjects
                      .Select(up => new ProjectUserSelectVM
                      {
                          Id = up.User.Id,
                          FullName = up.User.UserName ?? up.User.Email
                      }).ToList()
              })
              .FirstOrDefaultAsync();      
                

            var allUsers = await _context.Users
                .Select(u => new ProjectUserSelectVM
                {
                    Id = u.Id,
                    FullName = u.UserName ?? u.Email
                })
                .ToListAsync();
            project.AvailableUsers = allUsers
                .Where(u => !project.AssignedUsers.Any(a => a.Id == u.Id))
                .ToList();

            return project;
        }
        public async Task<Project> ProjectCreateAsync(ProjectCreateVM model)
        {
            Project item = new Project
            {
                ProjectName = model.ProjectName,
                Description = model.Description
            };
            // To Do: Consistent load of the items (for ticket categories and project users)

            foreach (var userId in model.SelectedUserIds)
            {
                item.UsersProjects.Add(new UserProject { UserId = userId });
            }

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

            if (project != null)
            {

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
        // to do: see the approach used in the sample example

        public async Task AssignUserAsync(int projectId, string userId)
        {
            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId
            };

            _context.UsersProjects.Add(userProject);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserAsync(int projectId, string userId)
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
