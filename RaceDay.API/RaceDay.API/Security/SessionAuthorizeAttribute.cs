using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Security
{
    // Checks the session: not logged in = 401, wrong role = 403
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            if (session.GetInt32("UserId") == null)
            {
                context.Result = new UnauthorizedObjectResult("You must be logged in.");
                return;
            }

            var role = session.GetString("Role");
            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new ObjectResult("You do not have permission to do this.") { StatusCode = 403 };
            }
        }
    }
}