using eVote360.Attributes;
using eVote360.Core.Application.Dtos.Election;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Election;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [AdminAuthorize]
    public class ElectionController : Controller
    {
        private readonly IElectionService _electionService;
        
        public ElectionController(IElectionService electionService)
        {
            _electionService = electionService;
        
        }

        public async Task<IActionResult> Index()
        {
            var result = await _electionService.GetAllAsync();

            if (!result.IsSuccess)
            {
                ViewBag.Error = result.Message;
                return View(new List<ElectionViewModel>());
            }

            var vm = result.Data.Select(e => new ElectionViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Date = e.Date,
                StatusText = e.StatusText,
                Status = e.Status,
                PartiesCount = e.PartiesCount,
                PositionsCount = e.PositionsCount,
                VotersCount = e.VotersCount
            }).ToList();

            ViewBag.HasActiveElection = await _electionService.HasActiveElection();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await _electionService.HasActiveElection())
            {
                TempData["Error"] = "No se puede crear una elección mientras exista una activa.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateElectionViewModel() { Date = DateTime.Today});
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateElectionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new CreateElectionDto
            {
                Name = vm.Name,
                Date = vm.Date
            };

            var result = await _electionService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmActivate(int id)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "Election";
            ViewBag.ActionName = "ActivateConfirmed";
            ViewBag.Message = "¿Está seguro que desea activar esta elección?";
            ViewBag.ButtonClass = "btn-success";
            ViewBag.ButtonText = "Activar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var result = await _electionService.ActivateAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmFinish(int id)
        {            
            ViewBag.Id = id;
            ViewBag.ControllerName = "Election";
            ViewBag.ActionName = "FinishConfirmed";
            ViewBag.Message = "¿Está seguro que desea finalizar esta elección?";
            ViewBag.ButtonClass = "btn-danger";
            ViewBag.ButtonText = "Finalizar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> FinishConfirmed(int id)
        {
            var result = await _electionService.FinishAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Results(int id)
        {
            var result = await _electionService.GetResultsByElectionAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            var dto = result.Data;

            var viewModel = new ElectionResultViewModel
            {
                ElectionId = dto.ElectionId,
                ElectionName = dto.ElectionName,
                Positions = dto.Positions.Select(p => new PositionResultSummaryViewModel
                {
                    PositionId = p.PositionId,
                    PositionName = p.PositionName,
                    TotalVotesInPosition = p.TotalVotesInPosition,
                    IsTie = p.IsTie,
                    Results = p.Results.Select(r => new CandidateResultViewModel
                    {
                        CandidateId = r.CandidateId,
                        CandidateFullName = r.CandidateFullName,
                        PoliticalPartyName = r.PoliticalPartyName,
                        VotesCount = r.VotesCount,
                        Percentage = r.Percentage,
                        IsWinner = r.IsWinner
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
