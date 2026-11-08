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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public ResultsController(RaceDayDbContext context) { _context = context; }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId")!.Value;

        [HttpPost("/api/results")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> AddResult(ResultRequest request)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);
            if (enrolment == null) return NotFound("Enrolment not found.");
            if (enrolment.Event.OrganiserId != CurrentUserId)
                return StatusCode(403, "You can only record results for your own events.");
            if (await _context.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
                return Conflict("A result has already been recorded for this enrolment.");

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition,
                RecordedAt = DateTime.Now
            };
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { result.ResultId, result.FinishPosition });
        }

        [HttpGet("/api/results/my")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var items = await _context.Results
                .Where(r => r.Enrolment.ParticipantId == CurrentUserId)
                .Select(r => new
                {
                    EventName = r.Enrolment.Event.Name,
                    r.Enrolment.Event.EventDate,
                    CategoryName = r.Enrolment.Category.CategoryName,
                    r.FinishTime,
                    r.FinishPosition,
                    TotalFinishers = _context.Results.Count(x => x.Enrolment.EventId == r.Enrolment.EventId)
                })
                .ToListAsync();
            return Ok(items);
        }
    }
}
