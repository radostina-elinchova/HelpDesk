using HelpDeskApp.ViewModels.Models.Common;
using HelpDeskApp.ViewModels.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Ticket
{
    public class TicketQueryVM
    {
        public string? SearchTerm { get; set; }

        public int? ProjectId { get; set; }

        public int? StatusId { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 6;

        public IEnumerable<ProjectIndexVM> Projects { get; set; }
            = new List<ProjectIndexVM>();

        public IEnumerable<TicketStatusVM> Statuses { get; set; }
            = new List<TicketStatusVM>();

        public PagedResultVM<TicketListVM> Result { get; set; }
            = new();
    }
}
