using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Data.Entities
{
    [PrimaryKey(nameof(UserId), nameof(ProjectId))]
    public class UserProject
    {
        [Required]
        public string UserId { get; set; } = null!;

        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public int ProjectId { get; set; }
       
        public virtual Project Project { get; set; } = null!;
    }
}
