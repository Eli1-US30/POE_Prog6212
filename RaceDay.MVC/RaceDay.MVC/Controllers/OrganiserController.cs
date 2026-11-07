using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [RequireRole("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {
            _api = api;
        }

        private async Task<List<EventSummary>> LoadMyEvents()
        {
            var response = await _api.GetAsync("api/events/mine");
            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage;
                return new List<EventSummary>();
            }
            return response.ReadAs<List<EventSummary>>() ?? new List<EventSummary>();
        }

        public async Task<IActionResult> Dashboard() => View(await LoadMyEvents());

        public async Task<IActionResult> Events() => View(await LoadMyEvents());

        [HttpGet]
        public IActionResult CreateEvent() => View(new EventFormViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.PostAsync("api/events", ToApiBody(model));
            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.ErrorMessage);
                return View(model);
            }

            TempData["Success"] = "Event created.";
            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var response = await _api.GetAsync($"api/events/{id}");
            var ev = response.IsSuccess ? response.ReadAs<EventSummary>() : null;
            if (ev == null)
            {
                TempData["Error"] = response.IsSuccess ? "Event not found." : response.ErrorMessage;
                return RedirectToAction(nameof(Events));
            }
            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return RedirectToAction("AccessDenied", "Account");

            return View(new EventFormViewModel
            {
                EventId = ev.EventId,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                DistanceKm = ev.DistanceKm,
                EventType = ev.EventType,
                BannerImageUrl = ev.BannerImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _api.PutAsync($"api/events/{model.EventId}", ToApiBody(model));
            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.ErrorMessage);
                return View(model);
            }

            TempData["Success"] = "Event updated.";
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var response = await _api.DeleteAsync($"api/events/{id}");
            if (response.IsSuccess) TempData["Success"] = "Event deleted.";
            else TempData["Error"] = response.ErrorMessage;
            return RedirectToAction(nameof(Events));
        }

        private static object ToApiBody(EventFormViewModel m) => new
        {
            name = m.Name,
            description = m.Description,
            eventDate = m.EventDate,
            location = m.Location,
            distanceKm = m.DistanceKm,
            eventType = m.EventType,
            bannerImageUrl = m.BannerImageUrl
        };
    }
}