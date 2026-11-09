namespace RaceDay.MVC.Helpers
{
    public static class UploadRules
    {
        private static readonly string[] Allowed = { "image/jpeg", "image/png", "image/webp" };
        public const long MaxBytes = 5 * 1024 * 1024;

        // Returns an error message, or null if the file is fine or no file was chosen
        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            if (!Allowed.Contains(file.ContentType)) return "Images must be JPG, PNG or WebP.";
            if (file.Length > MaxBytes) return "Images must be 5 MB or smaller.";
            return null;
        }
    }
}