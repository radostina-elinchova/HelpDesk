using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace HelpDeskApp.Core.Services
{

    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<TicketListVM>> GetAllTicketsAsync(string? userId = null, bool isAdmin = false)
        {
            var tickets = await _ticketRepository.GetAllAsync();

            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                tickets = tickets.Where(t => t.Project.UsersProjects.Any(up => up.UserId == userId));
            }

            return tickets.Select(t => new TicketListVM
            {
                Id = t.Id,
                Title = t.Title,
                ProjectName = t.Project.ProjectName,
                CreatorName = t.Creator.LastName,
                IsCteator = userId != null && t.CreatorId == userId
            }).ToList();
        }

        public async Task<TicketDetailsVM?> GetTicketByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetWithRelatedDataAsync(id);

            if (ticket == null)
            {
                return null;
            }

            return new TicketDetailsVM
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Status = ticket.Status.TicketStatusName,
                Category = ticket.SubCategory.Category.CategoryName,
                CreatorId = ticket.CreatorId
            };
        }

        public async Task<TicketEditVM?> GetTicketEditAsync(int id)
        {
            var ticket = await _ticketRepository.GetWithRelatedDataAsync(id);

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
                AvailableUsers = await GetProjectUsersAsync(ticket.ProjectId)
            };
        }

        public async Task<TicketDeleteVM?> GetTicketDeleteByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetWithRelatedDataAsync(id);

            if (ticket == null)
            {
                return null;
            }

            return new TicketDeleteVM
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Status = ticket.Status.TicketStatusName,
                ProjectName = ticket.Project.ProjectName
            };
        }

        public async Task<TicketStatusVM> GetTicketOpenStatusAsync()
        {
            var statuses = await _ticketRepository.GetAllStatusesAsync();
            var openStatus = statuses.FirstOrDefault(s => s.TicketStatusName == "Open");

            if (openStatus == null)
            {
                throw new InvalidOperationException("Open ticket status is not configured.");
            }

            return new TicketStatusVM
            {
                Id = openStatus.Id,
                Name = openStatus.TicketStatusName
            };
        }

        public async Task CreateTicketAsync(TicketFormVM model, string userId, bool isAdmin)
        {
            var projects = await _ticketRepository.GetAllProjectsAsync();
            bool projectExists = projects.Any(p => p.Id == model.ProjectId);

            if (!projectExists)
            {
                throw new KeyNotFoundException("The selected project does not exist.");
            }

            var userProjects = await _ticketRepository.GetAllUserProjectsAsync();

            if (!isAdmin)
            {
                bool userInProject = userProjects.Any(up =>
                    up.UserId == userId && up.ProjectId == model.ProjectId);

                if (!userInProject)
                {
                    throw new UnauthorizedAccessException("You do not have access to this project.");
                }

                model.AssigneeId = null;
            }

            var subCategories = await _ticketRepository.GetAllSubCategoriesAsync();
            var subCategory = subCategories.FirstOrDefault(sc => sc.Id == model.SubCategoryId);

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
                var users = await _ticketRepository.GetAllUsersAsync();
                bool assigneeExists = users.Any(u => u.Id == model.AssigneeId);

                if (!assigneeExists)
                {
                    throw new KeyNotFoundException("The selected assignee does not exist.");
                }

                bool assigneeInProject = userProjects.Any(up =>
                    up.UserId == model.AssigneeId && up.ProjectId == model.ProjectId);

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

            _ticketRepository.Add(ticket);
            await _ticketRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryVM>> GetTicketCategoriesAsync()
        {
            var categories = await _ticketRepository.GetAllCategoriesAsync();

            return categories.Select(c => new CategoryVM
            {
                Id = c.Id,
                Name = c.CategoryName
            }).ToList();
        }

        public async Task<IEnumerable<ProjectIndexVM>> GetTicketProjectsAsync()
        {
            var projects = await _ticketRepository.GetAllProjectsAsync();

            return projects.Select(p => new ProjectIndexVM
            {
                Id = p.Id,
                ProjectName = p.ProjectName
            }).ToList();
        }

        public async Task<IEnumerable<SubCategoryVM>> GetTicketSubCategoriesAsync(int categoryId)
        {
            var subCategories = await _ticketRepository.GetAllSubCategoriesAsync();

            return subCategories
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new SubCategoryVM
                {
                    Id = s.Id,
                    Name = s.SubCategoryName
                }).ToList();
        }

        public async Task EditTicketAsync(TicketEditVM model)
        {
            var ticket = await _ticketRepository.GetByIdAsync(model.Id);

            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            var projects = await _ticketRepository.GetAllProjectsAsync();
            bool projectExists = projects.Any(p => p.Id == model.ProjectId);

            if (!projectExists)
            {
                throw new KeyNotFoundException("The selected project does not exist.");
            }

            var subCategories = await _ticketRepository.GetAllSubCategoriesAsync();
            var subCategory = subCategories.FirstOrDefault(sc => sc.Id == model.SubCategoryId);

            if (subCategory == null)
            {
                throw new KeyNotFoundException("The selected subcategory does not exist.");
            }

            if (subCategory.CategoryId != model.CategoryId)
            {
                throw new InvalidOperationException("The selected subcategory does not belong to the selected category.");
            }

            var statuses = await _ticketRepository.GetAllStatusesAsync();
            bool statusExists = statuses.Any(s => s.Id == model.StatusId);

            if (!statusExists)
            {
                throw new KeyNotFoundException("The selected status does not exist.");
            }

            if (!string.IsNullOrWhiteSpace(model.AssigneeId))
            {
                var users = await _ticketRepository.GetAllUsersAsync();
                bool assigneeExists = users.Any(u => u.Id == model.AssigneeId);

                if (!assigneeExists)
                {
                    throw new KeyNotFoundException("The selected assignee does not exist.");
                }

                var userProjects = await _ticketRepository.GetAllUserProjectsAsync();
                bool assigneeInProject = userProjects.Any(up =>
                    up.UserId == model.AssigneeId && up.ProjectId == model.ProjectId);

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

            await _ticketRepository.SaveChangesAsync();
        }

        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket != null)
            {
                _ticketRepository.Remove(ticket);
                await _ticketRepository.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProjectUserSelectVM>> GetProjectUsersAsync(int projectId)
        {
            var userProjects = await _ticketRepository.GetAllUserProjectsAsync();

            return userProjects
                .Where(up => up.ProjectId == projectId)
                .Select(up => new ProjectUserSelectVM
                {
                    Id = up.UserId,
                    FullName = up.User.UserName ?? up.User.Email ?? string.Empty
                }).ToList();
        }

        public async Task<bool> CanUserAccessTicketAsync(int ticketId, string userId)
        {
            int? projectId = await _ticketRepository
                .GetTicketProjectIdAsync(ticketId);

            if (projectId == null)
            {
                return false;
            }

            return await _ticketRepository
                .UserProjectExistsAsync(projectId.Value, userId);
        }

        public async Task<bool> IsTicketCreatorAsync(int ticketId, string userId)
        {
            return await _ticketRepository.TicketCreatorExistsAsync(ticketId, userId);
        }
    }
}

