using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    [PrimaryKey(nameof(UserId), nameof(TicketId))]
    public class TicketFollower
    {
        [Required]
        public string UserId { get; set; } = null!;

        public virtual ApplicationUser User { get; set; } = null!;

        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; } = null!;

        public DateTime FollowedOn { get; set; } = DateTime.UtcNow;
    }
}
