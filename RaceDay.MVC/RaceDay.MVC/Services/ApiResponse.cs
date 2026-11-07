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
                if (StatusCode == 503) return Body;
                if (StatusCode == 401) return "Please log in to continue.";
                if (StatusCode == 403) return "You do not have permission to do that.";

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
                catch (JsonException) { } // plain-text error, handled below

                return string.IsNullOrWhiteSpace(Body)
                    ? "Something went wrong. Please try again."
                    : Body.Trim('"');
            }
        }

        public T? ReadAs<T>()
        {
            if (string.IsNullOrWhiteSpace(Body)) return default;
            return JsonSerializer.Deserialize<T>(Body, JsonOptions);
        }
    }
}