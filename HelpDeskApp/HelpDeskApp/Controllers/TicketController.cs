using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskApp.Controllers
{
    [Authorize]
    public class TicketController : BaseController
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task<IActionResult> Index([FromQuery] TicketQueryVM queryModel)
        {
            string? userId = GetUserId();
            bool isAdmin = User.IsInRole("Administrator");

            TicketQueryVM model = await _ticketService.GetAllTicketsAsync(queryModel, userId, isAdmin);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId = 0)
        {
            string? userId = GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Administrator");

            var projects = await _ticketService.GetAvailableTicketProjectsAsync(userId, isAdmin);

            if (projectId > 0 && !projects.Any(p => p.Id == projectId))
            {
                return isAdmin ? NotFound() : Forbid();
            }
            var status = await _ticketService.GetTicketOpenStatusAsync();

            var categories = await _ticketService.GetTicketCategoriesAsync();            

            var model = new TicketFormVM
            {
                Categories = categories,
                Projects = projects,
                Status = status.Name,
                ProjectId = projectId
            };

            if (User.IsInRole("Administrator") && projectId > 0)
            {
                model.AvailableUsers = await _ticketService.GetProjectUsersAsync(projectId);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketFormVM model, bool fromProject)
        {
            string? userId = GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Administrator");

            if (!isAdmin)
            {
                model.AssigneeId = null;
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _ticketService.GetTicketCategoriesAsync();

                model.Projects = await _ticketService.GetAvailableTicketProjectsAsync(userId, isAdmin);

                if (model.CategoryId > 0)
                {
                    model.SubCategories = await _ticketService.GetTicketSubCategoriesAsync(model.CategoryId);
                }

                if (isAdmin && model.ProjectId > 0)
                {
                    model.AvailableUsers = await _ticketService.GetProjectUsersAsync(model.ProjectId);
                }

                model.Status = "Open";

                return View(model);
            }

            try
            {
                await _ticketService.CreateTicketAsync(model, userId, isAdmin);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                model.Categories = await _ticketService.GetTicketCategoriesAsync();

                model.Projects = await _ticketService.GetAvailableTicketProjectsAsync(userId, isAdmin);

                if (model.CategoryId > 0)
                {
                    model.SubCategories = await _ticketService.GetTicketSubCategoriesAsync(model.CategoryId);
                }

                if (isAdmin && model.ProjectId > 0)
                {
                    model.AvailableUsers = await _ticketService.GetProjectUsersAsync(model.ProjectId);
                }

                model.Status = "Open";

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                model.Categories = await _ticketService.GetTicketCategoriesAsync();

                model.Projects = await _ticketService.GetAvailableTicketProjectsAsync(userId, isAdmin);

                if (model.CategoryId > 0)
                {
                    model.SubCategories = await _ticketService.GetTicketSubCategoriesAsync(model.CategoryId);
                }

                if (isAdmin && model.ProjectId > 0)
                {
                    model.AvailableUsers = await _ticketService.GetProjectUsersAsync(model.ProjectId);
                }

                model.Status = "Open";

                return View(model);
            }

            if (fromProject)
            {
                return RedirectToAction("Details", "Project",
                    new
                    {
                        id = model.ProjectId
                    });
            }

            return RedirectToAction("Index", "Ticket");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var ticket = await _ticketService.GetTicketEditAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }
            string userId = GetUserId()!;

            if (!User.IsInRole("Administrator"))
            {
                bool isCreator = await _ticketService.IsTicketCreatorAsync(id, userId);

                if (!isCreator)
                {
                    return Forbid();
                }
            }
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditVM model)
        {
            string userId = GetUserId()!;

            if (!User.IsInRole("Administrator"))
            {
                bool isCreator = await _ticketService.IsTicketCreatorAsync(model.Id, userId);

                if (!isCreator)
                {
                    return Forbid();
                }
            }


            if (!ModelState.IsValid)
            {
                model.Categories = await _ticketService.GetTicketCategoriesAsync();

                model.Projects = await _ticketService.GetTicketProjectsAsync();

                if (model.CategoryId > 0)
                {
                    model.SubCategories = await _ticketService.GetTicketSubCategoriesAsync(model.CategoryId);
                }

                if (model.ProjectId > 0)
                {
                    model.AvailableUsers = await _ticketService.GetProjectUsersAsync(model.ProjectId);
                }

                return View(model);
            }

            try
            {
                await _ticketService.EditTicketAsync(model);
                return RedirectToAction("Index", "Ticket");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Възникна грешка при записването на промените.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSubCategories(int categoryId)
        {
            if (categoryId <= 0)
            {
                return Json(new List<object>());
            }

            var subCategories = await _ticketService.GetTicketSubCategoriesAsync(categoryId);
            return Json(subCategories);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            string? userId = GetUserId();
            if (id <= 0)
            {
                return NotFound();
            }

            var model = await _ticketService.GetTicketByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            if (!User.IsInRole("Administrator"))
            {
                bool canAccess = await _ticketService.CanUserAccessTicketAsync(id, userId);

                if (!canAccess)
                {
                    return Forbid();
                }
            }
            model.IsCreator = model.CreatorId == userId;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
           
            var ticket = await _ticketService.GetTicketDeleteByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            string userId = GetUserId()!;

            if (!User.IsInRole("Administrator"))
            {
                bool isCreator = await _ticketService.IsTicketCreatorAsync(id, userId);

                if (!isCreator)
                {
                    return Forbid();
                }
            }
            return View(ticket);
        }
        //to do: implement soft delete
        //to do: add on delete restrict for projects. Projects with tickets should not be deletable.
        //To do: add it to project service and project controller.
        //To do: add it to project details view - show message if project has tickets.
        //To do: add it to project index view - show message if project has tickets.

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            string userId = GetUserId()!;

            if (!User.IsInRole("Administrator"))
            {
                bool isCreator = await _ticketService.IsTicketCreatorAsync(id, userId);

                if (!isCreator)
                {
                    return Forbid();
                }
            }

            try
            {
                await _ticketService.DeleteTicketAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
