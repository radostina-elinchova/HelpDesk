using HelpDeskApp.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskApp.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set;  } = null!;
        public DbSet<SubCategory> SubCategories { get; set;  } = null!;
        public DbSet<Ticket> Tickets { get; set;  } = null!;
        public DbSet<TicketStatus> TicketStatus { get; set;  } = null!;
        public DbSet<Project> Projects { get; set;  } = null!;
        public DbSet<UserProject> UsersProjects { get; set; } = null!;
    }
}
