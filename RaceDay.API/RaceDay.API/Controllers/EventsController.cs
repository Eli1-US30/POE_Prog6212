using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    [SessionAuthorize] // any logged-in user; stricter rules are added per method
    public class EventsController : ControllerBase
    {
        private static readonly string[] ValidTypes = { "run", "walk", "cycle" };
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId")!.Value;

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Select(e => new { e.EventId, e.Name, e.Description, e.EventDate, e.Location, e.DistanceKm, e.EventType, e.BannerImageUrl, e.OrganiserId })
                .ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Where(e => e.EventId == id)
                .Select(e => new { e.EventId, e.Name, e.Description, e.EventDate, e.Location, e.DistanceKm, e.EventType, e.BannerImageUrl, e.OrganiserId })
                .FirstOrDefaultAsync();
            if (ev == null) return NotFound("Event not found.");
            return Ok(ev);
        }

        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateEvent(EventRequest request)
        {
            if (!ValidTypes.Contains(request.EventType.ToLower()))
                return BadRequest("EventType must be run, walk or cycle.");

            var ev = new Event
            {
                OrganiserId = CurrentUserId,
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType.ToLower(),
                BannerImageUrl = request.BannerImageUrl
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, new { ev.EventId, ev.Name });
        }

        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != CurrentUserId)
                return StatusCode(403, "You can only edit your own events.");
            if (!ValidTypes.Contains(request.EventType.ToLower()))
                return BadRequest("EventType must be run, walk or cycle.");

            ev.Name = request.Name;
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.Location = request.Location;
            ev.DistanceKm = request.DistanceKm;
            ev.EventType = request.EventType.ToLower();
            ev.BannerImageUrl = request.BannerImageUrl;
            await _context.SaveChangesAsync();

            return Ok(new { ev.EventId, ev.Name });
        }

        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != CurrentUserId)
                return StatusCode(403, "You can only delete your own events.");
            if (await _context.Enrolments.AnyAsync(e => e.EventId == id))
                return Conflict("This event already has enrolments and cannot be deleted.");

            _context.Categories.RemoveRange(_context.Categories.Where(c => c.EventId == id));
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok("Event deleted.");
        }

        [HttpGet("{id}/categories")]
        public async Task<IActionResult> GetCategories(int id)
        {
            if (!await _context.Events.AnyAsync(e => e.EventId == id))
                return NotFound("Event not found.");

            var categories = await _context.Categories
                .Where(c => c.EventId == id)
                .Select(c => new { c.CategoryId, c.CategoryName, c.MinAge, c.MaxAge, c.DistanceKm })
                .ToListAsync();
            return Ok(categories);
        }

        [HttpPost("{id}/categories")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> AddCategory(int id, CategoryRequest request)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != CurrentUserId)
                return StatusCode(403, "You can only add categories to your own events.");

            var category = new Category
            {
                EventId = id,
                CategoryName = request.CategoryName,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                DistanceKm = request.DistanceKm
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { category.CategoryId, category.CategoryName });
        }
    }
}