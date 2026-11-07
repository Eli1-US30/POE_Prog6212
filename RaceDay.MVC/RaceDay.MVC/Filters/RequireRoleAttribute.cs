using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.MVC.Filters
{
    // Not logged in = redirected to login. Wrong role = redirected to "access denied".
    public class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (role == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(role))
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}