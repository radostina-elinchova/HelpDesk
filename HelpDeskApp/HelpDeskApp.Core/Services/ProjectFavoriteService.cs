using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Services
{
    public class ProjectFavoriteService : IProjectFavoriteService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectFavoriteService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> AddToFavoritesAsync(int projectId, string userId)
        {
            var userProject = await _projectRepository.GetUserProjectAsync(projectId, userId);

            if (userProject == null)
            {
                return false;
            }

            userProject.IsFavorite = true;

            await _projectRepository.SaveChangesAsync();

            return true;
        }

        public async Task RemoveFromFavoritesAsync(int projectId, string userId)
        {
            var userProject = await _projectRepository.GetUserProjectAsync(projectId, userId);

            if (userProject == null)
            {
                throw new UnauthorizedAccessException(
                    "You do not have access to this project.");
            }

            userProject.IsFavorite = false;

            await _projectRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProjectIndexVM>> GetFavoriteProjectsAsync(string userId)
        {
            var projects = await _projectRepository.GetFavoriteProjectsAsync(userId);

            return projects
                .Select(p => new ProjectIndexVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description ?? string.Empty
                })
                .ToList();
        }

        public async Task<bool> IsFavoriteAsync(int projectId, string userId)
        {
            var userProject = await _projectRepository.GetUserProjectAsync(projectId, userId);

            return userProject?.IsFavorite ?? false;
        }
    }
}
