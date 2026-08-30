using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public virtual ApplicationUser User { get; set; } = null!;

        public int? TicketId { get; set; }

        public virtual Ticket? Ticket { get; set; }

        [Required]
        [MaxLength(300)]
        public string Message { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public DateTime? ReadOn { get; set; }
    }
}
