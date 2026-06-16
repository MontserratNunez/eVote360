using eVote360.Attributes;
using eVote360.Core.Application.Dtos.PoliticalAlliance;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.PoliticalAlliance;
using eVote360.Core.Application.ViewModels.PoliticalParty;
using eVote360.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    [DirectorAuthorize]
    public class PoliticalAllianceController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IPoliticalAllianceService _allianceService;

        public PoliticalAllianceController(IUserSession userSession, IPoliticalAllianceService politicalAllianceService)
        { 
            _userSession = userSession;
            _allianceService = politicalAllianceService;
        }

        
        public async Task<IActionResult> Create()
        {
            if (await _allianceService.HasActiveElection())
            {
                TempData["Error"] = "No se pueden crear solicitudes mientras exista una elección activa.";
                return RedirectToAction(nameof(Current));
            }

            var user = _userSession.GetUserSession();

            var parties = await _allianceService.GetAvailableParties(user.Id);

            var vm = new CreatePoliticalAllianceViewModel
            {
                Parties = parties
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePoliticalAllianceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var user = _userSession.GetUserSession();
                vm.Parties = await _allianceService.GetAvailableParties(user.Id);

                return View(vm);
            }

            var userSession = _userSession.GetUserSession();

            var dto = new CreatePoliticalAllianceDto
            {
                ReceivingPartyId = vm.PoliticalPartyId
            };

            var result = await _allianceService.CreateAsync(dto, userSession.Id);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Message;

                vm.Parties = await _allianceService.GetAvailableParties(userSession.Id);

                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> All()
        {
            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            var result = await _allianceService.GetAllSentAsync(user.Id);

            if (!result.IsSuccess)
            {
                ViewBag.Error = result.Message;
                return View(new List<PoliticalAllianceViewModel>());
            }

            var vm = result.Data.Select(a => new PoliticalAllianceViewModel
            {
                Id = a.Id,
                PartyName = a.PartyName,
                RequestDate = a.RequestDate,
                Status = a.Status,
                StatusText = a.StatusText
            }).ToList();

            ViewBag.HasActiveElection = await _allianceService.HasActiveElection();
            var pending = await _allianceService.GetPendingCountAsync(user.Id);
            ViewBag.PendingCount = pending.Data;

            return View(vm);
        }

        public async Task<IActionResult> Current()
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _allianceService.GetCurrentAsync(user.Id);

            if (!result.IsSuccess || result.Data == null)
            {
                ViewBag.Error = result.Message;
                return View(new List<CurrentAllianceViewModel>());
            }

            var vm = result.Data.Select(a => new CurrentAllianceViewModel
            {
                Id = a.Id,
                PartnerName = a.PartnerName,
                AcceptanceDate = a.AcceptanceDate
            }).ToList();

            ViewBag.HasActiveElection = await _allianceService.HasActiveElection();
            var pending = await _allianceService.GetPendingCountAsync(user.Id);
            ViewBag.PendingCount = pending.Data;

            return View(vm);
        }

        public async Task<IActionResult> Pending()
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _allianceService.GetPendingAsync(user.Id);

            if (!result.IsSuccess || result.Data == null)
            {
                ViewBag.Error = result.Message;
                return View(new List<PendingAllianceViewModel>());
            }

            var vm = result.Data.Select(a => new PendingAllianceViewModel
            {
                Id = a.Id,
                PartyName = a.PartyName,
                RequestDate = a.RequestDate,
                Status = a.Status
            }).ToList();

            ViewBag.HasActiveElection = await _allianceService.HasActiveElection();
            ViewBag.PendingCount = result.Data.Count;

            return View(vm);
        }

        public async Task<IActionResult> Confirm(int id, bool accept)
        {
            var result = await _allianceService.GetPartyName(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            var partyName = result.Data;

            ViewBag.Id = id;
            ViewBag.ControllerName = "PoliticalAlliance";

            if (accept)
            {
                ViewBag.Message = $"¿Está seguro que desea aceptar la alianza con el partido {partyName}?";
                ViewBag.ActionName = "AcceptConfirmed";
                ViewBag.ButtonClass = "btn-success";
                ViewBag.ButtonText = "Aceptar";
            }
            else
            {
                ViewBag.Message = $"¿Está seguro que desea rechazar la alianza con el partido {partyName}?";
                ViewBag.ActionName = "RejectConfirmed";
                ViewBag.ButtonClass = "btn-danger";
                ViewBag.ButtonText = "Rechazar";
            }

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> AcceptConfirmed(int id)
        {
            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            var result = await _allianceService.AcceptAsync(id, user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Current));
        }

        [HttpPost]
        public async Task<IActionResult> RejectConfirmed(int id)
        {
            var user = _userSession.GetUserSession();

            if (user == null)
                return RedirectToAction("Index", "Login");

            var result = await _allianceService.RejectAsync(id, user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(All));
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmDeleteRequest(int id)
        {
            var result = await _allianceService.GetPartyName(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            var partyName = result.Data;

            ViewBag.Id = id;
            ViewBag.ControllerName = "PoliticalAlliance";
            ViewBag.ActionName = "DeleteConfirmedRequest";
            ViewBag.Message = $"¿Está seguro que desea eliminar la solicitud de alianza con el partido {partyName}?";
            ViewBag.ButtonClass = "btn-danger";
            ViewBag.ButtonText = "Eliminar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmedRequest(int id)
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _allianceService.DeleteRequestAsync(id, user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(All));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDeleteCurrent(int id)
        {

            var user = _userSession.GetUserSession();

            var result = await _allianceService.GetPartyNameCurrent(id, user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            var partyName = result.Data;

            ViewBag.Id = id;
            ViewBag.ControllerName = "PoliticalAlliance";
            ViewBag.ActionName = "DeleteCurrentConfirmed";
            ViewBag.Message = $"¿Está seguro que desea eliminar la alianza política con el partido {partyName}?";
            ViewBag.ButtonClass = "btn-danger";
            ViewBag.ButtonText = "Eliminar";

            return View("Confirm");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCurrentConfirmed(int id)
        {
            var user = _userSession.GetUserSession();

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _allianceService.DeleteCurrentAsync(id, user.Id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(All));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(All));
        }
    }
}
