using eVote360.Attributes;
using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.PoliticalParty;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.Services;
using eVote360.Core.Application.ViewModels.PoliticalParty;
using eVote360.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eVote360.Controllers
{
    [AdminAuthorize]
    public class PoliticalPartyController : Controller
    {
        private readonly IPoliticalPartyService _politicalPartyService;

        public PoliticalPartyController(IPoliticalPartyService politicalPartyService)
        {
            _politicalPartyService = politicalPartyService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _politicalPartyService.GetAllAsync();

            var vm = (data.Data ?? new List<PoliticalPartyDto>()).Select(p => 
                new PoliticalPartyViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Acronym = p.Acronym,
                    LogoPath = p.LogoPath,
                    Status = p.Status
                }).ToList();

            ViewBag.HasActiveElection = await _politicalPartyService.HasActiveElection();

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var party = new CreatePoliticalPartyViewModel()
            {
                Name = "",
                Description = "",
                Acronym = "",
                Status = true
            };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePoliticalPartyViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.Logo == null || !IsValidImage(vm.Logo))
            {
                ModelState.AddModelError("Logo", "El logo del partido debe ser una imagen válida.");
                return View(vm);
            }

            CreatePoliticalPartyDto dto = new()
            {
                Name = vm.Name,
                Description = vm.Description,
                Acronym = vm.Acronym,
                LogoPath = "",
                Status = true
            };

            var result = await _politicalPartyService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            if (result.Data != null && result.Data.Id != 0)
            {
                int id = result.Data.Id;

                string imagePath = FileManager.Upload(vm.Logo, id, "PoliticalParties");

                await _politicalPartyService.UpdateLogo(id, imagePath);

                TempData["Success"] = "Partido político creado correctamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _politicalPartyService.GetById(id);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            SavePoliticalPartyViewModel vm = new()
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                Acronym = result.Data.Acronym,
                ExistingLogoPath = result.Data.LogoPath,
                Status = result.Data.Status
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavePoliticalPartyViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string logoPath = vm.ExistingLogoPath ?? "";

            if (vm.Logo != null)
            {
                if (!IsValidImage(vm.Logo))
                {
                    ModelState.AddModelError("Logo", "El logo del partido debe ser una imagen válida.");
                    return View(vm);
                }

                logoPath = FileManager.Upload(vm.Logo, vm.Id, "PoliticalParties", true, vm.ExistingLogoPath);
            }

            UpdatePoliticalPartyDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                Acronym = vm.Acronym,
                LogoPath = logoPath,
                Status = vm.Status
            };

            var result = await _politicalPartyService.UpdateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Activate(int id)
        {
            ViewBag.Id = id;
            ViewBag.Message = "¿Está seguro que desea activar este partido político?";
            ViewBag.ActionName = "ActivateConfirmed";
            ViewBag.ControllerName = "PoliticalParty";
            ViewBag.ButtonClass = "btn-success";
            ViewBag.ButtonText = "Activar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var result = await _politicalPartyService.ActivateAsync(id);

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
            ViewBag.Message = "¿Está seguro que desea desactivar este partido político?";
            ViewBag.ActionName = "DeactivateConfirmed";
            ViewBag.ControllerName = "PoliticalParty";
            ViewBag.ButtonClass = "btn-danger";
            ViewBag.ButtonText = "Desactivar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var result = await _politicalPartyService.DeactivateAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        private bool IsValidImage(IFormFile file)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(extension);
        }
    }
}
