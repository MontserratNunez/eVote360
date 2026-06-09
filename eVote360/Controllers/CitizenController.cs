using eVote360.Attributes;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Citizen;
using eVote360.Core.Application.Dtos.Citizen;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [AdminAuthorize]
    public class CitizenController : Controller
    {
        private readonly ICitizenService _citizenService;
        private readonly IUserSession _userSession;

        public CitizenController(ICitizenService citizenService, IUserSession userSession)
        {
            _citizenService = citizenService;
            _userSession = userSession;
        }


        public async Task<IActionResult> Index()
        {
            var result = await _citizenService.GetAllAsync();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new List<CitizenViewModel>());
            }

            var vm = result.Data.Select(c => new CitizenViewModel
            {
                Id = c.Id,
                Name = c.Name,
                LastName = c.LastName,
                Email = c.Email,
                DocumentNumber = c.DocumentNumber,
                Status = c.Status
            }).ToList();

            ViewBag.HasActiveElection = await _citizenService.HasActiveElection();

            return View(vm);
        }


        public async Task<IActionResult> Create()
        {
            if (await _citizenService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden crear ciudadanos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }
            return View("Save", new SaveCitizenViewModel() { Name = "", LastName = "", Email = "", DocumentNumber = "", Status = true });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveCitizenViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Save", vm);
            }

            SaveCitizenDto dto = new()
            {
                Name = vm.Name,
                LastName = vm.LastName,
                Email = vm.Email,
                DocumentNumber = vm.DocumentNumber,
                Status = true
            };

            var result = await _citizenService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View("Save", vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            if (await _citizenService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden editar ciudadanos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _citizenService.GetById(id);

            if (result.Data == null)
            {
                return RedirectToAction(nameof(Index));
            }

            SaveCitizenDto vm = new()
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                LastName = result.Data.LastName,
                Email = result.Data.Email,
                DocumentNumber = result.Data.DocumentNumber,
                Status = result.Data.Status
            };

            ViewBag.HasParticipated = await _citizenService.HasCitizenParticipated(id);

            return View("Save", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveCitizenViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HasParticipated = await _citizenService.HasCitizenParticipated(vm.Id);
                return View("Save", vm);
            }

            SaveCitizenDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                LastName = vm.LastName,
                Email = vm.Email,
                DocumentNumber = vm.DocumentNumber,
                Status = vm.Status
            };

            var result = await _citizenService.UpdateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                ViewBag.HasParticipated = await _citizenService.HasCitizenParticipated(vm.Id);

                return View("Save", vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Confirm(int id, bool activate)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "Citizen";

            if (activate)
            {
                ViewBag.Message = "¿Está seguro que desea activar este ciudadano?";
                ViewBag.ActionName = "ActivateConfirmed";
                ViewBag.ButtonClass = "btn-success";
                ViewBag.ButtonText = "Activar";
            }
            else
            {
                ViewBag.Message = "¿Está seguro que desea desactivar este ciudadano?";
                ViewBag.ActionName = "DeactivateConfirmed";
                ViewBag.ButtonClass = "btn-danger";
                ViewBag.ButtonText = "Desactivar";
            }

            return View("Confirm");
        }


        [HttpPost]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var result = await _citizenService.ActivateAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var result = await _citizenService.DeactivateAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}