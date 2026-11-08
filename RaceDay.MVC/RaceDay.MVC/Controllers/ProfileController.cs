using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [RequireRole] // any logged-in user
    public class ProfileController : Controller
    {
        private readonly ApiClient _api;

        public ProfileController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _api.GetAsync("api/users/profile");
            var profile = response.IsSuccess ? response.ReadAs<ProfileViewModel>() : null;
            if (profile == null)
            {
                TempData["Error"] = response.IsSuccess ? "Could not load your profile." : response.ErrorMessage;
                return RedirectToAction("Index", "Home");
            }
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string fullName)
        {
            fullName = (fullName ?? string.Empty).Trim();
            if (fullName.Length == 0 || fullName.Length > 100)
            {
                TempData["Error"] = "Your name must be between 1 and 100 characters.";
                return RedirectToAction(nameof(Index));
            }

            // Re-read the saved picture URL so a name change never wipes the picture
            var current = await _api.GetAsync("api/users/profile");
            var profile = current.IsSuccess ? current.ReadAs<ProfileViewModel>() : null;
            if (profile == null)
            {
                TempData["Error"] = current.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var response = await _api.PutAsync("api/users/profile", new { fullName, profilePictureUrl = profile.ProfilePictureUrl });
            if (response.IsSuccess)
            {
                HttpContext.Session.SetString("FullName", fullName);
                TempData["Success"] = "Profile updated.";
            }
            else
            {
                TempData["Error"] = response.ErrorMessage;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}