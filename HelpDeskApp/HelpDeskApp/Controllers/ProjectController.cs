using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.Models.Project;
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
                ProjectName = project.ProjectName,
                Description = project.Description ?? String.Empty,
            }).ToList();

            return View(models);
        }

        //[AllowAnonymous]
        //public async Task<IActionResult> Details(int id)
        //{
        //    var recipe = await _recipeService.GetRecipesDetailsByIdAsync(id);
        //    if (recipe == null)
        //    {
        //        if (User?.Identity?.IsAuthenticated == false)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //        return RedirectToAction("Index");
        //    }

        //    string? userId = GetUserId();
        //    recipe.IsAuthor = await _recipeService.IsRecipeAuthorAsync(recipe.Id, userId);
        //    recipe.IsSaved = await _recipeService.IsRecipeSavedAsync(recipe.Id, userId);

        //    return View(recipe);
        //}

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
                var newProject = await _projectService.GetProjectCreateAsync(model.ProjectName, model.Description);


                if (newProject != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }


            return View(model);

        }
    }
}
