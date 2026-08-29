using HelpDeskApp.ViewModels.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Project
{
    public class ProjectQueryVM
    {
        public string? SearchTerm { get; set; }

        public bool FavoritesOnly { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 6;

        public PagedResultVM<ProjectIndexVM> Result { get; set; }
            = new();
    }
}
