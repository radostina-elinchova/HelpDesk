using HelpDeskApp.Common;
using System.ComponentModel.DataAnnotations;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class SubCategory
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string SubCategoryName { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

    }
}
