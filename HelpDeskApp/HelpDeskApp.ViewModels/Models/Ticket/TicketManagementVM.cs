using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Ticket
{
    public class TicketManagementVM
    {
        public IEnumerable<TicketListVM> Tickets { get; set; }
            = new List<TicketListVM>();

        public IEnumerable<TicketStatusVM> Statuses { get; set; }
            = new List<TicketStatusVM>();
    }
}
