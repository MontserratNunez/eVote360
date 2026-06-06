using eVote360.Core.Application.Dtos.User;
using eVote360.Core.Application.Helpers;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.User;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Helpers;
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
            if (_userSession.HasUser())
            {
                UserViewModel? userSession = _userSession.GetUserSession();
                if (userSession != null)
                {
                    return userSession.Role switch
                    {
                        (int)Role.ADMIN => RedirectToRoute(new { controller = "Home", action = "Admin" }),
                        (int)Role.DIRECTOR => RedirectToRoute(new { controller = "InvestorHome", action = "Director" }),
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
                UserViewModel? userSession = _userSession.GetUserSession();
                if (userSession != null)
                {
                    return userSession.Role switch
                    {
                        (int)Role.ADMIN => RedirectToRoute(new { controller = "Home", action = "Index" }),
                        (int)Role.DIRECTOR => RedirectToRoute(new { controller = "InvestorHome", action = "Index" }),
                        _ => RedirectToRoute(new { controller = "Login", action = "Index" }),
                    };
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Password = "";
                return View(vm);
            }

            UserDto? userDto = await _userService.LoginAsync(new LoginDto()
            {
                Password = vm.Password,
                UserName = vm.UserName
            });

            if (userDto != null)
            {
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

                if (userVm.Role == (int)Role.ADMIN)
                {
                    return RedirectToRoute(new { controller = "HomeAdmin", action = "Index" });
                }

                return RedirectToRoute(new { controller = "HomeDirector", action = "Index" });

            }
            else
            {
                ModelState.AddModelError("userValidation", "Data access is incorrect");
            }

            vm.Password = "";
            return View(vm);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }
        public IActionResult Register()
        {
            return View(new RegisterUserViewModel()
            {
                ConfirmPassword = "",
                Email = "",
                LastName = "",
                Name = "",
                Password = "",
                UserName = "",
            });
        }

        /*
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            SaveUserDto dto = new()
            {
                Id = 0,
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName,
                LastName = vm.LastName,
                Password = vm.Password,
                Role = (int)Role.INVESTOR,
                Phone = vm.Phone
            };
            UserDto? returnUser = await _userService.AddAsync(dto);
            if (returnUser != null && returnUser.Id != 0)
            {
                dto.Id = returnUser.Id;
                dto.ProfileImage = FileManager.Upload(vm.ProfileImageFile, dto.Id, "Users");
                await _userService.UpdateAsync(dto);
            }

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }*/

        public IActionResult AccessDenied()
        {
            if (_userSession.HasUser())
            {
                return View();
            }

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }
    }
}
