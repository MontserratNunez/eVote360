using eVote360.Attributes;
using eVote360.Core.Application.Interfaces;
using eVote360.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [DirectorAuthorize]
    public class HomeDirectorController : Controller
    {
        private readonly IUserSession _userSession;

        public HomeDirectorController(IUserSession userSession)
        {
            _userSession = userSession;
        }

        public IActionResult Index()
        {
            var userSession = _userSession.GetUserSession();

            if (userSession == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            ViewBag.Name = userSession.Name;
            return View();
        }
    }
}
