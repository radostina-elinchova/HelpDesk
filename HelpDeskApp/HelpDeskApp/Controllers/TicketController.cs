using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskApp.Controllers
{
    public class TicketController : BaseController
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            string? userId = GetUserId();
            var tickets = await _ticketService.GetAllTicketsAsync(userId);
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            var status = await _ticketService.GetTicketOpenStatusAsync();
            var categories = await _ticketService.GetTicketCategoriesAsync();          
            var allProjects = await _ticketService.GetTicketProjectsAsync();

            var model = new TicketFormVM
            {
                Categories = categories,
                Projects = allProjects,
                StatusId = status.Id,
                Status = status.Name,
                ProjectId = projectId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TicketFormVM model)
        {
            if (!ModelState.IsValid)
            {
                //to do : check if category id is valid before fetching subcategories
                //to do : check if subcategory id is valid before fetching subcategories
                //to do : check if project id is valid before fetching projects
                model.Categories = await _ticketService.GetTicketCategoriesAsync();
                model.Projects = await _ticketService.GetTicketProjectsAsync();

                if (model.CategoryId > 0)
                {
                    model.SubCategories = await _ticketService.GetTicketSubCategoriesAsync(model.CategoryId);
                }

                var status = await _ticketService.GetTicketOpenStatusAsync();
                if (status != null)
                {
                    model.StatusId = status.Id;
                    model.Status = status.Name;
                }

                return View(model);
            }

            var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _ticketService.CreateTicketAsync(model);

            return RedirectToAction("Index", "Home");
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
            if (id <= 0)
            {
                return NotFound();
            }
            var model = await _ticketService.GetTicketByIdAsync(id);

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
