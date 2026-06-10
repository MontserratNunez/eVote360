using eVote360.Core.Application.Dtos.CandidatePositionAssignment;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.CandidatePositionAssignment;
using eVote360.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class CandidatePositionAssignmentController : Controller
    {
        private readonly ICandidatePositionAssignmentService _candidatePositionAssignmentService;
        private readonly IUserSession _userSession;

        public CandidatePositionAssignmentController(
            ICandidatePositionAssignmentService candidatePositionAssignmentService,
            IUserSession userSession)
        {
            _candidatePositionAssignmentService = candidatePositionAssignmentService;
            _userSession = userSession;
        }

        public async Task<IActionResult> Index()
        {
            var validationResult = ValidateDirectorSession();

            if (validationResult != null)
            {
                return validationResult;
            }

            var user = _userSession.GetUserSession();
            int politicalPartyId = user!.PoliticalPartyId!.Value;

            var result = await _candidatePositionAssignmentService.GetAllByPoliticalPartyAsync(politicalPartyId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return View(new List<CandidatePositionAssignmentViewModel>());
            }

            var vm = result.Data.Select(a => new CandidatePositionAssignmentViewModel
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                CandidateName = a.CandidateName,
                CandidateLastName = a.CandidateLastName,
                CandidatePoliticalPartyId = a.CandidatePoliticalPartyId,
                CandidatePoliticalPartyName = a.CandidatePoliticalPartyName,
                ElectivePositionId = a.ElectivePositionId,
                ElectivePositionName = a.ElectivePositionName,
                PoliticalPartyId = a.PoliticalPartyId,
                PoliticalPartyName = a.PoliticalPartyName,
                IsAlliedCandidate = a.IsAlliedCandidate
            }).ToList();

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            var validationResult = ValidateDirectorSession();

            if (validationResult != null)
            {
                return validationResult;
            }

            var user = _userSession.GetUserSession();
            int politicalPartyId = user!.PoliticalPartyId!.Value;

            var result = await _candidatePositionAssignmentService.GetCreateViewModelAsync(politicalPartyId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCandidatePositionAssignmentViewModel vm)
        {
            var validationResult = ValidateDirectorSession();

            if (validationResult != null)
            {
                return validationResult;
            }

            var user = _userSession.GetUserSession();
            int politicalPartyId = user!.PoliticalPartyId!.Value;

            if (!ModelState.IsValid)
            {
                var formResult = await _candidatePositionAssignmentService.GetCreateViewModelAsync(politicalPartyId);

                if (formResult.Data != null)
                {
                    vm.Candidates = formResult.Data.Candidates;
                    vm.ElectivePositions = formResult.Data.ElectivePositions;
                }

                vm.PoliticalPartyId = politicalPartyId;

                return View(vm);
            }

            var dto = new CreateCandidatePositionAssignmentDto
            {
                CandidateId = vm.CandidateId,
                ElectivePositionId = vm.ElectivePositionId,
                PoliticalPartyId = politicalPartyId
            };

            var result = await _candidatePositionAssignmentService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("assignmentValidation", result.Message);

                var formResult = await _candidatePositionAssignmentService.GetCreateViewModelAsync(politicalPartyId);

                if (formResult.Data != null)
                {
                    vm.Candidates = formResult.Data.Candidates;
                    vm.ElectivePositions = formResult.Data.ElectivePositions;
                }

                vm.PoliticalPartyId = politicalPartyId;

                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var validationResult = ValidateDirectorSession();

            if (validationResult != null)
            {
                return validationResult;
            }

            var user = _userSession.GetUserSession();
            int politicalPartyId = user!.PoliticalPartyId!.Value;

            var result = await _candidatePositionAssignmentService.GetDeleteInfoAsync(id, politicalPartyId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var vm = new DeleteCandidatePositionAssignmentViewModel
            {
                Id = result.Data.Id,
                CandidateFullName = result.Data.CandidateFullName,
                ElectivePositionName = result.Data.ElectivePositionName,
                PoliticalPartyName = result.Data.PoliticalPartyName
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteCandidatePositionAssignmentViewModel vm)
        {
            var validationResult = ValidateDirectorSession();

            if (validationResult != null)
            {
                return validationResult;
            }

            var user = _userSession.GetUserSession();
            int politicalPartyId = user!.PoliticalPartyId!.Value;

            var result = await _candidatePositionAssignmentService.DeleteAsync(vm.Id, politicalPartyId);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private IActionResult? ValidateDirectorSession()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (user.Role != Role.DIRECTOR)
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            if (!user.PoliticalPartyId.HasValue)
            {
                TempData["Error"] = "Este usuario no tiene un partido político asignado.";
                return RedirectToRoute(new { controller = "HomeDirector", action = "Index" });
            }

            return null;
        }
    }
}