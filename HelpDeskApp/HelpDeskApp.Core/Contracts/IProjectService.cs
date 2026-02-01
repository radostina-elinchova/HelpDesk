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

        Task<Project> ProjectCreateAsync(string name, string? description);      

    

        Task RemoveProjectAsync(int id, string userId);
      

        Task EditProjectAsync(int id, string name, string? description);

        Task DeleteProjectAsync(int id, string userId);
    }
}
