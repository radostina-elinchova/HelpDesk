using HelpDeskApp.ViewModels.Models.Common;
using HelpDeskApp.ViewModels.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface IUserService
    {
        Task<UserQueryVM> GetAllUsersAsync(UserQueryVM queryModel);

        Task<UserDetailsVM?> GetUserDetailsAsync(string userId, string currentAdminId);

        Task<UserDeleteVM?> GetUserDeleteAsync(string userId, string currentAdminId);

        Task<string?> DeleteUserAsync(string userId, string currentAdminId);
    }
}
