using eVote360.Attributes;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.HomeAdmin;
using eVote360.Middlewares;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace eVote360.Controllers
{
    [AdminAuthorize]
    public class HomeAdminController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IHomeAdminService _dashboardService;
        public HomeAdminController(IUserSession userSession, IHomeAdminService dashboardService)
        {
            _userSession = userSession;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var session = _userSession.GetUserSession();

            if (session == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            ViewBag.Name = session.Name;
            var viewModel = new AdminHomeViewModel();

            var result = await _dashboardService.GetDashboardInitialDataAsync();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(viewModel);
            }

            var data = result.Data;
            viewModel.ActiveElection = data.ActiveElection;

            viewModel.YearsList = data.AvailableYears
                .Select(y => new DropdownDto
                {
                    Id = y,
                    Name = y.ToString()
                })
                .ToList();

            viewModel.SelectedYear = data.DefaultYear;

            var summaryResult = await _dashboardService.GetElectionSummaryByYearAsync(data.DefaultYear);
            if (summaryResult.IsSuccess)
            {
                viewModel.YearlyElections = summaryResult.Data;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AdminHomeViewModel model)
        {
            var session = _userSession.GetUserSession();

            if (session == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            ViewBag.Name = session.Name;
            var result = await _dashboardService.GetDashboardInitialDataAsync();

            if (result.IsSuccess)
            {
                model.ActiveElection = result.Data.ActiveElection;
                model.YearsList = result.Data.AvailableYears
                    .Select(y => new DropdownDto
                    {
                        Id = y,
                        Name = y.ToString()
                    })
                    .ToList();
            }

            var summaryResult = await _dashboardService.GetElectionSummaryByYearAsync(model.SelectedYear.Value);

            if (!summaryResult.IsSuccess)
            {
                TempData["Error"] = summaryResult.Message;
                return View(model);
            }

            model.YearlyElections = summaryResult.Data;
            return View(model);
        }
    }
}
