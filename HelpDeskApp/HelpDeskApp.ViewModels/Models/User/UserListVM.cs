using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.User
{
    public class UserListVM
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; }  = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
