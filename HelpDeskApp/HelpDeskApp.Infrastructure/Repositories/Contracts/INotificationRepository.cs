using HelpDeskApp.Infrastructure.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Infrastructure.Repositories.Contracts
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(string userId);

        Task<Notification?> GetByIdAndUserIdAsync(int notificationId, string userId);

        Task<int> GetUnreadCountAsync(string userId);

        void AddRange(IEnumerable<Notification> notifications);

        Task SaveChangesAsync();
    }
}
