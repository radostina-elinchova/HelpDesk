using System.ComponentModel.DataAnnotations;
using HelpDeskApp.Common;


namespace HelpDeskApp.Infrastructure.Data.Entities
{
    public class Category
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.DefaultNameMaxLength)]
        public string CategoryName { get; set; } = null!;

        public virtual ICollection<SubCategory> SubCategories { get; set; } = new HashSet<SubCategory>();

    }
}
