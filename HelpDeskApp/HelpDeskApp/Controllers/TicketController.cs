using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskApp.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task<IActionResult> Index()
        {
            //string? userId = GetUserId();
            var tickets = await _ticketService.GetAllAsync();
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var status = await _ticketService.GetOpenStatusAsync();
            var model = new TicketFormVM
            {
                Categories = await _ticketService.GetCategoriesAsync(),
                Projects = await _ticketService.GetProjectsAsync(),
                StatusId = status.Id,
                Status = status.Name
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TicketFormVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _ticketService.GetCategoriesAsync();
                model.Projects = await _ticketService.GetProjectsAsync();

                if (model.CategoryId > 0)
                {
                    model.SubCategories = await _ticketService.GetSubCategoriesAsync(model.CategoryId);
                }

                var status = await _ticketService.GetOpenStatusAsync();
                if (status != null)
                {
                    model.StatusId = status.Id;
                    model.Status = status.Name;
                }

                return View(model);
            }

            var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            await _ticketService.CreateAsync(model);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<JsonResult> GetSubCategories(int categoryId)
        {
            if (categoryId <= 0)
            {
                return Json(new List<object>());
            } 

            var subCategories = await _ticketService.GetSubCategoriesAsync(categoryId);
            return Json(subCategories);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var model = await _ticketService.GetByIdAsync(id);          

            if (model == null)
            {
                throw new InvalidOperationException("Destination not found");
            }

            return View(model);
        }
    }
}
