using HelpDeskApp.Common;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class Ticket
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(ValidationConstants.DescriptionMaxLength)]
        public string Description { get; set; } = null!;
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Required]
        public string CreatorId { get; set; } = null!;
        public virtual ApplicationUser Creator { get; set; } = null!;

        public string? AssigneeId { get; set; }
        public virtual ApplicationUser? Assignee { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        [Required]
        public int SubCategoryId { get; set; }
        public virtual SubCategory SubCategory { get; set; } = null!;

        [Required]
        public int StatusId { get; set; }
        public virtual TicketStatus Status { get; set; } = null!;
    }
}
