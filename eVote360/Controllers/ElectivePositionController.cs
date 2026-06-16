using eVote360.Attributes;
using eVote360.Core.Application.Dtos.ElectivePosition;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.ElectivePosition;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [AdminAuthorize]
    public class ElectivePositionController : Controller
    {
        private readonly IElectivePositionService _electivePositionService;

        public ElectivePositionController(IElectivePositionService electivePositionService)
        {
            _electivePositionService = electivePositionService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _electivePositionService.GetAllAsync();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new List<ElectivePositionViewModel>());
            }

            var vm = result.Data.Select(e => new ElectivePositionViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status = e.Status
            }).ToList();

            ViewBag.HasActiveElection = await _electivePositionService.HasActiveElection();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await _electivePositionService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden crear puestos electivos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreatePositionViewModel
            {
                Name = "",
                Description = "",
                Status = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePositionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            CreateElectivePositionDto dto = new()
            {
                Name = vm.Name,
                Description = vm.Description,
                Status = true
            };

            var result = await _electivePositionService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (await _electivePositionService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden editar puestos electivos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _electivePositionService.GetById(id);

           
            if (result.Data == null)
                return RedirectToAction(nameof(Index));

            EditPositionViewModel vm = new()
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                Status = result.Data.Status
            };

            ViewBag.HasBeenUsed = await _electivePositionService.HasPositionBeenUsed(id);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditPositionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HasBeenUsed = await _electivePositionService.HasPositionBeenUsed(vm.Id);
                return View(vm);
            }

            UpdateElectivePositionDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                Status = vm.Status
            };

            var result = await _electivePositionService.UpdateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                ViewBag.HasBeenUsed = await _electivePositionService.HasPositionBeenUsed(vm.Id);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Confirm(int id, bool activate)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "ElectivePosition";

            if (activate)
            {
                ViewBag.Message = "¿Está seguro que desea activar este puesto electivo?";
                ViewBag.ActionName = "ActivateConfirmed";
                ViewBag.ButtonClass = "btn-success";
                ViewBag.ButtonText = "Activar";
            }
            else
            {
                ViewBag.Message = "¿Está seguro que desea desactivar este puesto electivo?";
                ViewBag.ActionName = "DeactivateConfirmed";
                ViewBag.ButtonClass = "btn-danger";
                ViewBag.ButtonText = "Desactivar";
            }

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var result = await _electivePositionService.ActivateAsync(id);

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
            var result = await _electivePositionService.DeactivateAsync(id);

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
