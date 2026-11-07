using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.PostAsync("api/auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.ErrorMessage);
                return View(model);
            }

            TempData["Success"] = "Account created. You can now log in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.PostAsync("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            var user = response.IsSuccess ? response.ReadAs<LoginResult>() : null;
            if (user == null)
            {
                ModelState.AddModelError(string.Empty,
                    response.IsSuccess ? "Login failed. Please try again." : response.ErrorMessage);
                return View(model);
            }

            // The MVC session remembers who is logged in and their role
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);

            return user.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _api.PostAsync("api/auth/logout"); // ends the API's session too
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();
    }
}