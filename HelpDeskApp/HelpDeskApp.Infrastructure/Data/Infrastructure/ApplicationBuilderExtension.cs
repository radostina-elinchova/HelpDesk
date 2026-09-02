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
            if (await context.TicketStatus.AnyAsync())
            {
                return;
            }

            await context.TicketStatus.AddRangeAsync(new[]
            {
                new TicketStatus
                { 
                    TicketStatusName = "Open"
                },
                new TicketStatus
                { 
                    TicketStatusName = "In Progress"
                },
                new TicketStatus
                { 
                    TicketStatusName = "Resolved"
                },
                new TicketStatus
                { 
                    TicketStatusName = "Closed"
                }
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedProjectsAsync(ApplicationDbContext context)
        {
            if (await context.Projects.AnyAsync())
            {
                return;
            }

            var projects = new[]
            {
                new Project
                {
                    ProjectName = "Internal IT Support",
                    Description = "Technical support for company employees, workstations, software and user accounts."
                },
                new Project
                {
                    ProjectName = "Network Infrastructure",
                    Description = "Maintenance and support of the company network, Wi-Fi, VPN and internet connectivity."
                },
                new Project
                {
                    ProjectName = "Hardware Maintenance",
                    Description = "Support, repair and replacement of computers, monitors, printers and other equipment."
                },
                new Project
                {
                    ProjectName = "Software Support",
                    Description = "Installation, configuration, updates, licensing and troubleshooting of business software."
                },
                new Project
                {
                    ProjectName = "Accounts and Access",
                    Description = "Management of user accounts, passwords, permissions and authentication problems."
                },
                new Project
                {
                    ProjectName = "Email and Collaboration",
                    Description = "Support for email, shared mailboxes, calendars, Microsoft Teams and online meetings."
                },
                new Project
                {
                    ProjectName = "Remote Work Support",
                    Description = "Technical support for remote employees, VPN connections and remote access."
                }
            };

            await context.Projects.AddRangeAsync(projects);

            await context.SaveChangesAsync();
        }
        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
            {
                return;
            }

            var hardware = new Category
            {
                CategoryName = "Hardware"
            };

            var software = new Category
            {
                CategoryName = "Software"
            };

            var network = new Category
            {
                CategoryName = "Network and Connectivity"
            };

            var account = new Category
            {
                CategoryName = "Accounts and Access"
            };

            var communication = new Category
            {
                CategoryName = "Email and Communication"
            };

            await context.Categories.AddRangeAsync(
                hardware,
                software,
                network,
                account,
                communication);

            await context.SaveChangesAsync();

            var subCategories = new[]
            {
                new SubCategory
                {
                    SubCategoryName = "PC or Laptop",
                    CategoryId = hardware.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Monitor or Peripherals",
                    CategoryId = hardware.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Printer or Scanner",
                    CategoryId = hardware.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Hardware Failure",
                    CategoryId = hardware.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Software Installation",
                    CategoryId = software.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Software Update",
                    CategoryId = software.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Application Error",
                    CategoryId = software.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Operating System",
                    CategoryId = software.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Software License",
                    CategoryId = software.Id
                },

                new SubCategory
                {
                    SubCategoryName = "Internet Connection",
                    CategoryId = network.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Wi-Fi Connection",
                    CategoryId = network.Id
                },
                new SubCategory
                {
                    SubCategoryName = "VPN Access",
                    CategoryId = network.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Local Network",
                    CategoryId = network.Id
                },

                new SubCategory
                {
                    SubCategoryName = "Login Problem",
                    CategoryId = account.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Password Reset",
                    CategoryId = account.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Access Permissions",
                    CategoryId = account.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Multi-Factor Authentication",
                    CategoryId = account.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Email Problem",
                    CategoryId = communication.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Shared Mailbox",
                    CategoryId = communication.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Teams or Video Meetings",
                    CategoryId = communication.Id
                },
                new SubCategory
                {
                    SubCategoryName = "Calendar Problem",
                    CategoryId = communication.Id
                }
            };

            await context.SubCategories.AddRangeAsync(subCategories);

            await context.SaveChangesAsync();
        }
    }   
}

