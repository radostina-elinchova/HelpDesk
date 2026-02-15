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


        public async Task<IEnumerable<TicketListVM>> GetAllTicketsAsync(string? userId)
        {
            return await _context.Tickets
                .Select(t => new TicketListVM
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectName = t.Project.ProjectName,
                    CreatorName = t.Creator.LastName,
                    IsCteator = userId != null && t.CreatorId == userId,
                }).ToListAsync();
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
        public async Task<TicketStatusVM> GetTicketOpenStatusAsync()
        {
            var openStatus = await _context.TicketStatus.FirstOrDefaultAsync(s => s.TicketStatusName == "Open");
            var openStatusVM = new TicketStatusVM
            {
                Id = openStatus != null ? openStatus.Id : 0,
                Name = openStatus != null ? openStatus.TicketStatusName : "Open"
            };
            return openStatusVM;
        }
        public async Task CreateTicketAsync(TicketFormVM model)
        {
            var openStatus = await GetTicketOpenStatusAsync();
            var ticket = new Ticket
            {
                Title = model.Title,
                Description = model.Description,
                SubCategoryId = model.SubCategoryId,
                ProjectId = model.ProjectId,
                StatusId = openStatus.Id,
                CreatedOn = DateTime.UtcNow
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync(); 
        }
        public async Task<IEnumerable<CategoryVM>> GetTicketCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryVM
                { 
                    Id = c.Id, Name = c.CategoryName
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
                    Id = s.Id, Name = s.SubCategoryName 
                })
                .ToListAsync();
        }


        //public async Task UpdateAsync(TicketFormViewModel model)
        //{
        //    var ticket = await _context.Tickets.FindAsync(model.Id);
        //    if (ticket != null)
        //    {
        //        ticket.Title = model.Title;
        //        ticket.Description = model.Description;
        //        ticket.AssigneeId = model.AssigneeId;
        //        ticket.SubCategoryId = model.SubCategoryId;
        //        
        //        await _context.SaveChangesAsync();
        //    }
        //}


        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }
    }
}

