using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.ViewModels.Models.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace HelpDeskApp.Controllers
{
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

            var models = projects.Select(project => new ProjectIndexVM
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                Description = project.Description ?? String.Empty,
            }).ToList();

            return View(models);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _projectService.GetProjectDetailsAsync(id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var newProject = await _projectService.ProjectCreateAsync(model.ProjectName, model.Description);


                if (newProject != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
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
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return RedirectToAction("Login", "Account");
            //}

            if (!ModelState.IsValid)
            {               
                return View(model);
            }

            try
            {
                await _projectService.EditProjectAsync(model.Id, model.ProjectName, model.Description);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            return RedirectToAction("Details", new { id = model.Id });
        }

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

        // POST: ProductController/Delete/5
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

    }
}
