using System.ComponentModel.DataAnnotations;
using HelpDeskApp.Common;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string ProjectName { get; set; } = null!;

        [MaxLength(ValidationConstants.DescriptionMaxLength)]
        public string? Description { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
        public virtual ICollection<UserProject> UsersProjects { get; set; } = new HashSet<UserProject>();

    }
}
