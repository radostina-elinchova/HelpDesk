using HelpDeskApp.ViewModels.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Core.Contracts
{
    public interface IProjectFavoriteService
    {
        Task<bool> AddToFavoritesAsync(int projectId, string userId);

        Task RemoveFromFavoritesAsync(int projectId, string userId);

        Task<IEnumerable<ProjectIndexVM>> GetFavoriteProjectsAsync(string userId);

        Task<bool> IsFavoriteAsync(int projectId, string userId);
    }
}
