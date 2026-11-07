using System.Text.Json;

namespace RaceDay.MVC.Services
{
    public class ApiResponse
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public int StatusCode { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
        public string Body { get; set; } = string.Empty;

        // Turns whatever the API sent back into a friendly message for the user
        public string ErrorMessage
        {
            get
            {
                if (IsSuccess) return string.Empty;

                var text = Body.Trim().Trim('"');

                // Plain-text messages from the API (like "Invalid email or password.") are already user-friendly
                if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith("{"))
                    return text;

                // Validation errors arrive as JSON: { "errors": { "Field": ["message"] } }
                try
                {
                    using var doc = JsonDocument.Parse(Body);
                    if (doc.RootElement.TryGetProperty("errors", out var errors))
                    {
                        var messages = errors.EnumerateObject()
                            .SelectMany(p => p.Value.EnumerateArray().Select(m => m.GetString()));
                        return string.Join(" ", messages);
                    }
                }
                catch (JsonException) { }

                return StatusCode switch
                {
                    401 => "Please log in to continue.",
                    403 => "You do not have permission to do that.",
                    404 => "That item could not be found.",
                    _ => "Something went wrong. Please try again."
                };
            }
        }

        public T? ReadAs<T>()
        {
            if (string.IsNullOrWhiteSpace(Body)) return default;
            return JsonSerializer.Deserialize<T>(Body, JsonOptions);
        }
    }
}