using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Common;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;

namespace HelpDeskApp.Core.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectIndexVM>> GetAllProjectsAsync(string? userId, bool isAdmin)
        {
            var projects = await _projectRepository.GetAllAsync();

            if (!isAdmin)
            {
                projects = projects.Where(p => p.UsersProjects.Any(up => up.UserId == userId));
            }

            return projects.Select(p => new ProjectIndexVM
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                Description = p.Description ?? string.Empty,
                IsFavorite = p.UsersProjects.Any(up => up.UserId == userId && up.IsFavorite)
            }).ToList();
        }

        public async Task<ProjectDetailsVM?> GetProjectDetailsAsync(int projectId)
        {
            var project = await _projectRepository.GetWithRelatedDataAsync(projectId);

            if (project == null)
            {
                return null;
            }

            var model = new ProjectDetailsVM
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                Description = project.Description,
                AssignedUsers = project.UsersProjects.Select(up => new ProjectUserSelectVM
                {
                    Id = up.User.Id,
                    FullName = up.User.UserName ?? up.User.Email
                }).ToList(),
                Tickets = project.Tickets.Select(t => new TicketDetailsVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.TicketStatusName
                }).ToList()
            };

            var assignedUserIds = model.AssignedUsers.Select(a => a.Id);
            var users = await _projectRepository.GetAllUsersAsync();

            model.AvailableUsers = users
                .Where(u => !assignedUserIds.Contains(u.Id))
                .Select(u => new ProjectUserSelectVM
                {
                    Id = u.Id,
                    FullName = u.UserName ?? u.Email
                }).ToList();

            return model;
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

            _projectRepository.Add(project);

            await _projectRepository.SaveChangesAsync();

            return project;
        }

        public async Task EditProjectAsync(ProjectEditVM model)
        {
            var project = await _projectRepository.GetByIdAsync(model.Id);

            if (project == null)
            {
                throw new KeyNotFoundException("Project not found.");
            }

            project.ProjectName = model.ProjectName;
            project.Description = model.Description;

            await _projectRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return false;
            }

            _projectRepository.Remove(project);
            await _projectRepository.SaveChangesAsync();
            return true;
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return null;
            }

            return new Project
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                Description = project.Description ?? string.Empty
            };
        }

        public async Task AssignUserToProjectAsync(int projectId, string userId)
        {
            bool userProjectExists = await _projectRepository.UserProjectExistsAsync(projectId, userId);

            if (userProjectExists)
            {
                throw new InvalidOperationException("User is already assigned to this project.");
            }

            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId
            };

            _projectRepository.AddUserProject(userProject);

            await _projectRepository.SaveChangesAsync();
        }

        public async Task RemoveUserFromProjectAsync(int projectId, string userId)
        {
            var userProject = await _projectRepository.GetUserProjectAsync(projectId, userId);

            if (userProject != null)
            {
                _projectRepository.RemoveUserProject(userProject);
                await _projectRepository.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProjectUserSelectVM>> GetAvailableUsersAsync()
        {
            var users = await _projectRepository.GetAllUsersAsync();

            return users.Select(u => new ProjectUserSelectVM
            {
                Id = u.Id,
                FullName = u.UserName ?? u.Email
            }).ToList();
        }

        public async Task<bool> IsUserInProjectAsync(int projectId, string userId)
        {
            return await _projectRepository.UserProjectExistsAsync(projectId, userId);
        }
        public async Task<ProjectQueryVM> GetAllProjectsAsync(ProjectQueryVM queryModel, string? userId, bool isAdmin)
        {
            queryModel.SearchTerm = string.IsNullOrWhiteSpace(queryModel.SearchTerm)
                    ? null
                    : queryModel.SearchTerm.Trim();

            queryModel.CurrentPage = Math.Max(queryModel.CurrentPage, 1);

            queryModel.PageSize = queryModel.PageSize is 6 or 12 or 24
                ? queryModel.PageSize
                : 6;

            int totalItems = await _projectRepository.GetFilteredCountAsync(userId, isAdmin, queryModel.SearchTerm, queryModel.FavoritesOnly);

            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)queryModel.PageSize));

            queryModel.CurrentPage = Math.Min(queryModel.CurrentPage,  totalPages);

            IEnumerable<Project> projects =
                await _projectRepository.GetFilteredAsync(
                    userId,
                    isAdmin,
                    queryModel.SearchTerm,
                    queryModel.FavoritesOnly,
                    queryModel.CurrentPage,
                    queryModel.PageSize);

            queryModel.Result = new PagedResultVM<ProjectIndexVM>
            {
                Items = projects
                    .Select(p => new ProjectIndexVM
                    {
                        Id = p.Id,
                        ProjectName = p.ProjectName,
                        Description = p.Description ?? string.Empty,

                        IsFavorite = p.UsersProjects.Any(up =>
                            up.UserId == userId &&
                            up.IsFavorite)
                    })
                    .ToList(),

                CurrentPage = queryModel.CurrentPage,
                PageSize = queryModel.PageSize,
                TotalItems = totalItems
            };

            return queryModel;
        }
    }
}