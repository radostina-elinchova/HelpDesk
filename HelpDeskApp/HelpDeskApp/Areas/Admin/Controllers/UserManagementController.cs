using HelpDeskApp.Core.Contracts;
using HelpDeskApp.ViewModels.Models.Common;
using HelpDeskApp.ViewModels.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskApp.Areas.Admin.Controllers
{
    public class UserManagementController : BaseAdminController
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] UserQueryVM queryModel)
        {
            UserQueryVM model = await _userService.GetAllUsersAsync(queryModel);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            string currentAdminId = GetAdminUserId()!;

            UserDetailsVM? model = await _userService.GetUserDetailsAsync(id, currentAdminId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            string currentAdminId = GetAdminUserId()!;

            UserDeleteVM? model = await _userService.GetUserDeleteAsync(id, currentAdminId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            string currentAdminId = GetAdminUserId()!;

            string? errorMessage = await _userService.DeleteUserAsync(id, currentAdminId);

            if (errorMessage != null)
            {
                TempData["ErrorMessage"] = errorMessage;

                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
