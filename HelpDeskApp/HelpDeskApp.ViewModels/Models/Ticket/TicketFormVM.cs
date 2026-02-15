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
        // to do : use the global constants for validation. Do not use magical numbers
        [Required(ErrorMessage = "Заглавието е задължително")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Заглавието трябва да е между 5 и 100 символа")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Описанието трябва да е поне 10 символа")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Моля, изберете категория")]
        [Range(1, int.MaxValue, ErrorMessage = "Избраната категория е невалидна")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Моля, изберете подкатегория")]
        [Range(1, int.MaxValue, ErrorMessage = "Избраната подкатегория е невалидна")]
        public int SubCategoryId { get; set; }

        [Required(ErrorMessage = "Моля, изберете проект")]
        [Range(1, int.MaxValue, ErrorMessage = "Избраният проект е невалиден")]
        public int ProjectId { get; set; }

        [Required]
        public int StatusId { get; set; }
        public string Status { get; set; } = null!;

        public IEnumerable<CategoryVM>? Categories { get; set; } = new HashSet<CategoryVM>();
        public IEnumerable<ProjectIndexVM>? Projects { get; set; } = new HashSet<ProjectIndexVM>();
        public IEnumerable<SubCategoryVM>? SubCategories { get; set; } = new HashSet<SubCategoryVM>();

        public string? AssigneeId { get; set; }

    }
}
