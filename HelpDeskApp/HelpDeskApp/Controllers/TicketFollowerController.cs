using HelpDeskApp.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Controllers
{
    [Authorize]
    public class TicketFollowerController : BaseController
    {
        private readonly ITicketFollowerService _ticketFollowerService;

        public TicketFollowerController(
            ITicketFollowerService ticketFollowerService)
        {
            _ticketFollowerService = ticketFollowerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? userId = GetUserId();

            var model = await _ticketFollowerService.GetFollowedTicketsAsync(userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(int ticketId)
        {
            if (ticketId <= 0)
            {
                return BadRequest();
            }

            string? userId = GetUserId();
            bool isAdmin = User.IsInRole("Administrator");

            bool result = await _ticketFollowerService.FollowAsync(ticketId, userId, isAdmin);

            if (!result)
            {
                return Forbid();
            }

            return RedirectToAction("Index", "Ticket");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(int ticketId)
        {
            if (ticketId <= 0)
            {
                return BadRequest();
            }

            string? userId = GetUserId();

            bool result = await _ticketFollowerService.UnfollowAsync(ticketId, userId);

            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
