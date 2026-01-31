using HelpDeskApp.Common;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
     
        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(ValidationConstants.AddressMaxLength)]
        public string Address { get; set; } = null!;

        [InverseProperty("Creator")]
        public virtual ICollection<Ticket> CreatedTickets { get; set; } = new HashSet<Ticket>();

        [InverseProperty("Assignee")]
        public virtual ICollection<Ticket> AssignedTickets { get; set; } = new HashSet<Ticket>();
    }
}
