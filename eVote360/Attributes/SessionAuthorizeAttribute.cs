using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using eVote360.Core.Application.Interfaces;

namespace eVote360.Attributes
{
    public class SessionAuthorizeAttribute : Attribute, IActionFilter
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
                        { "action", "Index" }
                    });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}