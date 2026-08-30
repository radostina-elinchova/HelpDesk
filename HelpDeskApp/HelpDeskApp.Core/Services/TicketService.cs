using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Infrastructure.Data;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Common;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace HelpDeskApp.Core.Services
{

    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketFollowerRepository _ticketFollowerRepository;
        private readonly INotificationService _notificationService;
        public TicketService(
            ITicketRepository ticketRepository, 
            ITicketFollowerRepository ticketFollowerRepository,
            INotificationService notificationService)
        {
            _ticketRepository = ticketRepository;
            _ticketFollowerRepository = ticketFollowerRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<TicketListVM>> GetAllTicketsAsync(string? userId, bool isAdmin)
        {
            var tickets = await _ticketRepository.GetAllAsync();

            if (!isAdmin)
            {
                tickets = tickets
                    .Where(t => t.Project.UsersProjects.Any(up => up.UserId == userId));
            }

            var followedTicketIds = await _ticketFollowerRepository
                .GetFollowedTicketIdsAsync(userId!);

            return tickets
                .Select(t => new TicketListVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectName = t.Project.ProjectName,
                    StatusId = t.StatusId,
                    Status = t.Status.TicketStatusName,
                    IsFollowing = followedTicketIds.Contains(t.Id)
                })
                .ToList();
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

        public async Task EditTicketAsync(TicketEditVM model, bool isAdmin)
        {
            Ticket? ticket = await _ticketRepository.GetByIdAsync(model.Id);

            if (ticket == null)
            {
                throw new KeyNotFoundException( "Ticket not found.");
            }

            if (!isAdmin)
            {
                model.ProjectId = ticket.ProjectId;
                model.StatusId = ticket.StatusId;
                model.AssigneeId = ticket.AssigneeId;
            }

            var projects =  await _ticketRepository.GetAllProjectsAsync();

            bool projectExists =  projects.Any(p => p.Id == model.ProjectId);

            if (!projectExists)
            {
                throw new KeyNotFoundException("The selected project does not exist.");
            }

            var subCategories =  await _ticketRepository.GetAllSubCategoriesAsync();

            SubCategory? subCategory = subCategories.FirstOrDefault(sc =>  sc.Id == model.SubCategoryId);

            if (subCategory == null)
            {
                throw new KeyNotFoundException("The selected subcategory does not exist.");
            }

            if (subCategory.CategoryId != model.CategoryId)
            {
                throw new InvalidOperationException("The selected subcategory does not belong to the selected category.");
            }

            var statuses = await _ticketRepository.GetAllStatusesAsync();

            bool statusExists =  statuses.Any(s => s.Id == model.StatusId);

            if (!statusExists)
            {
                throw new KeyNotFoundException("The selected status does not exist.");
            }
                    
            if (isAdmin && !string.IsNullOrWhiteSpace(model.AssigneeId))
            {
                var users = await _ticketRepository.GetAllUsersAsync();

                bool assigneeExists =  users.Any(u => u.Id == model.AssigneeId);

                if (!assigneeExists)
                {
                    throw new KeyNotFoundException("The selected assignee does not exist.");
                }

                var userProjects = await _ticketRepository.GetAllUserProjectsAsync();

                bool assigneeInProject = userProjects.Any(up =>
                        up.UserId == model.AssigneeId &&
                        up.ProjectId == model.ProjectId);

                if (!assigneeInProject)
                {
                    throw new InvalidOperationException("The assignee must belong the selected project.");
                }
            }

            ticket.Title = model.Title.Trim();
            ticket.Description = model.Description.Trim();
            ticket.ProjectId = model.ProjectId;
            ticket.SubCategoryId = model.SubCategoryId;
            ticket.StatusId = model.StatusId;
            ticket.AssigneeId = model.AssigneeId;

            await _ticketRepository.SaveChangesAsync();

            await _notificationService.NotifyTicketFollowersAsync(
                ticket.Id,
                $"Ticket {ticket.Title} was updated.");
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
        public async Task<IEnumerable<TicketStatusVM>> GetStatusesAsync()
        {
            var statuses = await _ticketRepository.GetAllStatusesAsync();

            return statuses
                .Select(s => new TicketStatusVM
                {
                    Id = s.Id,
                    Name = s.TicketStatusName
                })
                .ToList();
        }
        public async Task ChangeStatusAsync(int ticketId, int statusId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            var statuses = await _ticketRepository.GetAllStatusesAsync();

            TicketStatus? status =  statuses.FirstOrDefault(s => s.Id == statusId);

            if (status == null)
            {
                throw new KeyNotFoundException("The selected status does not exist.");
            }

            if (ticket.StatusId == statusId)
            {
                return;
            }
            ticket.StatusId = statusId;

            await _ticketRepository.SaveChangesAsync();
            await _notificationService.NotifyTicketFollowersAsync(ticketId,
                $"Ticket status was changed to {status.TicketStatusName}.");
        }
        public async Task<TicketQueryVM> GetAllTicketsAsync(TicketQueryVM queryModel,  string? userId, bool isAdmin)
        {
            queryModel.SearchTerm = string.IsNullOrWhiteSpace(queryModel.SearchTerm)
                    ? null
                    : queryModel.SearchTerm.Trim();

            queryModel.CurrentPage = Math.Max(
                queryModel.CurrentPage,
                1);

            queryModel.PageSize =  queryModel.PageSize is 6 or 12 or 24
                    ? queryModel.PageSize
                    : 6;

            int totalItems =
                await _ticketRepository.GetFilteredCountAsync(
                    userId,
                    isAdmin,
                    queryModel.SearchTerm,
                    queryModel.ProjectId,
                    queryModel.StatusId);

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalItems / (double)queryModel.PageSize));

            queryModel.CurrentPage = Math.Min(
                queryModel.CurrentPage,
                totalPages);

            IEnumerable<Ticket> tickets =
                await _ticketRepository.GetFilteredAsync(
                    userId,
                    isAdmin,
                    queryModel.SearchTerm,
                    queryModel.ProjectId,
                    queryModel.StatusId,
                    queryModel.CurrentPage,
                    queryModel.PageSize);

            ICollection<int> followedTicketIds =
                string.IsNullOrWhiteSpace(userId)
                    ? new List<int>()
                    : await _ticketFollowerRepository
                        .GetFollowedTicketIdsAsync(userId);

            queryModel.Projects =
                (await _ticketRepository.GetFilterProjectsAsync(
                    userId,
                    isAdmin))
                .Select(p => new ProjectIndexVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description ?? string.Empty
                })
                .ToList();

            queryModel.Statuses =
                (await _ticketRepository.GetAllStatusesAsync())
                .Select(s => new TicketStatusVM
                {
                    Id = s.Id,
                    Name = s.TicketStatusName
                })
                .ToList();

            queryModel.Result = new PagedResultVM<TicketListVM>
            {
                Items = tickets
                    .Select(t => new TicketListVM
                    {
                        Id = t.Id,
                        Title = t.Title,
                        ProjectName = t.Project.ProjectName,
                        StatusId = t.StatusId,
                        Status = t.Status.TicketStatusName,
                        CreatorId = t.CreatorId,

                        CreatorName =
                            $"{t.Creator.FirstName} {t.Creator.LastName}"
                            .Trim(),

                        IsCteator = t.CreatorId == userId,

                        IsFollowing =
                            followedTicketIds.Contains(t.Id)
                    })
                    .ToList(),

                CurrentPage = queryModel.CurrentPage,
                PageSize = queryModel.PageSize,
                TotalItems = totalItems
            };

            return queryModel;
        }
        public async Task<IEnumerable<ProjectIndexVM>> GetAvailableTicketProjectsAsync(string? userId, bool isAdmin)
        {
            IEnumerable<Project> projects = await _ticketRepository.GetFilterProjectsAsync(userId, isAdmin);

            return projects.Select(p => new ProjectIndexVM
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Description = p.Description ?? string.Empty
                })
                .ToList();
        }
    }
}

