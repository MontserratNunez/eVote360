using eVote360.Attributes;
using eVote360.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using eVote360.Core.Application.ViewModels.HomeDirector;

namespace eVote360.Controllers
{
    [DirectorAuthorize]
    public class HomeDirectorController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IHomeDirectorService _homeDirectorService;

        public HomeDirectorController(IUserSession userSession, IHomeDirectorService homeDirectorService)
        {
            _userSession = userSession;
            _homeDirectorService = homeDirectorService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DirectorHomeViewModel();

            var session = _userSession.GetUserSession();

            if (session == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            ViewBag.Name = session.Name;

            var result = await _homeDirectorService.GetDirectorDashboardDataAsync(session.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(viewModel);
            }

            viewModel.DashboardData = result.Data;
            return View(viewModel);
        }

    }
}


