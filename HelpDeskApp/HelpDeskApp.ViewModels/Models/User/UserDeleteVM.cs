using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.User
{
    public class UserDeleteVM
    {
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public int ProjectsCount { get; set; }

        public int CreatedTicketsCount { get; set; }

        public int AssignedTicketsCount { get; set; }

        public int FollowedTicketsCount { get; set; }

        public bool CanBeDeleted { get; set; }

        public string? DeleteRestrictionMessage { get; set; }
    }
}
