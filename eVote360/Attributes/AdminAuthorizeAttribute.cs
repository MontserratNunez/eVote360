using eVote360.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eVote360.Attributes
{
    public class AdminAuthorizeAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userSession = context.HttpContext.RequestServices
                .GetService<IUserSession>();

            if (userSession == null || !userSession.HasUser())
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "AccessDenied" }
                    });
                return;
            }

            if (!userSession.IsAdmin())
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "AccessDenied" }
                    });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

}
