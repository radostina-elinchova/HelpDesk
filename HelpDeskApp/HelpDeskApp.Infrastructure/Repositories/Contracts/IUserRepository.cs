using HelpDeskApp.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync(
            string? searchTerm,
            string? role,
            int currentPage,
            int pageSize);

        Task<int> GetCountAsync(string? searchTerm, string? role);

        Task<ApplicationUser?> GetByIdAsync(string userId);

        Task<int> GetProjectsCountAsync(string userId);

        Task<int> GetCreatedTicketsCountAsync(string userId);

        Task<int> GetAssignedTicketsCountAsync(string userId);

        Task<int> GetFollowedTicketsCountAsync(string userId);

        Task PrepareForDeletionAsync(string userId);

        void Remove(ApplicationUser user);

        Task SaveChangesAsync();
    }
}
