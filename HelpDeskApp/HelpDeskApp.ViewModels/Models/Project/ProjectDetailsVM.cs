using HelpDeskApp.Common;
using HelpDeskApp.ViewModels.Models.Ticket;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HelpDeskApp.ViewModels.Models.Project
{
    public class ProjectDetailsVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.DefaultNameMaxLength, MinimumLength = ValidationConstants.DefaultNameMinLength)]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; } = null!;

        [StringLength(ValidationConstants.DescriptionMaxLength)]
        [Display(Name = "Project Description")]
        public string Description { get; set; } = null!;
        public virtual ICollection<TicketDetailsVM> Tickets { get; set; } = new HashSet<TicketDetailsVM>();
        public virtual ICollection<ProjectUserSelectVM> AssignedUsers { get; set; } = new HashSet<ProjectUserSelectVM>();
        public virtual ICollection<ProjectUserSelectVM> AvailableUsers { get; set; } = new HashSet<ProjectUserSelectVM>();

        public string? SelectedUserId { get; set; }

    }
}

