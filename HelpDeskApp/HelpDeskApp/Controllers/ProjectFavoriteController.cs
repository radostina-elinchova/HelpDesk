using HelpDeskApp.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class ProjectFavoriteController : BaseController
    {
        private readonly IProjectFavoriteService _projectFavoriteService;

        public ProjectFavoriteController(IProjectFavoriteService projectFavoriteService)
        {
            _projectFavoriteService = projectFavoriteService;
        }
        public async Task<IActionResult> Index()
        {
            string userId = GetUserId()!;

            var projects = await _projectFavoriteService
                .GetFavoriteProjectsAsync(userId);

            return View(projects);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int projectId)
        {
            string userId = GetUserId()!;

            try
            {
                await _projectFavoriteService
                    .AddToFavoritesAsync(projectId, userId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction("Index", "Project");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int projectId)
        {
            string userId = GetUserId()!;

            try
            {
                await _projectFavoriteService.RemoveFromFavoritesAsync(projectId, userId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction("Index", "Project");
        }
    }
}
