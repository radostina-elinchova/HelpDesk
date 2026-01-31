using HelpDeskApp.Common;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class TicketStatus
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string TicketStatusName { get; set; } = null!;
       
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
