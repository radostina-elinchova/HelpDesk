using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.ViewModels.Models.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace HelpDeskApp.Controllers
{
    [Authorize]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            string? userId = GetUserId();
            var projects = await _projectService.GetAllProjectsAsync(userId);           

            return View(projects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _projectService.GetProjectDetailsAsync(id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProjectCreateVM
            {                
                AvailableUsers = await _projectService.GetAvailableUsersAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectCreateVM model)
        {
            if (ModelState.IsValid)
            {
                await _projectService.CreateProjectAsync(model);
                return RedirectToAction(nameof(Index));
            }

            model.AvailableUsers = await _projectService.GetAvailableUsersAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //to do: Add it to service
            var item = await _projectService.GetProjectByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            ProjectEditVM updatedProduct = new ProjectEditVM()
            {
                Id = item.Id,
                ProjectName = item.ProjectName,
                Description = item.Description ?? String.Empty,
            };
           
            return View(updatedProduct);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ProjectEditVM model)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {               
                return View(model);
            }

            try
            {
                await _projectService.EditProjectAsync(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            return RedirectToAction("Details", new { id = model.Id });
        }

        //to do: implement soft delete
        //to do: implement soft delete
        //to do: add on delete restrict for projects. Projects with tickets should not be deletable.
        //To do: add it to project service and project controller.
        //To do: add it to project details view - show message if project has tickets.
        //To do: add it to project index view - show message if project has tickets.
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _projectService.GetProjectByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ProjectDeleteVM product = new ProjectDeleteVM()
            {
                Id = item.Id,
                ProjectName = item.ProjectName,
                Description = item.Description,
                
            };
            return View(product);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            var deleted = await _projectService.DeleteProjectAsync(id);

            if (deleted)
            {
                return this.RedirectToAction("Success");
            }
            else
            {
                return View();
            }
        }
        public IActionResult Success()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddUser(int projectId, string userId)
        {
            await _projectService.AssignUserToProjectAsync(projectId, userId);
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUser(int projectId, string userId)
        {
            await _projectService.RemoveUserFromProjectAsync(projectId, userId);
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

    }
}
