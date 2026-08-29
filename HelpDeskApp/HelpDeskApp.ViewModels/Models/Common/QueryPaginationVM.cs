using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Common
{
    public class QueryPaginationVM
    {
        public string Action { get; set; } = "Index";

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public string? SearchTerm { get; set; }

        public bool FavoritesOnly { get; set; }

        public int? ProjectId { get; set; }

        public int? StatusId { get; set; }
    }
}
