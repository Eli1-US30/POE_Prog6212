using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/users/profile")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public ProfileController(RaceDayDbContext context) { _context = context; }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId")!.Value;

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _context.Users
                .Where(u => u.UserId == CurrentUserId)
                .Select(u => new { u.UserId, u.FullName, u.Email, u.ProfilePictureUrl, Role = u.Role.RoleName })
                .FirstOrDefaultAsync();
            if (profile == null) return NotFound("User not found.");
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound("User not found.");

            user.FullName = request.FullName.Trim();
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            await _context.SaveChangesAsync();
            return Ok(new { user.UserId, user.FullName, user.ProfilePictureUrl });
        }
    }
}