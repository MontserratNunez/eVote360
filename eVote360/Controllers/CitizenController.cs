using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Citizen;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class CitizenController : Controller
    {
        private readonly ICitizenService _citizenService;
        private readonly IUserSession _userSession;

        public CitizenController(ICitizenService citizenService, IUserSession userSession)
        {
            _citizenService = citizenService;
            _userSession = userSession;
        }

        private bool IsAuthorized()
        {
            return _userSession.HasUser() && _userSession.IsAdmin();
        }

        // Stub: Reemplazar con llamada real a IElectionService
        private async Task<bool> IsElectionActive()
        {
            return false;
        }

        // Stub: Reemplazar con llamada real a IVoteRepository o IElectionService
        private async Task<bool> HasCitizenParticipated(int citizenId)
        {
            return false;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            ViewBag.HasActiveElection = await IsElectionActive();
            var citizens = await _citizenService.GetAllAsync();
            return View(citizens);
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            if (await IsElectionActive())
            {
                TempData["ErrorMessage"] = "No se pueden crear ciudadanos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }
            return View("Save", new SaveCitizenViewModel() { Name = "", LastName = "", Email = "", DocumentNumber = "" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveCitizenViewModel vm)
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            if (await IsElectionActive()) return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid) return View("Save", vm);

            if (await _citizenService.ExistsByDocumentAsync(vm.DocumentNumber))
            {
                ModelState.AddModelError("DocumentNumber", "Ya existe un ciudadano registrado con este número de documento de identidad.");
                return View("Save", vm);
            }

            if (await _citizenService.ExistsByEmailAsync(vm.Email))
            {
                ModelState.AddModelError("Email", "Ya existe un ciudadano registrado con este correo electrónico.");
                return View("Save", vm);
            }

            await _citizenService.AddAsync(vm);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            if (await IsElectionActive())
            {
                TempData["ErrorMessage"] = "No se pueden editar ciudadanos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            var vm = await _citizenService.GetByIdSaveViewModelAsync(id);
            if (vm == null) return RedirectToAction(nameof(Index));

            ViewBag.HasParticipated = await HasCitizenParticipated(id);

            return View("Save", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveCitizenViewModel vm)
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            if (await IsElectionActive()) return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                ViewBag.HasParticipated = await HasCitizenParticipated(vm.Id);
                return View("Save", vm);
            }

            
            bool hasParticipated = await HasCitizenParticipated(vm.Id);
            if (hasParticipated)
            {
                var originalCitizen = await _citizenService.GetByIdSaveViewModelAsync(vm.Id);
                if (originalCitizen != null && originalCitizen.DocumentNumber != vm.DocumentNumber)
                {
                    ModelState.AddModelError("DocumentNumber", "No se puede modificar el número de documento de identidad porque este ciudadano ya participó en una elección.");
                    ViewBag.HasParticipated = true;
                    vm.DocumentNumber = originalCitizen.DocumentNumber; 
                    return View("Save", vm);
                }
            }
            else
            {
                if (await _citizenService.ExistsByDocumentAsync(vm.DocumentNumber, vm.Id))
                {
                    ModelState.AddModelError("DocumentNumber", "Ya existe un ciudadano registrado con este número de documento de identidad.");
                    ViewBag.HasParticipated = false;
                    return View("Save", vm);
                }
            }

            if (await _citizenService.ExistsByEmailAsync(vm.Email, vm.Id))
            {
                ModelState.AddModelError("Email", "Ya existe un ciudadano registrado con este correo electrónico.");
                ViewBag.HasParticipated = hasParticipated;
                return View("Save", vm);
            }

            await _citizenService.UpdateAsync(vm);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            if (!IsAuthorized()) return RedirectToAction("Index", "Home");

            if (await IsElectionActive())
            {
                TempData["ErrorMessage"] = "No se pueden modificar ciudadanos mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            await _citizenService.ChangeStatusAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}