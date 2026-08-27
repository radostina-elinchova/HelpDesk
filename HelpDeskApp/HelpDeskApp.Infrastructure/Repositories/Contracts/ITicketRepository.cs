using HelpDeskApp.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Repositories.Contracts
{
    public interface ITicketRepository
    {

        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket?> GetWithRelatedDataAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<IEnumerable<SubCategory>> GetAllSubCategoriesAsync();
        Task<IEnumerable<TicketStatus>> GetAllStatusesAsync();
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<IEnumerable<UserProject>> GetAllUserProjectsAsync();
        void Add(Ticket ticket);
        void Remove(Ticket ticket);
        Task<int?> GetTicketProjectIdAsync(int ticketId);

        Task<bool> UserProjectExistsAsync(int projectId, string userId);
        Task<bool> TicketCreatorExistsAsync(int ticketId, string userId);
        Task SaveChangesAsync();
    }
}
