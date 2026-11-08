using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [RequireRole("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {
            _api = api;
        }

        // Home page: upcoming events with filtering
        public async Task<IActionResult> Index(EventFilter filter)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.Type)) parts.Add($"type={Uri.EscapeDataString(filter.Type)}");
            if (!string.IsNullOrWhiteSpace(filter.Search)) parts.Add($"search={Uri.EscapeDataString(filter.Search)}");
            if (filter.FromDate != null) parts.Add("fromDate=" + filter.FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (filter.ToDate != null) parts.Add("toDate=" + filter.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            var path = "api/events" + (parts.Count > 0 ? "?" + string.Join("&", parts) : "");
            var response = await _api.GetAsync(path);
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;

            return View(new EventsPageViewModel
            {
                Events = (response.IsSuccess ? response.ReadAs<List<EventSummary>>() : null) ?? new(),
                Filter = filter
            });
        }

        // Event detail page: full information, categories, and the enrol form
        public async Task<IActionResult> Details(int id)
        {
            var evResponse = await _api.GetAsync($"api/events/{id}");
            var ev = evResponse.IsSuccess ? evResponse.ReadAs<EventSummary>() : null;
            if (ev == null)
            {
                TempData["Error"] = evResponse.IsSuccess ? "Event not found." : evResponse.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var catResponse = await _api.GetAsync($"api/events/{id}/categories");
            var categories = (catResponse.IsSuccess ? catResponse.ReadAs<List<CategoryItem>>() : null) ?? new();

            var mineResponse = await _api.GetAsync("api/enrolments/my");
            var mine = (mineResponse.IsSuccess ? mineResponse.ReadAs<List<MyEnrolmentItem>>() : null) ?? new();

            return View(new EventDetailsViewModel
            {
                Event = ev,
                Categories = categories,
                MyEnrolment = mine.FirstOrDefault(e => e.EventId == id)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (categoryId <= 0)
            {
                TempData["Error"] = "Please choose a category before enrolling.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var response = await _api.PostAsync("api/enrolments", new { eventId, categoryId });
            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            TempData["Success"] = "You are enrolled! Your place is confirmed.";
            return RedirectToAction(nameof(MyEnrolments));
        }

        public async Task<IActionResult> MyEnrolments()
        {
            var response = await _api.GetAsync("api/enrolments/my");
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            var items = (response.IsSuccess ? response.ReadAs<List<MyEnrolmentItem>>() : null) ?? new();
            return View(items.OrderBy(e => e.EventDate).ToList());
        }

        public async Task<IActionResult> MyResults()
        {
            var response = await _api.GetAsync("api/results/my");
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            var items = (response.IsSuccess ? response.ReadAs<List<MyResultItem>>() : null) ?? new();
            return View(items.OrderByDescending(r => r.EventDate).ToList());
        }
    }
}