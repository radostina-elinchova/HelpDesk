using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.ViewModels.Models.Ticket
{
    public class TicketFormVM
    {        
        [Required]
        [StringLength(ValidationConstants.TicketTitleMaxLength, MinimumLength = ValidationConstants.TicketTitleMinLength,
            ErrorMessage = "Ticket Name must be between {2} and {1} charachters")]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(ValidationConstants.TicketDescriptionMaxLength, MinimumLength = ValidationConstants.TicketDescriptionMinLength,        
            ErrorMessage = "Ticket Description must be between {2} and {1} charachters")]
        public string Description { get; set; } = null!;
        
        [Range(1, int.MaxValue, ErrorMessage = "Select a category")]
        public int CategoryId { get; set; }
     
        [Range(1, int.MaxValue, ErrorMessage = "Select a subcategory")]
        public int SubCategoryId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Select a project")]
        public int ProjectId { get; set; }

        //[Range(1, int.MaxValue, ErrorMessage = "Select a status")]
        //public int StatusId { get; set; }
        public string Status { get; set; } = null!;

        public IEnumerable<CategoryVM>? Categories { get; set; } = new HashSet<CategoryVM>();
        public IEnumerable<ProjectIndexVM>? Projects { get; set; } = new HashSet<ProjectIndexVM>();
        public IEnumerable<SubCategoryVM>? SubCategories { get; set; } = new HashSet<SubCategoryVM>();
        public IEnumerable<ProjectUserSelectVM>? AvailableUsers { get; set; } = new HashSet<ProjectUserSelectVM>();

        public string? AssigneeId { get; set; }
        //public string CreatorId { get; set; }

    }
}
