using HelpDeskApp.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface IProjectService
    {

        Task<IEnumerable<Project>> GetAllProjectsAsync(string? userId);

        Task<Project> GetProjectByIdAsync(int id);

        Task<Project> GeProjectDetailsByIdAsync(int id);

        Task<Project> GetProjectCreateAsync(string name, string? description);

        Task AddProjectAsync(Project model);

        Task SaveProjectAsync(int id, string userId);

        Task RemoveProjectAsync(int id, string userId);

        Task<Project> GetProjectForEditAsync(int id);

        Task EditProjectAsync(Project model, string userId);

        Task DeleteProjectAsync(int id, string userId);
    }
}
