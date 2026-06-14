using eVote360.Core.Application.Dtos.Vote;
using eVote360.Core.Application.Helpers;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Citizen;
using eVote360.Core.Application.ViewModels.Vote;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class VoteController : Controller
    {
        private readonly IVoteService _voteService;
        private readonly ICitizenSession _citizenSession;

        public VoteController(IVoteService voteService, ICitizenSession citizenSession)
        {
            _voteService = voteService;
            _citizenSession = citizenSession;
        }

        public IActionResult Index()
        {
            var session = _citizenSession.Get();

            if (session != null)
            {
                return RedirectToAction("OCR");
            }

            return View(new VoteViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(VoteViewModel vm)
        {
            var existingSession = _citizenSession.Get();

            if (existingSession != null)
            {
                return RedirectToAction("OCR");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _voteService.StartAsync(new StartVoteDto
            {
                DocumentNumber = vm.DocumentNumber
            });

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("citizenValidation", result.Message);
                return View(vm);
            }

            var (citizenId, electionId) = result.Data;

            var citizenSession = new CitizenSession
            {
                CitizenId = citizenId,
                ElectionId = electionId,
                IdentityValidated = false,
                CodeValidated = false,
                HasFinalizedVoted = false
            };

            _citizenSession.Set(citizenSession);

            return RedirectToAction("OCR");
        }


        public IActionResult OCR()
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            return View(new OcrValidationViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> OCR(OcrValidationViewModel vm)
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new OcrValidationDto
            {
                File = vm.File,
                CitizenId = session.CitizenId,
                ElectionId = session.ElectionId,
            };

            var result = await _voteService.ValidateIdentityAsync(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(vm);
            }

            session.IdentityValidated = true;
            _citizenSession.Set(session);

            return RedirectToAction("VerifyCode");
        }

        public IActionResult VerifyCode()
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!session.IdentityValidated)
                return RedirectToAction(nameof(OCR));

            if (session.CodeValidated)
                return RedirectToAction("Positions");

            return View(new VerifyCodeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel vm)
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!session.IdentityValidated)
                return RedirectToAction(nameof(OCR));

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new VerifyCodeDto
            {
                CitizenId = session.CitizenId,
                ElectionId = session.ElectionId,
                Code = vm.Code.Trim()
            };

            var result = await _voteService.VerifyCodeAsync(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(vm);
            }

            session.CodeValidated = true;
            _citizenSession.Set(session);

            return RedirectToAction("Positions");
        }

        public async Task<IActionResult> Positions()
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!session.IdentityValidated)
                return RedirectToAction(nameof(OCR));

            if (!session.CodeValidated)
                return RedirectToAction(nameof(VerifyCode));

            var result = await _voteService.GetAvailablePositionsAsync(session.SelectedVotes);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(new PositionsViewModel());
            }

            var vm = new PositionsViewModel
            {
                Positions = result.Data.Select(c => new AvailablePositionsViewModel
                {
                    Id = c.Id,
                    PositionName = c.PositionName,
                    TotalParties = c.TotalParties,
                    TotalRealCandidates = c.TotalRealCandidates,
                    IsSelected = c.IsSelected,
                }).ToList(),
                CanFinalizeVoting = session.SelectedVotes.Any(v => v.Value.HasValue)
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult SelectPosition(int positionId)
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!session.IdentityValidated)
                return RedirectToAction(nameof(OCR));

            if (!session.CodeValidated)
                return RedirectToAction(nameof(VerifyCode));

            if (positionId <= 0)
            {
                return RedirectToAction(nameof(Positions));
            }

            if (!session.SelectedVotes.ContainsKey(positionId))
            {
                session.SelectedVotes[positionId] = null;
                _citizenSession.Set(session);
            }

            return RedirectToAction("Candidates", new { positionId = positionId });
        }


        public async Task<IActionResult> Candidates(int positionId)
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!session.IdentityValidated)
                return RedirectToAction(nameof(OCR));

            if (!session.CodeValidated)
                return RedirectToAction(nameof(VerifyCode));

            var result = await _voteService.GetCandidatesByPositionAsync(positionId);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Positions));
            }

            var vm = new CandidatesViewModel
            {
                PositionId = result.Data.PositionId,
                PositionName = result.Data.PositionName,
                Candidates = result.Data.Candidates.Select(c => new CandidateOptionViewModel {
                    AssignPositionId = c.AssignPositionId,
                    CandidateId = c.CandidateId,
                    CandidateFullName = c.CandidateFullName,
                    CandidatePhotoPath = c.CandidatePhotoPath,
                    PoliticalPartyId = c.PoliticalPartyId,
                    PoliticalPartyName = c.PoliticalPartyName,
                    PoliticalPartyLogoPath = c.PoliticalPartyLogoPath
                }).ToList(),
                SelectedAssignPositionId = session.SelectedVotes.ContainsKey(positionId) ? session.SelectedVotes[positionId] : null
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Candidates(CandidatesViewModel vm)
        {
            var session = _citizenSession.Get();

            if (session == null)
                return RedirectToAction(nameof(Index));

            if (!session.IdentityValidated)
                return RedirectToAction(nameof(OCR));

            if (!session.CodeValidated)
                return RedirectToAction(nameof(VerifyCode));

            if (!ModelState.IsValid || vm.SelectedAssignPositionId == null)
            {
                var result = await _voteService.GetCandidatesByPositionAsync(vm.PositionId);
                vm.Candidates = result.Data.Candidates.Select(c => new CandidateOptionViewModel
                {
                    AssignPositionId = c.AssignPositionId,
                    CandidateId = c.CandidateId,
                    CandidateFullName = c.CandidateFullName,
                    CandidatePhotoPath = c.CandidatePhotoPath,
                    PoliticalPartyId = c.PoliticalPartyId,
                    PoliticalPartyName = c.PoliticalPartyName,
                    PoliticalPartyLogoPath = c.PoliticalPartyLogoPath
                }).ToList();

                ModelState.AddModelError("", "Debe seleccionar un candidato antes de votar.");
                return View(vm);
            }

            session.SelectedVotes[vm.PositionId] = vm.SelectedAssignPositionId.Value;

            _citizenSession.Set(session);

            return RedirectToAction(nameof(Positions));
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeVote()
        {
            var session = _citizenSession.Get();

            if (session == null) return RedirectToAction(nameof(Index));
            if (!session.IdentityValidated || !session.CodeValidated) return RedirectToAction(nameof(Index));
            if (session.HasFinalizedVoted) return RedirectToAction("VotedSuccess");

            var dto = new FinalizeVoteDto
            {
                CitizenId = session.CitizenId,
                ElectionId = session.ElectionId,
                SelectedVotes = session.SelectedVotes
            };

            var result = await _voteService.FinalizeVotingAsync(dto);

            if (!result.IsSuccess)
            {
                TempData["FinalizeErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Positions));
            }

            session.HasFinalizedVoted = true;
            _citizenSession.Set(session);

            return RedirectToAction(nameof(VotedSuccess));
        }

        public IActionResult VotedSuccess()
        {
            var session = _citizenSession.Get();
            if (session == null || !session.HasFinalizedVoted)
            {
                return RedirectToAction(nameof(Index));
            }

            _citizenSession.Remove();

            return View();
        }
    }
}
