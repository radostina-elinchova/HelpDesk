using HelpDeskApp.ViewModels.Models.Project;
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
        [Required(ErrorMessage = "Заглавието е задължително")]
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int ProjectId { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; } = null!;
        public IEnumerable<CategoryVM>? Categories { get; set; }
        public IEnumerable<ProjectIndexVM>? Projects { get; set; }
        public IEnumerable<SubCategoryVM>? SubCategories { get; set; }
        public string? AssigneeId { get; set; }

    }
}
