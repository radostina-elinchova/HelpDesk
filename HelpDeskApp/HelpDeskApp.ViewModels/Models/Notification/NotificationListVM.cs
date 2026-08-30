using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Notification
{
    public class NotificationListVM
    {
        public int Id { get; set; }

        public int? TicketId { get; set; }

        public string Message { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public bool IsRead { get; set; }
    }
}
