using eVote360.Attributes;
using eVote360.Core.Application.Dtos.User;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.User;
using eVote360.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [AdminAuthorize]
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
            var dtos = await _userService.GetAll();

            var listEntityVms = (dtos.Data ?? new List<UserDto>()).Select(s =>
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

            return View(new CreateUserViewModel() { Id = 0, Name = "", Email = "", UserName = "", LastName = "", Password = "", ConfirmPassword = "", Role = 0, Status = true});
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            CreateUserDto dto = new()
            {
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName,
                LastName = vm.LastName,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword,
                Role = vm.Role,
                Status = vm.Status
            };

            var result = await _userService.AddAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            TempData["Success"] = "Usuario creado exitosamente.";
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _userService.GetById(id);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var dto = result.Data;

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
            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = true;
                return View(vm);
            }

            var userSession = _userSession.GetUserSession();

            if (userSession == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            SaveUserDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName,
                LastName = vm.LastName,
                Password = vm.Password ?? "",
                ConfirmPassword = vm.ConfirmPassword ?? "",
                Role = vm.Role,
                Status = vm.Status              
            };

            var result = await _userService.UpdateAsync(dto, userSession.Id);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            TempData["Success"] = "Usuario actualizado exitosamente.";
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        [HttpGet]
        public IActionResult Activate(int id)
        {
            ViewBag.Id = id;
            ViewBag.Message = "¿Está seguro que desea activar este usuario?";
            ViewBag.ActionName = "ActivateConfirmed";
            ViewBag.ControllerName = "User";
            ViewBag.ButtonClass = "btn-success";
            ViewBag.ButtonText = "Aceptar";


            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var result = await _userService.ActivateAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Deactivate(int id)
        {
            ViewBag.Id = id;
            ViewBag.Message = "¿Está seguro que desea desactivar este usuario?";
            ViewBag.ActionName = "DeactivateConfirmed";
            ViewBag.ControllerName = "User";
            ViewBag.ButtonClass = "btn btn-danger";
            ViewBag.ButtonText = "Desactivar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var userSession = _userSession.GetUserSession();

            if (userSession == null)
            {
                TempData["Error"] = "Sesión inválida.";
                return RedirectToAction("Index");
            }

            var result = await _userService.DeactivateAsync(id, userSession.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}
