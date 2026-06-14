using eVote360.Attributes;
using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Candidate;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.Services;
using eVote360.Core.Application.ViewModels.Candidate;
using eVote360.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [DirectorAuthorize]
    public class CandidateController : Controller
    {

        private readonly ICandidateService _candidateService;
        private readonly IUserSession _userSession;

        public CandidateController(ICandidateService candidateService, IUserSession userSession)
        {
            _candidateService = candidateService;
            _userSession = userSession;
        }

        public async Task<IActionResult> Index()
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _candidateService.GetAllAsync(user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new List<CandidateViewModel>());
            }

            var vm = result.Data.Select(c => new CandidateViewModel
            {
                Id = c.Id,
                Name = c.Name,
                LastName = c.LastName,
                PhotoPath = c.PhotoPath,
                PositionName = c.PositionName,
                Status = c.Status
            }).ToList();

            ViewBag.HasActiveElection = await _candidateService.HasActiveElection();

            return View(vm);
        }

        
        public async Task<IActionResult> Create()
        {
            if (await _candidateService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden crear candidatos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateCandidateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCandidateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.Photo == null || !IsValidImage(vm.Photo))
            {
                ModelState.AddModelError("Photo", "La foto del candidato debe ser una imagen válida.");
                return View(vm);
            }

            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            CreateCandidateDto dto = new()
            {
                Name = vm.Name,
                LastName = vm.LastName,
                PhotoPath = "",
                Status = true
            };

            var result = await _candidateService.CreateAsync(dto, user.Id);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            if (result.Data != null && result.Data.Id != 0)
            {
                int id = result.Data.Id;

                string imagePath = FileManager.Upload(vm.Photo, id, "Candidates");

                await _candidateService.UpdatePhoto(id, imagePath);

                TempData["Success"] = "Candidato creado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (await _candidateService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden editar candidatos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            var result = await _candidateService.GetById(id, user.Id);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            EditCandidateViewModel vm = new()
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                LastName = result.Data.LastName,
                ExistingPhotoPath = result.Data.PhotoPath,
                Status = result.Data.Status
            };

            ViewBag.HasParticipated = await _candidateService.HasCandidateParticipated(id);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditCandidateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HasParticipated = await _candidateService.HasCandidateParticipated(vm.Id);
                return View(vm);
            }

            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            string photoPath = vm.ExistingPhotoPath ?? "";

            if (vm.Photo != null)
            {
                if (!IsValidImage(vm.Photo))
                {
                    ModelState.AddModelError("Photo", "La foto del candidato debe ser una imagen válida.");
                    return View(vm);
                }

                photoPath = FileManager.Upload(vm.Photo, vm.Id, "Candidates", true, vm.ExistingPhotoPath);
            }

            UpdateCandidateDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                LastName = vm.LastName,
                PhotoPath = photoPath,
                Status = vm.Status
            };

            var result = await _candidateService.UpdateAsync(dto, user.Id);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                ViewBag.HasParticipated = await _candidateService.HasCandidateParticipated(vm.Id);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

       
        public IActionResult Confirm(int id, bool activate)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "Candidate";

            if (activate)
            {
                ViewBag.Message = "¿Está seguro que desea activar este candidato?";
                ViewBag.ActionName = "ActivateConfirmed";
                ViewBag.ButtonClass = "btn-success";
                ViewBag.ButtonText = "Activar";
            }
            else
            {
                ViewBag.Message = "¿Está seguro que desea desactivar este candidato?";
                ViewBag.ActionName = "DeactivateConfirmed";
                ViewBag.ButtonClass = "btn-danger";
                ViewBag.ButtonText = "Desactivar";
            }

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _candidateService.ActivateAsync(id, user.Id);

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
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _candidateService.DeactivateAsync(id, user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }



        private bool IsValidImage(IFormFile file)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(extension);
        }
    }
}
