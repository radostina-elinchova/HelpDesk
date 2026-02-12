using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Ticket
{
    public class TicketListVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string CreatorName { get; set; } = null!;

    }
}
