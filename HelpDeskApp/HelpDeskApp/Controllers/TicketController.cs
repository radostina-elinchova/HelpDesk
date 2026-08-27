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

        public async Task<IActionResult> Index()
        {
            string? userId = GetUserId();
            bool isAdmin = User.IsInRole("Administrator");
            var tickets = await _ticketService.GetAllTicketsAsync(userId, isAdmin);

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            var status = await _ticketService.GetTicketOpenStatusAsync();

            var categories = await _ticketService.GetTicketCategoriesAsync();

            var projects = await _ticketService.GetTicketProjectsAsync();

            var model = new TicketFormVM
            {
                Categories = categories,
                Projects = projects,
                Status = status.Name,
                ProjectId = projectId
            };

            if (User.IsInRole("Administrator") &&  projectId > 0)
            {
                model.AvailableUsers =  await _ticketService.GetProjectUsersAsync(projectId);
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

                model.Projects = await _ticketService.GetTicketProjectsAsync();

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

                model.Categories =await _ticketService.GetTicketCategoriesAsync();

                model.Projects = await _ticketService.GetTicketProjectsAsync();

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
                ModelState.AddModelError(string.Empty,ex.Message);

                model.Categories = await _ticketService.GetTicketCategoriesAsync();

                model.Projects = await _ticketService.GetTicketProjectsAsync();

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
                return RedirectToAction("Details","Project",
                    new
                    {
                        id = model.ProjectId
                    });
            }

            return RedirectToAction( "Index", "Ticket");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {        
            var ticket = await _ticketService.GetTicketEditAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }         

            return View(ticket);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketEditVM model)        {
          
            if (!ModelState.IsValid)
            {                
                model.Categories = await _ticketService.GetTicketCategoriesAsync();
                model.Projects = await _ticketService.GetTicketProjectsAsync();
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

            model.IsCreator = model.CreatorId == userId;
            if (model == null)
            {
                throw new InvalidOperationException("Destination not found");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var ticket = await _ticketService.GetTicketDeleteByIdAsync(id);

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
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _ticketService.DeleteTicketAsync(id);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }
    }
}
