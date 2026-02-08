using HelpDeskApp.Common;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;


namespace HelpDeskApp.ViewModels.Models.Project
{
    public class ProjectEditVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.DefaultNameMaxLength, MinimumLength = ValidationConstants.DefaultNameMinLength)]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; } = null!;

        [StringLength(ValidationConstants.DescriptionMaxLength)]
        [Display(Name = "Project Description")]
        public string Description { get; set; } = null!;
    }
}
