using HelpDeskApp.Core.Contracts;
using HelpDeskApp.ViewModels.Models.Ticket;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Areas.Admin.Controllers
{
    public class TicketManagementController : BaseAdminController
    {
        private readonly ITicketService _ticketService;

        public TicketManagementController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tickets = await _ticketService.GetAllTicketsAsync(null, true);
            var statuses = await _ticketService.GetStatusesAsync();

            var model = new TicketManagementVM
            {
                Tickets = tickets,
                Statuses = statuses
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int ticketId, int statusId)
        {
            try
            {
                await _ticketService.ChangeStatusAsync(ticketId, statusId);

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
