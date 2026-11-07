using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.API.Security;

namespace RaceDay.Tests
{
    public class RoleAccessTests
    {
        private static AuthorizationFilterContext BuildContext(int? userId, string? role)
        {
            var session = new TestSession();
            if (userId != null) session.SetInt32("UserId", userId.Value);
            if (role != null) session.SetString("Role", role);
            var httpContext = new DefaultHttpContext { Session = session };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void NotLoggedIn_IsRejectedWith401()
        {
            var context = BuildContext(null, null);
            new SessionAuthorizeAttribute("Organiser").OnAuthorization(context);
            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void Participant_OnOrganiserOnlyEndpoint_IsRejectedWith403()
        {
            var context = BuildContext(1, "Participant");
            new SessionAuthorizeAttribute("Organiser").OnAuthorization(context);
            Assert.Equal(403, Assert.IsType<ObjectResult>(context.Result).StatusCode);
        }

        [Fact]
        public void Organiser_OnOrganiserOnlyEndpoint_IsAllowed()
        {
            var context = BuildContext(1, "Organiser");
            new SessionAuthorizeAttribute("Organiser").OnAuthorization(context);
            Assert.Null(context.Result);
        }
    }
}