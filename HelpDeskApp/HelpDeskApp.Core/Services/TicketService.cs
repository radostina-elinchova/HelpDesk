using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace HelpDeskApp.Core.Services
{

    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;

        public TicketService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<TicketListVM>> GetAllTicketsAsync(string? userId = null, bool isAdmin = false)
        {            
            var tickets = _context.Tickets.AsQueryable();
           
            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                tickets = tickets.Where(t => t.CreatorId == userId);
            }
           
            return await tickets
                .Select(t => new TicketListVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectName = t.Project.ProjectName,
                    CreatorName = t.Creator.LastName,
                    IsCteator = userId != null && t.CreatorId == userId,
                })
                .ToListAsync();
        }

        public async Task<TicketDetailsVM?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Where(t => t.Id == id)
                .Select(t => new TicketDetailsVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.TicketStatusName,
                    Category = t.SubCategory.Category.CategoryName,

                }).FirstOrDefaultAsync();
        }

        public async Task<TicketEditVM?> GetTicketEditAsync(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return null;
            }

            return new TicketEditVM
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                CategoryId = ticket.SubCategory.CategoryId,
                SubCategoryId = ticket.SubCategoryId,
                ProjectId = ticket.ProjectId,
                StatusId = ticket.StatusId,
                Status = ticket.Status.TicketStatusName,
                AssigneeId = ticket.AssigneeId,
                Categories = await GetTicketCategoriesAsync(),
                Projects = await GetTicketProjectsAsync(),
                SubCategories = await GetTicketSubCategoriesAsync(ticket.SubCategory.CategoryId),
                AvailableUsers =  await GetProjectUsersAsync(ticket.ProjectId)
            };
        }
        public async Task<TicketDeleteVM?> GetTicketDeleteByIdAsync(int id)
        {
            return await _context.Tickets
                .Where(t => t.Id == id)
                .Select(t => new TicketDeleteVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.TicketStatusName,

                }).FirstOrDefaultAsync();
        }
        //to do: adding all statuses
        public async Task<TicketStatusVM> GetTicketOpenStatusAsync()
        {
            var openStatus = await _context.TicketStatus
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.TicketStatusName == "Open");

            if (openStatus == null)
            {
                throw new InvalidOperationException(
                    "Open ticket status is not configured.");
            }

            return new TicketStatusVM
            {
                Id = openStatus.Id,
                Name = openStatus.TicketStatusName
            };
        }
        public async Task CreateTicketAsync(TicketFormVM model, string userId, bool isAdmin)
        {
            bool projectExists = await _context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == model.ProjectId);

            if (!projectExists)
            {
                throw new KeyNotFoundException(
                    "The selected project does not exist.");
            }

            // Normal user may create tickets only
            // in projects where they are a member.
            if (!isAdmin)
            {
                bool userInProject =
                    await _context.UsersProjects
                        .AsNoTracking()
                        .AnyAsync(up =>
                            up.UserId == userId &&
                            up.ProjectId == model.ProjectId);

                if (!userInProject)
                {
                    throw new UnauthorizedAccessException(
                        "You do not have access to this project.");
                }

                // Ignore forged AssigneeId from a normal user.
                model.AssigneeId = null;
            }

            var subCategory = await _context.SubCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(sc =>
                    sc.Id == model.SubCategoryId);

            if (subCategory == null)
            {
                throw new KeyNotFoundException("The selected subcategory does not exist.");
            }

            if (subCategory.CategoryId != model.CategoryId)
            {
                throw new InvalidOperationException("The selected subcategory does not belong to the selected category.");
            }

            if (isAdmin && !string.IsNullOrWhiteSpace(model.AssigneeId))
            {
                bool assigneeExists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Id == model.AssigneeId);

                if (!assigneeExists)
                {
                    throw new KeyNotFoundException("The selected assignee does not exist.");
                }

                bool assigneeInProject = await _context.UsersProjects
                        .AsNoTracking()
                        .AnyAsync(up =>
                            up.UserId == model.AssigneeId &&
                            up.ProjectId == model.ProjectId);

                if (!assigneeInProject)
                {
                    throw new InvalidOperationException("The assignee must belong to the selected project.");
                }
            }

            var openStatus = await GetTicketOpenStatusAsync();

            var ticket = new Ticket
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                ProjectId = model.ProjectId,
                SubCategoryId = model.SubCategoryId,
                CreatorId = userId,
                AssigneeId = isAdmin ? model.AssigneeId : null,
                StatusId = openStatus.Id,
                CreatedOn = DateTime.UtcNow
            };
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<CategoryVM>> GetTicketCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.CategoryName
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<ProjectIndexVM>> GetTicketProjectsAsync()
        {
            return await _context.Projects
                .Select(c => new ProjectIndexVM
                {
                    Id = c.Id,
                    ProjectName = c.ProjectName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SubCategoryVM>> GetTicketSubCategoriesAsync(int categoryId)
        {
            return await _context.SubCategories
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new SubCategoryVM
                {
                    Id = s.Id,
                    Name = s.SubCategoryName
                })
                .ToListAsync();
        }


        public async Task EditTicketAsync(TicketEditVM model)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t =>t.Id == model.Id);

            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            bool projectExists = await _context.Projects
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == model.ProjectId);

            if (!projectExists)
            {
                throw new KeyNotFoundException("The selected project does not exist.");
            }

            var subCategory = await _context.SubCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(sc =>
                    sc.Id == model.SubCategoryId);

            if (subCategory == null)
            {
                throw new KeyNotFoundException("The selected subcategory does not exist.");
            }

            if (subCategory.CategoryId != model.CategoryId)
            {
                throw new InvalidOperationException("The selected subcategory does not belong to the selected category.");
            }

            bool statusExists = await _context.TicketStatus
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id == model.StatusId);

            if (!statusExists)
            {
                throw new KeyNotFoundException("The selected status does not exist.");
            }

            if (!string.IsNullOrWhiteSpace(
                    model.AssigneeId))
            {
                bool assigneeExists =
                    await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u =>
                            u.Id == model.AssigneeId);

                if (!assigneeExists)
                {
                    throw new KeyNotFoundException("The selected assignee does not exist.");
                }

                bool assigneeInProject =
                    await _context.UsersProjects
                        .AsNoTracking()
                        .AnyAsync(up =>
                            up.UserId == model.AssigneeId &&
                            up.ProjectId == model.ProjectId);

                if (!assigneeInProject)
                {
                    throw new InvalidOperationException("The assignee must belong to the selected project.");
                }
            }

            ticket.Title = model.Title.Trim();
            ticket.Description = model.Description.Trim();
            ticket.ProjectId = model.ProjectId;
            ticket.SubCategoryId = model.SubCategoryId;
            ticket.StatusId = model.StatusId;
            ticket.AssigneeId = model.AssigneeId;
            await _context.SaveChangesAsync();
        }


        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<ProjectUserSelectVM>> GetProjectUsersAsync(int projectId)
        {
            return await _context.UsersProjects
                .AsNoTracking()
                .Where(up => up.ProjectId == projectId)
                .Select(up => new ProjectUserSelectVM
                {
                    Id = up.UserId,
                    FullName = up.User.UserName
                               ?? up.User.Email
                               ?? string.Empty
                })
                .ToListAsync();
        }
    }
}

