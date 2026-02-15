using HelpDeskApp.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Data.Infrastructure
{
    public static class ApplicationBuilderExtension
    {
        public static async Task<IApplicationBuilder> PrepareDatabase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var services = serviceScope.ServiceProvider;

            await RoleSeeder(services);
            await SeedAdministrator(services);

            var context = services.GetRequiredService<ApplicationDbContext>();
            await SeedStatusAsync(context);
            await SeedCategoriesAsync(context);
            await SeedProjectsAsync(context);

            return app;
        }
        private static async Task RoleSeeder(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Administrator", "Client" };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdministrator(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var user = new ApplicationUser()
                {
                    FirstName = "admin",
                    LastName = "admin",
                    UserName = "admin",
                    Email = "admin@admin.com",
                    Address = "admin address",
                    PhoneNumber = "0888888888"
                };
                var result = await userManager.CreateAsync(user, "Admin123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrator");
                }
            }
        }

        private static async Task SeedStatusAsync(ApplicationDbContext context)
        {
            if (await context.TicketStatus.AnyAsync()) return;

            await context.TicketStatus.AddRangeAsync(new[]
            {
                new TicketStatus { TicketStatusName = "Open" },
                new TicketStatus { TicketStatusName = "In Progress" },
                new TicketStatus { TicketStatusName = "Resolved" },
                new TicketStatus { TicketStatusName = "Closed" }
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedProjectsAsync(ApplicationDbContext context)
        {
            if (await context.Projects.AnyAsync()) return;

            await context.Projects.AddRangeAsync(new[]
            {
                new Project { ProjectName = "Internal Infrastructure", Description = "Maintenance" },
                new Project { ProjectName = "External Support", Description = "Client relations" }
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
            {
                return;
            }

            var hardware = new Category { CategoryName = "Hardware" };
            var software = new Category { CategoryName = "Software" };

            await context.Categories.AddRangeAsync(hardware, software);
            await context.SaveChangesAsync();

            await context.SubCategories.AddRangeAsync(new[]
            {
                new SubCategory { SubCategoryName = "PC/Laptop", CategoryId = hardware.Id },
                new SubCategory { SubCategoryName = "Network", CategoryId = hardware.Id },
                new SubCategory { SubCategoryName = "OS Install", CategoryId = software.Id }
            });

            await context.SaveChangesAsync();
        }
    }
}

