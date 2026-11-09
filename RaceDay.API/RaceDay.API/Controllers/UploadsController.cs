using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [SessionAuthorize]
    public class UploadsController : ControllerBase
    {
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
        private readonly RaceDayDbContext _context;
        private readonly IBlobService _blobs;

        public UploadsController(RaceDayDbContext context, IBlobService blobs)
        {
            _context = context;
            _blobs = blobs;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId")!.Value;

        [HttpPost("/api/events/{id}/banner")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != CurrentUserId)
                return StatusCode(403, "You can only change the banner of your own events.");

            var (url, error) = await StoreImage(file, "banners");
            if (error != null) return error;

            ev.BannerImageUrl = url;
            await _context.SaveChangesAsync();
            return Ok(new { bannerImageUrl = url });
        }

        [HttpPost("/api/users/profile/picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound("User not found.");

            var (url, error) = await StoreImage(file, "profiles");
            if (error != null) return error;

            user.ProfilePictureUrl = url;
            await _context.SaveChangesAsync();
            return Ok(new { profilePictureUrl = url });
        }

        private async Task<(string? Url, IActionResult? Error)> StoreImage(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return (null, BadRequest("Please choose an image file."));
            if (file.Length > MaxBytes)
                return (null, BadRequest("Images must be 5 MB or smaller."));

            await using var stream = file.OpenReadStream();
            var type = DetectImageType(stream);
            if (type == null)
                return (null, BadRequest("Only JPG, PNG or WebP images are allowed."));

            stream.Position = 0;
            try
            {
                return (await _blobs.UploadImageAsync(stream, type, folder), null);
            }
            catch (Exception)
            {
                return (null, StatusCode(503, "File storage is not available right now. Please try again shortly."));
            }
        }

        // Checks the file's real first bytes, not just the name or the header the browser claims
        private static string? DetectImageType(Stream s)
        {
            var h = new byte[12];
            var read = s.Read(h, 0, h.Length);
            if (read >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return "image/jpeg";
            if (read >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return "image/png";
            if (read >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46
                && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return "image/webp";
            return null;
        }
    }
}