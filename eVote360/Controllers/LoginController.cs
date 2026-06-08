using eVote360.Core.Application.Dtos.User;
using eVote360.Core.Application.Helpers;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.User;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Helpers;
using eVote360.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserSession _userSession;

        public LoginController(IUserService userService, IUserSession userSession)
        {
            _userService = userService;
            _userSession = userSession;
        }
        public IActionResult Index()
        {
            Console.WriteLine(PasswordEncryptation.ComputeSha256Hash("123"));
            if (_userSession.HasUser())
            {
                UserViewModel? userSession = _userSession.GetUserSession();
                if (userSession != null)
                {
                    return userSession.Role switch
                    {
                        Role.ADMIN => RedirectToRoute(new { controller = "HomeAdmin", action = "Index" }),
                        Role.DIRECTOR => RedirectToRoute(new { controller = "HomeDirector", action = "Index" }),
                        _ => RedirectToRoute(new { controller = "Login", action = "Index" }),
                    };
                }
            }

            return View(new LoginViewModel() { Password = "", UserName = "" });
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            if (_userSession.HasUser())
            {
                var userSession = _userSession.GetUserSession();

                if (userSession != null)
                {
                    return userSession.Role switch
                    {
                        Role.ADMIN => RedirectToRoute(new { controller = "HomeAdmin", action = "Index" }),
                        Role.DIRECTOR => RedirectToRoute(new { controller = "HomeDirector", action = "Index" }),
                        _ => RedirectToRoute(new { controller = "Login", action = "Index" }),
                    };
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Password = "";
                return View(vm);
            }

            var result = await _userService.LoginAsync(new LoginDto()
            {
                Password = vm.Password,
                UserName = vm.UserName
            });

            if (!result.IsSuccess || result.Data == null)
            {
                ModelState.AddModelError("userValidation", result.Message);
                vm.Password = "";
                return View(vm);
            }

            var userDto = result.Data;

            UserViewModel userVm = new()
            {
                Email = userDto.Email,
                Id = userDto.Id,
                LastName = userDto.LastName,
                Name = userDto.Name,
                Role = userDto.Role,
                UserName = userDto.UserName,
                Status = userDto.Status
            };

            HttpContext.Session.Set<UserViewModel>("User", userVm);

            return userVm.Role switch
            {
                Role.ADMIN => RedirectToRoute(new { controller = "HomeAdmin", action = "Index" }),
                Role.DIRECTOR => RedirectToRoute(new { controller = "HomeDirector", action = "Index" }),
                _ => RedirectToRoute(new { controller = "Login", action = "Index" })
            };
        }



        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }
        
        public IActionResult AccessDenied()
        {
            if (_userSession.HasUser())
            {
                var userSession = _userSession.GetUserSession();

                if (userSession.Role == Role.ADMIN)
                {
                    ViewBag.Controller = "HomeAdmin";
                }

                if (userSession.Role == Role.DIRECTOR)
                {
                    ViewBag.Controller = "HomeDirector";
                }

                return View();
            }

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }
    }
}
