using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;

namespace HelpDeskApp.Core.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectIndexVM>> GetAllProjectsAsync(string? userId, bool isAdmin)
        {
            var projects = await projectRepository.AllAsync();

            if (!isAdmin)
            {
                projects = projects
                    .Where(p => p.UsersProjects.Any(up => up.UserId == userId));
            }

            return projects
                .Select(p => new ProjectIndexVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description ?? string.Empty
                })
                .ToList();
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await projectRepository.FindAsync(id);

            if (project == null)
            {
                throw new InvalidOperationException("Project not found.");
            }

            return project;
        }

        public async Task<Project> CreateProjectAsync(ProjectCreateVM model)
        {
            var project = new Project
            {
                ProjectName = model.ProjectName,
                Description = model.Description
            };

            foreach (var userId in model.SelectedUserIds)
            {
                project.UsersProjects.Add(new UserProject
                {
                    UserId = userId
                });
            }

            projectRepository.Add(project);

            await projectRepository.SaveChangesAsync();

            return project;
        }

        public async Task EditProjectAsync(ProjectEditVM model)
        {
            var project = await projectRepository.FindAsync(model.Id);

            if (project == null)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to edit this project.");
            }

            project.ProjectName = model.ProjectName;
            project.Description = model.Description;

            await projectRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await projectRepository.FindAsync(id);

            if (project == null)
            {
                return false;
            }

            projectRepository.Remove(project);

            await projectRepository.SaveChangesAsync();

            return true;
        }

        public async Task<ProjectDetailsVM> GetProjectDetailsAsync(int projectId)
        {
            var project = await projectRepository.ReadAsync(projectId);

            if (project == null)
            {
                return null!;
            }

            var model = new ProjectDetailsVM
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                Description = project.Description,

                AssignedUsers = project.UsersProjects
                    .Select(up => new ProjectUserSelectVM
                    {
                        Id = up.User.Id,
                        FullName = up.User.UserName ?? up.User.Email
                    })
                    .ToList(),

                Tickets = project.Tickets
                    .Select(t => new TicketDetailsVM
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Status = t.Status.TicketStatusName
                    })
                    .ToList()
            };

            var assignedUserIds = model.AssignedUsers
                .Select(u => u.Id)
                .ToList();

            var users = await projectRepository.AllUsersAsync();

            model.AvailableUsers = users
                .Where(u => !assignedUserIds.Contains(u.Id))
                .Select(u => new ProjectUserSelectVM
                {
                    Id = u.Id,
                    FullName = u.UserName ?? u.Email
                })
                .ToList();

            return model;
        }

        public async Task AssignUserToProjectAsync(int projectId, string userId)
        {
            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId
            };

            projectRepository.AddMembership(userProject);

            await projectRepository.SaveChangesAsync();
        }

        public async Task RemoveUserFromProjectAsync(int projectId, string userId)
        {
            var userProject = await projectRepository
                .FindMembershipAsync(projectId, userId);

            if (userProject == null)
            {
                return;
            }

            projectRepository.RemoveMembership(userProject);

            await projectRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProjectUserSelectVM>> GetAvailableUsersAsync()
        {
            var users = await projectRepository.AllUsersAsync();

            return users
                .Select(u => new ProjectUserSelectVM
                {
                    Id = u.Id,
                    FullName = u.UserName ?? u.Email
                })
                .ToList();
        }
        public async Task<bool> IsUserInProjectAsync(int projectId, string userId)
        {
            return await projectRepository.MembershipExistsAsync(projectId, userId);
        }
    }
}