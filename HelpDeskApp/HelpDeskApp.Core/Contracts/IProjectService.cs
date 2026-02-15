using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.ViewModels.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface IProjectService
    {

        Task<IEnumerable<ProjectIndexVM>> GetAllProjectsAsync(string? userId);
        Task<Project> GetProjectByIdAsync(int id);
        Task<Project> CreateProjectAsync(ProjectCreateVM model);        
        Task EditProjectAsync(ProjectEditVM model);
        Task<bool> DeleteProjectAsync(int id);
        Task<ProjectDetailsVM> GetProjectDetailsAsync(int projectId);
        Task AssignUserToProjectAsync(int projectId, string userId);
        Task RemoveUserFromProjectAsync(int projectId, string userId);
        Task<IEnumerable<ProjectUserSelectVM>> GetAvailableUsersAsync();
    }
}
