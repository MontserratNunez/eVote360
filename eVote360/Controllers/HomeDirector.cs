using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class HomeDirectorController : Controller
    {

        public HomeDirectorController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
