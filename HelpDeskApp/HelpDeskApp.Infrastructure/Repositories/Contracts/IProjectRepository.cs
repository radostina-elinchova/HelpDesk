using HelpDeskApp.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Repositories.Contracts
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<Project?> GetWithRelatedDataAsync(int id);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<UserProject?> GetUserProjectAsync(int projectId, string userId);
        Task<bool> UserProjectExistsAsync(int projectId, string userId);
        void Add(Project project);
        void AddUserProject(UserProject userProject);
        void Remove(Project project);
        void RemoveUserProject(UserProject userProject);
        Task<IEnumerable<Project>> GetFavoriteProjectsAsync(string userId);
        Task SaveChangesAsync();
        Task<IEnumerable<Project>> GetFilteredAsync(string? userId, bool isAdmin, string? searchTerm,  bool favoritesOnly, int currentPage, int pageSize);
        Task<int> GetFilteredCountAsync(string? userId, bool isAdmin,  string? searchTerm, bool favoritesOnly);
        Task<bool> HasTicketsAsync(int projectId);
    }
    }

