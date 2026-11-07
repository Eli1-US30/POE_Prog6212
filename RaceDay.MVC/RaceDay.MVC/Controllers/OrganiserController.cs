using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Globalization;

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
        // Loads an event and confirms it belongs to the logged-in Organiser
        private async Task<EventSummary?> GetOwnedEvent(int id)
        {
            var response = await _api.GetAsync($"api/events/{id}");
            var ev = response.IsSuccess ? response.ReadAs<EventSummary>() : null;
            if (ev == null)
            {
                TempData["Error"] = response.IsSuccess ? "Event not found." : response.ErrorMessage;
                return null;
            }
            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You can only manage your own events.";
                return null;
            }
            return ev;
        }

        // ----- Categories -----
        private async Task<CategoriesPageViewModel> BuildCategoriesPage(EventSummary ev, CategoryFormViewModel form)
        {
            var response = await _api.GetAsync($"api/events/{ev.EventId}/categories");
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            return new CategoriesPageViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name,
                Categories = (response.IsSuccess ? response.ReadAs<List<CategoryItem>>() : null) ?? new(),
                Form = form
            };
        }

        [HttpGet]
        public async Task<IActionResult> Categories(int id)
        {
            var ev = await GetOwnedEvent(id);
            if (ev == null) return RedirectToAction(nameof(Events));
            return View(await BuildCategoriesPage(ev, new CategoryFormViewModel { EventId = id }));
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([Bind(Prefix = "Form")] CategoryFormViewModel form)
        {
            var ev = await GetOwnedEvent(form.EventId);
            if (ev == null) return RedirectToAction(nameof(Events));

            if (form.MinAge == null && form.MaxAge == null && form.DistanceKm == null)
                ModelState.AddModelError(string.Empty, "Enter an age range, a distance, or both.");
            if (form.MinAge != null && form.MaxAge != null && form.MinAge > form.MaxAge)
                ModelState.AddModelError(string.Empty, "Minimum age cannot be higher than maximum age.");

            if (ModelState.IsValid)
            {
                var response = await _api.PostAsync($"api/events/{form.EventId}/categories", new
                {
                    categoryName = form.CategoryName,
                    minAge = form.MinAge,
                    maxAge = form.MaxAge,
                    distanceKm = form.DistanceKm
                });
                if (response.IsSuccess)
                {
                    TempData["Success"] = "Category added.";
                    return RedirectToAction(nameof(Categories), new { id = form.EventId });
                }
                ModelState.AddModelError(string.Empty, response.ErrorMessage);
            }
            return View(nameof(Categories), await BuildCategoriesPage(ev, form));
        }

        // ----- Enrolments and results -----
        private async Task<EnrolmentsPageViewModel?> BuildEnrolmentsPage(int id)
        {
            var ev = await GetOwnedEvent(id);
            if (ev == null) return null;

            var response = await _api.GetAsync($"api/events/{id}/enrolments");
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            return new EnrolmentsPageViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name,
                Enrolments = (response.IsSuccess ? response.ReadAs<List<EnrolmentItem>>() : null) ?? new()
            };
        }

        public async Task<IActionResult> Enrolments(int id)
        {
            var page = await BuildEnrolmentsPage(id);
            if (page == null) return RedirectToAction(nameof(Events));
            return View(page);
        }

        public async Task<IActionResult> Results(int id)
        {
            var page = await BuildEnrolmentsPage(id);
            if (page == null) return RedirectToAction(nameof(Events));
            return View(page);
        }

        [HttpPost]
        public async Task<IActionResult> RecordResult(int eventId, int enrolmentId, string finishTime, int finishPosition)
        {
            if (!TimeSpan.TryParse(finishTime, CultureInfo.InvariantCulture, out var time)
                || time <= TimeSpan.Zero || time >= TimeSpan.FromHours(24))
            {
                TempData["Error"] = "Enter the finish time as hh:mm:ss, for example 00:52:30.";
                return RedirectToAction(nameof(Results), new { id = eventId });
            }
            if (finishPosition < 1)
            {
                TempData["Error"] = "The finishing position must be 1 or higher.";
                return RedirectToAction(nameof(Results), new { id = eventId });
            }

            var response = await _api.PostAsync("api/results", new { enrolmentId, finishTime = time, finishPosition });
            if (response.IsSuccess) TempData["Success"] = "Result saved.";
            else TempData["Error"] = response.ErrorMessage;

            return RedirectToAction(nameof(Results), new { id = eventId });
        }
    }
}