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

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(r => r.Id == id);
            if (project == null)
            {
                throw new InvalidOperationException("Destination not found");
            }
            return project;
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
    }
}
