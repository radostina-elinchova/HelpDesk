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
        public DbSet<TicketFollower> TicketFollowers { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<Ticket>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Ticket>()
                .HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UsersProjects)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TicketFollower>()
                .HasOne(tf => tf.User)
                .WithMany(u => u.TicketFollowers)
                .HasForeignKey(tf => tf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
