using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return BadRequest("An account with this email already exists.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role);
            if (role == null)
                return BadRequest("Role must be 'Organiser' or 'Participant'.");

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                RoleId = role.RoleId,
                CreatedAt = DateTime.Now
            };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { user.UserId, user.FullName, user.Email, Role = role.RoleName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Same message for "no such user" and "wrong password" so attackers can't tell which was wrong
            if (user == null)
                return Unauthorized("Invalid email or password.");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password.");

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            return Ok(new { user.UserId, user.FullName, Role = user.Role.RoleName });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Unauthorized("You are not logged in.");

            HttpContext.Session.Clear();
            return Ok("Logged out.");
        }
    }
}