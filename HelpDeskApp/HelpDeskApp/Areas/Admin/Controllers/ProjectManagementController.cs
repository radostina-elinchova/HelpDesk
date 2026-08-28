using HelpDeskApp.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Areas.Admin.Controllers
{
    public class ProjectManagementController : BaseAdminController
    {
        private readonly IProjectService _projectService;

        public ProjectManagementController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _projectService.GetAllProjectsAsync(null, true);

            return View(model);
        }
    }
}
