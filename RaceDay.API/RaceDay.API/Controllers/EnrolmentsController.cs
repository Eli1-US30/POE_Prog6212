using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [SessionAuthorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EnrolmentsController(RaceDayDbContext context) { _context = context; }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId")!.Value;

        [HttpPost("/api/enrolments")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(EnrolRequest request)
        {
            var ev = await _context.Events.FindAsync(request.EventId);
            if (ev == null) return NotFound("Event not found.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.EventId == request.EventId);
            if (category == null) return BadRequest("That category does not belong to this event.");

            if (await _context.Enrolments.AnyAsync(e => e.ParticipantId == CurrentUserId && e.EventId == request.EventId))
                return Conflict("You are already enrolled in this event.");

            var enrolment = new Enrolment
            {
                ParticipantId = CurrentUserId,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { enrolment.EnrolmentId, enrolment.Status });
        }

        [HttpGet("/api/enrolments/my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var items = await _context.Enrolments
                .Where(e => e.ParticipantId == CurrentUserId)
               .Select(e => new
               {
                   e.EnrolmentId,
                   e.EventId,
                   EventName = e.Event.Name,
                   e.Event.EventDate,
                   e.Event.Location,
                   CategoryName = e.Category.CategoryName,
                   e.Status,
                   e.EnrolmentDate
               })
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("/api/events/{id}/enrolments")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != CurrentUserId)
                return StatusCode(403, "You can only view enrolments for your own events.");

            var items = await _context.Enrolments
                .Where(e => e.EventId == id)
               .Select(e => new
               {
                   e.EnrolmentId,
                   ParticipantName = e.Participant.FullName,
                   ParticipantEmail = e.Participant.Email,
                   CategoryName = e.Category.CategoryName,
                   e.Status,
                   e.EnrolmentDate,
                   FinishTime = e.Result != null ? (TimeSpan?)e.Result.FinishTime : null,
                   FinishPosition = e.Result != null ? (int?)e.Result.FinishPosition : null
               })
                .ToListAsync();
            return Ok(items);
        }
    }
}
