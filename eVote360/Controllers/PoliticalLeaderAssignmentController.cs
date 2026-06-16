using eVote360.Attributes;
using eVote360.Core.Application.Dtos.LeaderAssignment;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.LeaderAssignment;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [AdminAuthorize]
    public class PoliticalLeaderController : Controller
    {
        private readonly IPoliticalLeaderAssignmentService _assignmentService;

        public PoliticalLeaderController(IPoliticalLeaderAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _assignmentService.GetAllAsync();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(new List<LeaderAssignmentViewModel>());
            }

            var vm = result.Data.Select(a => new LeaderAssignmentViewModel
            {
                Id = a.Id,
                FullName = a.FullName,
                UserName = a.UserName,
                PartyName = a.PartyName,
                PartyAcronym = a.PartyAcronym,
                UserStatus = a.UserStatus,
                PartyStatus = a.PartyStatus
            }).ToList();

            ViewBag.HasActiveElection = await _assignmentService.HasActiveElection();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await _assignmentService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden modificar asignaciones mientras exista una elección activa.";
                return RedirectToAction(nameof(Index));
            }

            var (users, parties) = await _assignmentService.GetDropdowns();

            CreateAssignmentViewModel vm = new()
            {
                Users = users,
                Parties = parties
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssignmentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var (users, parties) = await _assignmentService.GetDropdowns();
                vm.Users = users;
                vm.Parties = parties;

                return View(vm);
            }

            CreateAssignmentDto dto = new()
            {
                UserId = vm.UserId,
                PoliticalPartyId = vm.PoliticalPartyId
            };

            var result = await _assignmentService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;

                var (users, parties) = await _assignmentService.GetDropdowns();
                vm.Users = users;
                vm.Parties = parties;

                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Confirm(int id)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "PoliticalLeader";
            ViewBag.ActionName = "DeleteConfirmed";
            ViewBag.Message = "¿Está seguro que desea desvincular este dirigente político de este partido?";
            ViewBag.ButtonClass = "btn-danger";
            ViewBag.ButtonText = "Eliminar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _assignmentService.DeleteAsync(id);

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
