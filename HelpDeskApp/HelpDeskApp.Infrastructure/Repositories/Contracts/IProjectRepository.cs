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

        Task<IEnumerable<Project>> AllAsync();

        Task<Project?> FindAsync(int id);

        Task<Project?> ReadAsync(int id);

        Task<IEnumerable<ApplicationUser>> AllUsersAsync();

        Task<UserProject?> FindMembershipAsync(int projectId, string userId);

        Task<bool> MembershipExistsAsync(int projectId, string userId);

        void Add(Project project);

        void AddMembership(UserProject userProject);

        void Remove(Project project);

        void RemoveMembership(UserProject userProject);

        Task SaveChangesAsync();
    }
    }

