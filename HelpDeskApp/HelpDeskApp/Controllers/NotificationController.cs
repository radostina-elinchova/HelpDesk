using HelpDeskApp.Core.Contracts;
using HelpDeskApp.ViewModels.Models.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? userId = GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            IEnumerable<NotificationListVM> model =  await _notificationService.GetUserNotificationsAsync(userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            string? userId = GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool marked = await _notificationService.MarkNotificationAsReadAsync(id, userId);

            if (!marked)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
