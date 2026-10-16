
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Authorization
{
    public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute()
        {
        }

        public SessionAuthorizeAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public Task OnAuthorizationAsync(
            AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var role = session.GetString("Role");

            // No active login session
            if (userId == null || string.IsNullOrEmpty(role))
            {
                context.Result = new UnauthorizedObjectResult(
                    "Please log in first.");

                return Task.CompletedTask;
            }

            // Logged in, but the role is incorrect
            if (_requiredRole != null &&
                !string.Equals(
                    role,
                    _requiredRole,
                    StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(
                    "You do not have permission to access this endpoint.")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
