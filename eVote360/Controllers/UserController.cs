using eVote360.Core.Application.Dtos.User;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.User;
using eVote360.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserSession _userSession;

        public UserController(IUserService userService, IUserSession userSession)
        {
            _userService = userService;
            _userSession = userSession;
        }

        public async Task<IActionResult> Index()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            var dtos = await _userService.GetAllWithInclude();

            var listEntityVms = dtos.Select(s =>
              new UserViewModel()
              {
                  Id = s.Id,
                  Name = s.Name,
                  Email = s.Email,
                  UserName = s.UserName,
                  LastName = s.LastName,
                  Role = s.Role,
                  Status = s.Status
              }).ToList();

            return View(listEntityVms);
        }

        public IActionResult Create()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            return View(new CreateUserViewModel() { Id = 0, Name = "", Email = "", UserName = "", LastName = "", Password = "", ConfirmPassword = "", Role = 0, Status = true});
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

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
                Role = vm.Role,
                Status = vm.Status
            };

            UserDto? returnUser = await _userService.AddAsync(dto);
            if (returnUser != null && returnUser.Id != 0)
            {
                dto.Id = returnUser.Id;
                await _userService.UpdateAsync(dto);
            }

            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            if (!ModelState.IsValid)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            ViewBag.EditMode = true;
            var dto = await _userService.GetById(id);

            if (dto == null)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            UpdateUserViewModel vm = new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.UserName,
                LastName = dto.LastName,
                Password = "",
                Role = dto.Role,
                Status = dto.Status
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserViewModel vm)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = true;
                return View(vm);
            }

            SaveUserDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName,
                LastName = vm.LastName,
                Password = vm.Password ?? "",
                Role = vm.Role,
                Status = vm.Status              
            };

            await _userService.UpdateAsync(dto);
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            if (!ModelState.IsValid)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            var dto = await _userService.GetById(id);
            if (dto == null)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            DeleteUserViewModel vm = new() { Id = dto.Id, Name = dto.Name, LastName = dto.LastName };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteUserViewModel vm)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _userService.DeleteAsync(vm.Id);
            FileManager.Delete(vm.Id, "Users");
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }
    }
}
