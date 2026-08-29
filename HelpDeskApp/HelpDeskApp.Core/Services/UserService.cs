using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Common;
using HelpDeskApp.ViewModels.Models.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Services
{
    public class UserService : IUserService
    {
        private const string AdministratorRole = "Administrator";
        private const string ClientRole = "Client";

        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<UserQueryVM> GetAllUsersAsync(UserQueryVM queryModel)
        {
            queryModel.SearchTerm = string.IsNullOrWhiteSpace(queryModel.SearchTerm)
                ? null
                : queryModel.SearchTerm.Trim();

            queryModel.Role = NormalizeRole(queryModel.Role);
            queryModel.CurrentPage = Math.Max(queryModel.CurrentPage, 1);
            queryModel.PageSize = NormalizePageSize(queryModel.PageSize);

            int totalItems = await _userRepository.GetCountAsync(queryModel.SearchTerm, queryModel.Role);

            int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)queryModel.PageSize));

            queryModel.CurrentPage = Math.Min(queryModel.CurrentPage, totalPages);

            var users = await _userRepository.GetAllAsync(
                queryModel.SearchTerm,
                queryModel.Role,
                queryModel.CurrentPage,
                queryModel.PageSize);

            var userModels = new List<UserListVM>();

            foreach (ApplicationUser user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userModels.Add(new UserListVM
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = GetFullName(user),
                    Role = roles.FirstOrDefault() ?? string.Empty
                });
            }

            queryModel.Result = new PagedResultVM<UserListVM>
            {
                Items = userModels,
                CurrentPage = queryModel.CurrentPage,
                PageSize = queryModel.PageSize,
                TotalItems = totalItems
            };

            return queryModel;
        }

        public async Task<UserDetailsVM?> GetUserDetailsAsync(string userId, string currentAdminId)
        {
            ApplicationUser? user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? string.Empty;

            int projectsCount = await _userRepository.GetProjectsCountAsync(userId);
            int createdTicketsCount = await _userRepository.GetCreatedTicketsCountAsync(userId);
            int assignedTicketsCount = await _userRepository.GetAssignedTicketsCountAsync(userId);
            int followedTicketsCount = await _userRepository.GetFollowedTicketsCountAsync(userId);

            string? restrictionMessage = GetDeleteRestrictionMessage(user, role, currentAdminId, createdTicketsCount);

            return new UserDetailsVM
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = GetFullName(user),
                Address = user.Address,
                Role = role,
                ProjectsCount = projectsCount,
                CreatedTicketsCount = createdTicketsCount,
                AssignedTicketsCount = assignedTicketsCount,
                FollowedTicketsCount = followedTicketsCount,
                CanBeDeleted = restrictionMessage == null,
                DeleteRestrictionMessage = restrictionMessage
            };
        }

        public async Task<UserDeleteVM?> GetUserDeleteAsync(string userId, string currentAdminId)
        {
            UserDetailsVM? details = await GetUserDetailsAsync(userId, currentAdminId);

            if (details == null)
            {
                return null;
            }

            return new UserDeleteVM
            {
                Id = details.Id,
                UserName = details.UserName,
                Email = details.Email,
                FullName = details.FullName,
                ProjectsCount = details.ProjectsCount,
                CreatedTicketsCount = details.CreatedTicketsCount,
                AssignedTicketsCount = details.AssignedTicketsCount,
                FollowedTicketsCount = details.FollowedTicketsCount,
                CanBeDeleted = details.CanBeDeleted,
                DeleteRestrictionMessage = details.DeleteRestrictionMessage
            };
        }

        public async Task<string?> DeleteUserAsync(
            string userId,
            string currentAdminId)
        {
            ApplicationUser? user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return "User not found.";
            }

            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? string.Empty;
            int createdTicketsCount = await _userRepository.GetCreatedTicketsCountAsync(userId);

            string? restrictionMessage = GetDeleteRestrictionMessage(
                user,
                role,
                currentAdminId,
                createdTicketsCount);

            if (restrictionMessage != null)
            {
                return restrictionMessage;
            }

            await _userRepository.PrepareForDeletionAsync(userId);
            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();

            return null;
        }

        private static string GetFullName(ApplicationUser user)
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }

        private static string? NormalizeRole(string? role)
        {
            return role is AdministratorRole or ClientRole
                ? role
                : null;
        }

        private static int NormalizePageSize(int pageSize)
        {
            return pageSize is 5 or 10 or 20
                ? pageSize
                : 10;
        }

        private static string? GetDeleteRestrictionMessage(
            ApplicationUser user,
            string role,
            string currentAdminId,
            int createdTicketsCount)
        {
            if (user.Id == currentAdminId)
            {
                return "You cannot delete your own account.";
            }

            if (role == AdministratorRole)
            {
                return "Administrator accounts cannot be deleted from User Management.";
            }

            if (createdTicketsCount > 0)
            {
                return "The user cannot be deleted because they have created tickets. Ticket history must be preserved.";
            }

            return null;
        }
    }
}
