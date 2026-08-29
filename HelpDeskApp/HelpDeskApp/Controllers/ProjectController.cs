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

        public async Task<IActionResult> Index([FromQuery] ProjectQueryVM queryModel)
        {
            string? userId = GetUserId();
            bool isAdmin = User.IsInRole("Administrator");

            ProjectQueryVM model = await _projectService.GetAllProjectsAsync(
                    queryModel,
                    userId,
                    isAdmin);

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var model = await _projectService.GetProjectDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            string userId = GetUserId();
          

            if (!User.IsInRole("Administrator"))
            {
                bool hasAccess = await _projectService.IsUserInProjectAsync(id, userId);

                if (!hasAccess)
                {
                    return Forbid();
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            var model = new ProjectCreateVM
            {                
                AvailableUsers = await _projectService.GetAvailableUsersAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(ProjectCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _projectService.GetAvailableUsersAsync();
                return View(model);              
            }
            await _projectService.CreateProjectAsync(model);
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
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
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
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
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddUser(int projectId, string userId)
        {
            await _projectService.AssignUserToProjectAsync(projectId, userId);
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveUser(int projectId, string userId)
        {
            await _projectService.RemoveUserFromProjectAsync(projectId, userId);
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

    }
}
