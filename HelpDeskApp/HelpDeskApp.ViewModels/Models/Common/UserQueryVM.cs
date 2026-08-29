using HelpDeskApp.ViewModels.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Common
{
    public class UserQueryVM
    {
        public string? SearchTerm { get; set; }

        public string? Role { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public PagedResultVM<UserListVM> Result { get; set; }
    }
}