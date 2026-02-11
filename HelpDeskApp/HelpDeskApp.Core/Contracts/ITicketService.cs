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
        Task<IEnumerable<TicketListVM>> GetAllAsync();
        Task<TicketDetailsVM?> GetByIdAsync(int id);
        Task<TicketDeleteVM?> GetDeleteByIdAsync(int id);
        Task CreateAsync(TicketFormVM model);
        //Task EditAsync(TicketFormVM model);
        Task DeleteAsync(int id);
        Task<TicketStatusVM> GetOpenStatusAsync();
        Task<IEnumerable<CategoryVM>> GetCategoriesAsync();
        Task<IEnumerable<ProjectIndexVM>> GetProjectsAsync();
        Task<IEnumerable<SubCategoryVM>> GetSubCategoriesAsync(int categoryId);
        
    }
}
