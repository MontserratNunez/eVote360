using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class HomeAdminController : Controller
    {
        public HomeAdminController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
