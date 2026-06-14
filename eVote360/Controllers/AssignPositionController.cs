using eVote360.Attributes;
using eVote360.Core.Application.Dtos.AssignPosition;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.AssignPosition;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [DirectorAuthorize]
    public class AssignPositionController : Controller
    {

        private readonly IUserSession _userSession;
        private readonly IAssignPositionService _assignPositionService;

        public AssignPositionController(IUserSession userSession, IAssignPositionService assignPositionService)
        {
            _userSession = userSession;
            _assignPositionService = assignPositionService;
        }

        public async Task<IActionResult> Index()
        {
            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            var result = await _assignPositionService.GetAllAsync(user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new AssignPositionIndexViewModel());
            }

            var dto = result.Data;

            var vm = new AssignPositionIndexViewModel
            {
                CurrentAssignments = dto.CurrentAssignments.Select(a => new AssignPositionViewModel
                {
                    Id = a.Id,
                    CandidateName = a.CandidateName,
                    CandidateLastName = a.CandidateLastName,
                    CandidatePartyName = a.CandidatePartyName,
                    PositionName = a.PositionName,
                    CandidateType = a.CandidateType,
                    IsAlliance = a.IsAlliance,
                    IsEditable = a.IsEditable
                }).ToList(),

                PastAssignments = dto.PastAssignments.Select(a => new AssignPositionViewModel
                {
                    Id = a.Id,
                    CandidateName = a.CandidateName,
                    CandidateLastName = a.CandidateLastName,
                    CandidatePartyName = a.CandidatePartyName,
                    PositionName = a.PositionName,
                    CandidateType = a.CandidateType,
                    IsAlliance = a.IsAlliance,
                    IsEditable = a.IsEditable
                }).ToList()
            };

            ViewBag.HasActiveElection = await _assignPositionService.HasActiveElection();

            return View(vm);
        }

        /*
        public async Task<IActionResult> Index()
        {
            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            var result = await _assignPositionService.GetAllAsync(user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new List<AssignPositionViewModel>());
            }

            var vm = result.Data.Select(a => new AssignPositionViewModel
            {
                Id = a.Id,
                CandidateName = a.CandidateName,
                CandidateLastName = a.CandidateLastName,
                CandidatePartyName = a.CandidatePartyName,
                PositionName = a.PositionName,
                CandidateType = a.CandidateType,
                IsAlliance = a.IsAlliance,
                IsEditable = a.IsEditable
            }).ToList();

            ViewBag.HasActiveElection = await _assignPositionService.HasActiveElection();

            return View(vm);
        }
        */


        public async Task<IActionResult> Create()
        {
            if (await _assignPositionService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden modificar asignaciones mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            var user = _userSession.GetUserSession();

            var (candidates, positions) = await _assignPositionService.GetDropdowns(user.Id);

            var vm = new CreateAssignPositionViewModel
            {
                Candidates = candidates,
                Positions = positions
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssignPositionViewModel vm)
        {
            var user = _userSession.GetUserSession();
            if (user == null)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                var data = await _assignPositionService.GetDropdowns(user.Id);
                vm.Candidates = data.Candidates;
                vm.Positions = data.Positions;

                return View(vm);
            }  

            var dto = new CreateAssignPositionDto
            {
                CandidateId = vm.CandidateId,
                ElectivePositionId = vm.ElectivePositionId
            };

            var result = await _assignPositionService.CreateAsync(dto, user.Id);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;

                var data = await _assignPositionService.GetDropdowns(user.Id);
                vm.Candidates = data.Candidates;
                vm.Positions = data.Positions;

                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "AssignPosition";
            ViewBag.ActionName = "DeleteConfirmed";
            ViewBag.Message = "¿Está seguro que desea desvincular este candidato de este puesto electivo?";
            ViewBag.ButtonClass = "btn-danger";
            ViewBag.ButtonText = "Eliminar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _assignPositionService.DeleteAsync(id, user.Id);

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
