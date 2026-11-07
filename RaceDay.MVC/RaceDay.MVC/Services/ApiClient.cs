using System.Net.Http.Json;

namespace RaceDay.MVC.Services
{
    public class ApiClient
    {
        private const string CookieKey = "ApiCookie";
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _accessor = accessor;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public Task<ApiResponse> GetAsync(string path) => SendAsync(HttpMethod.Get, path);
        public Task<ApiResponse> PostAsync(string path, object? body = null) => SendAsync(HttpMethod.Post, path, body);
        public Task<ApiResponse> PutAsync(string path, object? body = null) => SendAsync(HttpMethod.Put, path, body);
        public Task<ApiResponse> DeleteAsync(string path) => SendAsync(HttpMethod.Delete, path);

        private async Task<ApiResponse> SendAsync(HttpMethod method, string path, object? body = null)
        {
            var request = new HttpRequestMessage(method, path);
            if (body != null) request.Content = JsonContent.Create(body);
            return await ExecuteAsync(request);
        }

        // Used later for file uploads (banner and profile pictures)
        public async Task<ApiResponse> SendMultipartAsync(HttpMethod method, string path, MultipartFormDataContent content)
        {
            var request = new HttpRequestMessage(method, path) { Content = content };
            return await ExecuteAsync(request);
        }

        private async Task<ApiResponse> ExecuteAsync(HttpRequestMessage request)
        {
            // Show the API its session cookie (the wristband) if we have one
            var cookie = Session.GetString(CookieKey);
            if (!string.IsNullOrEmpty(cookie)) request.Headers.Add("Cookie", cookie);

            try
            {
                using var response = await _http.SendAsync(request);
                StoreCookie(response);
                return new ApiResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Body = await response.Content.ReadAsStringAsync()
                };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse
                {
                    StatusCode = 503,
                    Body = "The RaceDay service is not available right now. Please try again shortly."
                };
            }
        }

        // If the API issued a session cookie, keep it in the MVC user's own session
        private void StoreCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var values)) return;
            var sessionCookie = values.FirstOrDefault(v => v.StartsWith(".AspNetCore.Session="));
            if (sessionCookie != null) Session.SetString(CookieKey, sessionCookie.Split(';')[0]);
        }
    }
}