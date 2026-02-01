using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data;
using HelpDeskApp.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddProjectAsync(Project model)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProjectAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }

        public Task EditProjectAsync(Project model, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Project> GeProjectDetailsByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(string? userId)
        {
            return await _context.Projects
                .Select(p => new Project
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description
                })
                .ToListAsync();
        }

        public Task<Project> GetProjectByIdAsync(int id)
        {
            throw new NotImplementedException();
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

        public Task<Project> GetProjectForEditAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProjectAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }

        public Task SaveProjectAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
