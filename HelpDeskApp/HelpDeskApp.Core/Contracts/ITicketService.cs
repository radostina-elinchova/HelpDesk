using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketListVM>> GetAllTicketsAsync(string? userId);
        Task<TicketDetailsVM?> GetTicketByIdAsync(int id);
        Task<TicketEditVM?> GetTicketEditAsync(int id);
        Task<TicketDeleteVM?> GetTicketDeleteByIdAsync(int id);
        Task CreateTicketAsync(TicketFormVM model);
        Task EditTicketAsync(TicketEditVM model);
        Task DeleteTicketAsync(int id);
        Task<TicketStatusVM> GetTicketOpenStatusAsync();
        Task<IEnumerable<CategoryVM>> GetTicketCategoriesAsync();
        Task<IEnumerable<ProjectIndexVM>> GetTicketProjectsAsync();
        Task<IEnumerable<SubCategoryVM>> GetTicketSubCategoriesAsync(int categoryId);
        
    }
}
