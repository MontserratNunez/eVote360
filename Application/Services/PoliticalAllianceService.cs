using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.PoliticalAlliance;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class PoliticalAllianceService : IPoliticalAllianceService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IPoliticalLeaderAssignmentRepository _assignmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPoliticalPartyRepository _partyRepository;
        private readonly IPoliticalAllienceRepository _allianceRepository;
        private readonly IElectionRepository _electionRepository;

        public PoliticalAllianceService(ICandidateRepository candidateRepository,
            IPoliticalLeaderAssignmentRepository assignmentRepository, 
            IUserRepository userRepository,
            IPoliticalPartyRepository politicalPartyRepository, 
            IPoliticalAllienceRepository politicalAllienceRepository,
            IElectionRepository electionRepository
            )
        {
            _candidateRepository = candidateRepository;
            _assignmentRepository = assignmentRepository;
            _userRepository = userRepository;
            _partyRepository = politicalPartyRepository;
            _allianceRepository = politicalAllienceRepository;
            _electionRepository = electionRepository;
        }

        public async Task<Result> CreateAsync(CreatePoliticalAllianceDto dto, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede crear una solicitud de alianza mientras exista una elección activa."
                };
            }

            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene un partido político asignado."
                };
            }

            int myPartyId = assignment.PoliticalPartyId;

            if (dto.ReceivingPartyId == myPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No puede crear una solicitud de alianza hacia su propio partido político."
                };
            }

            var party = await _partyRepository.GetById(dto.ReceivingPartyId);

            if (party == null)
            {
                return new Result { IsSuccess = false, Message = "El partido seleccionado no existe." };
            }

            if (!party.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No puede crear una solicitud de alianza con un partido político inactivo."
                };
            }

            var myParty = await _partyRepository.GetById(myPartyId);

            if (!myParty!.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político asignado se encuentra inactivo."
                };
            }

            bool existsPendingFromMe = await _allianceRepository
                .GetAllQuery()
                .AnyAsync(a =>
                    a.RequestingPartyId == myPartyId &&
                    a.ReceivingPartyId == dto.ReceivingPartyId &&
                    a.Status == PoliticalAllianceStatus.PENDING);

            if (existsPendingFromMe)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe una solicitud de alianza pendiente enviada a este partido político."
                };
            }

            bool existsPendingFromOther = await _allianceRepository
                .GetAllQuery()
                .AnyAsync(a =>
                    a.RequestingPartyId == dto.ReceivingPartyId &&
                    a.ReceivingPartyId == myPartyId &&
                    a.Status == PoliticalAllianceStatus.PENDING);

            if (existsPendingFromOther)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe una solicitud de alianza pendiente enviada por este partido político."
                };
            }

            bool existsAlliance = await _allianceRepository
                .GetAllQuery()
                .AnyAsync(a =>
                    (
                        a.RequestingPartyId == myPartyId && a.ReceivingPartyId == dto.ReceivingPartyId ||
                        a.RequestingPartyId == dto.ReceivingPartyId && a.ReceivingPartyId == myPartyId
                    )
                    && a.Status == PoliticalAllianceStatus.ACCEPTED);

            if (existsAlliance)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe una alianza vigente con este partido político."
                };
            }

            PoliticalAlliance entity = new()
            {
                RequestingPartyId = myPartyId,
                ReceivingPartyId = dto.ReceivingPartyId,
                RequestDate = DateTime.UtcNow,
                Status = PoliticalAllianceStatus.PENDING
            };

            var created = await _allianceRepository.AddAsync(entity);

            if (created == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo crear la solicitud."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Solicitud de alianza creada correctamente."
            };
        }

        public async Task<List<DropdownDto>> GetAvailableParties(int userId)
        {
            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new List<DropdownDto>();
            }

            int myPartyId = assignment.PoliticalPartyId;

            var alliances = await _allianceRepository
                .GetAllQuery()
                .Where(a =>
                    a.Status == PoliticalAllianceStatus.PENDING ||
                    a.Status == PoliticalAllianceStatus.ACCEPTED)
                .ToListAsync();

            var blockedParties = alliances
                .Where(a =>
                    a.RequestingPartyId == myPartyId ||
                    a.ReceivingPartyId == myPartyId)
                .Select(a =>
                    a.RequestingPartyId == myPartyId
                        ? a.ReceivingPartyId
                        : a.RequestingPartyId)
                .ToHashSet();

            var parties = await _partyRepository
                .GetAllQuery()
                .Where(p =>
                    p.Status &&
                    p.Id != myPartyId &&
                    !blockedParties.Contains(p.Id))
                .Select(p => new DropdownDto
                {
                    Id = p.Id,
                    Name = $"{p.Name} ({p.Acronym})"
                })
                .ToListAsync();

            return parties;
        }

        public async Task<Result<List<PoliticalAllianceDto>>> GetAllSentAsync(int userId)
        {
            var result = new Result<List<PoliticalAllianceDto>>();

            try
            {
                var assignment = await _assignmentRepository
                        .GetAllQuery()
                        .FirstOrDefaultAsync(a => a.UserId == userId);

                if (assignment == null)
                {
                    result.IsSuccess = false;
                    result.Message = "No tiene un partido político asignado.";
                    return result;
                }

                int myPartyId = assignment.PoliticalPartyId;

                var alliances = await _allianceRepository.GetAllQueryWithInclude(["ReceivingParty"])
                   .Where(a => a.RequestingPartyId == myPartyId)
                   .ToListAsync();


                result.IsSuccess = true;

                result.Data = alliances.Select(a => new PoliticalAllianceDto
                {
                    Id = a.Id,
                    PartyName = $"{a.ReceivingParty.Name} ({a.ReceivingParty.Acronym})",
                    RequestDate = a.RequestDate,
                    StatusText = GetStatusText(a.Status)
                }).ToList();
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al listar alianzas politicas";
            }

            return result;
        }

        public async Task<Result<List<CurrentAllianceDto>>> GetCurrentAsync(int userId)
        {
            var result = new Result<List<CurrentAllianceDto>>();

            var assignment = await _assignmentRepository.GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                result.IsSuccess = false;
                result.Message = "No tiene un partido político asignado.";
                return result;
            }

            int myPartyId = assignment.PoliticalPartyId;

            var alliances = await _allianceRepository.GetAllQueryWithInclude(new List<string> { "RequestingParty", "ReceivingParty" })
               .Where(a =>
                   a.Status == PoliticalAllianceStatus.ACCEPTED &&
                   (a.RequestingPartyId == myPartyId || a.ReceivingPartyId == myPartyId))
               .ToListAsync();

            result.IsSuccess = true;

            result.Data = alliances.Select(a =>
            {
                var partner = a.RequestingPartyId == myPartyId
                    ? a.ReceivingParty
                    : a.RequestingParty;

                return new CurrentAllianceDto
                {
                    Id = a.Id,
                    PartnerName = $"{partner.Name} ({partner.Acronym})",
                    AcceptanceDate = a.ResponseDate ?? a.RequestDate
                };
            }).ToList();

            return result;
        }

        public async Task<Result<List<PendingAllianceDto>>> GetPendingAsync(int userId)
        {
            var result = new Result<List<PendingAllianceDto>>();

            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                result.IsSuccess = false;
                result.Message = "No tiene un partido político asignado.";
                return result;
            }

            int myPartyId = assignment.PoliticalPartyId;

            var alliances = await _allianceRepository.GetAllQueryWithInclude(["RequestingParty"])
               .Where(a =>
                   a.Status == PoliticalAllianceStatus.PENDING &&
                   a.ReceivingPartyId == myPartyId)
               .ToListAsync();


            result.IsSuccess = true;

            result.Data = alliances.Select(a => new PendingAllianceDto
            {
                Id = a.Id,
                PartyName = $"{a.RequestingParty.Name} ({a.RequestingParty.Acronym})",
                RequestDate = a.RequestDate,
                Status = "En espera de respuesta"
            }).ToList();

            return result;
        }

        public async Task<Result> AcceptAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede aceptar una solicitud de alianza mientras exista una elección activa."
                };
            }

            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene un partido político asignado."
                };
            }

            int myPartyId = assignment.PoliticalPartyId;

            var entity = await _allianceRepository.GetAllQueryWithInclude(new List<string> { "RequestingParty", "ReceivingParty" })
               .FirstOrDefaultAsync(a => a.Id == id);



            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La solicitud no existe."
                };
            }

            if (entity.ReceivingPartyId != myPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para responder esta solicitud de alianza."
                };
            }

            if (entity.Status != PoliticalAllianceStatus.PENDING)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Esta solicitud de alianza ya fue respondida."
                };
            }

            if (!entity.RequestingParty.Status || !entity.ReceivingParty.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Uno de los partidos involucrados está inactivo."
                };
            }

            bool existsAlliance = await _allianceRepository.GetAllQuery()
                .AnyAsync(a =>
                    (
                        a.RequestingPartyId == entity.RequestingPartyId &&
                        a.ReceivingPartyId == entity.ReceivingPartyId ||
                        a.RequestingPartyId == entity.ReceivingPartyId &&
                        a.ReceivingPartyId == entity.RequestingPartyId
                    )
                    && a.Status == PoliticalAllianceStatus.ACCEPTED);

            if (existsAlliance)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe una alianza vigente con este partido político."
                };
            }

            entity.Status = PoliticalAllianceStatus.ACCEPTED;
            entity.ResponseDate = DateTime.UtcNow;

            var updated = await _allianceRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo aceptar la solicitud."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Alianza aceptada correctamente."
            };
        }

        public async Task<Result> RejectAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede rechazar una solicitud de alianza mientras exista una elección activa."
                };
            }

            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene un partido político asignado."
                };
            }

            int myPartyId = assignment.PoliticalPartyId;

            var entity = await _allianceRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La solicitud no existe."
                };
            }

            if (entity.ReceivingPartyId != myPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para responder esta solicitud de alianza."
                };
            }

            if (entity.Status != PoliticalAllianceStatus.PENDING)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Esta solicitud de alianza ya fue respondida."
                };
            }

            entity.Status = PoliticalAllianceStatus.REJECTED;
            entity.ResponseDate = DateTime.UtcNow;

            var updated = await _allianceRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo rechazar la solicitud."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Solicitud de alianza rechazada."
            };
        }

        public async Task<Result> DeleteRequestAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar una solicitud de alianza mientras exista una elección activa."
                };
            }

            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene un partido político asignado."
                };
            }

            int myPartyId = assignment.PoliticalPartyId;

            var entity = await _allianceRepository.GetById(id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La solicitud de alianza seleccionada no existe o ya fue eliminada."
                };
            }

            if (entity.RequestingPartyId != myPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para eliminar esta solicitud de alianza."
                };
            }

            if (entity.Status == PoliticalAllianceStatus.ACCEPTED)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar una solicitud aceptada porque ya generó una alianza vigente. Para terminarla debe eliminar la alianza desde el listado de alianzas vigentes."
                };
            }

            if (entity.Status != PoliticalAllianceStatus.PENDING &&
                entity.Status != PoliticalAllianceStatus.REJECTED)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Estado de la solicitud inválido."
                };
            }

            await _allianceRepository.DeleteAsync(entity.Id);

            return new Result
            {
                IsSuccess = true,
                Message = "Solicitud de alianza eliminada correctamente."
            };
        }

        public async Task<Result> DeleteCurrentAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar una alianza política mientras exista una elección activa."
                };
            }

            var assignment = await _assignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene un partido político asignado."
                };
            }

            int myPartyId = assignment.PoliticalPartyId;

            var entity = await _allianceRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La alianza política seleccionada no existe o ya fue eliminada."
                };
            }

            if (entity.Status != PoliticalAllianceStatus.ACCEPTED)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La alianza no es válida o ya fue eliminada."
                };
            }

            if (entity.RequestingPartyId != myPartyId &&
                entity.ReceivingPartyId != myPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para eliminar esta alianza política."
                };
            }

            if (await HasSharedCandidateAssignments(entity.RequestingPartyId, entity.ReceivingPartyId))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar esta alianza porque existen candidatos aliados asignados entre estos partidos. Primero deben eliminarse las asignaciones correspondientes desde el módulo Asignar candidato a puesto."
                };
            }

            await _allianceRepository.DeleteAsync(entity.Id);

            return new Result
            {
                IsSuccess = true,
                Message = "Alianza eliminada correctamente."
            };
        }

        public async Task<Result<string>> GetPartyName(int id)
        {
            var alliance = await _allianceRepository
               .GetAllQuery()
               .Include(a => a.RequestingParty)
               .FirstOrDefaultAsync(a => a.Id == id);

            if (alliance == null)
            {
                return new Result<string> { IsSuccess = false, Message = "La solicitud no existe." };
            }

            return new Result<string>
            {
                IsSuccess = true,
                Data = $"{alliance.RequestingParty.Name} ({alliance.RequestingParty.Acronym})"
            };
        }

        public async Task<Result<string>> GetPartyNameCurrent(int id, int userId)
        {
            var alliance = await _allianceRepository
                .GetAllQuery()
                .Include(a => a.RequestingParty)
                .Include(a => a.ReceivingParty)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alliance == null)
            {
                return new Result<string> { IsSuccess = false, Message = "La solicitud no existe." };
            }

            var partner = alliance.RequestingPartyId == userId ? alliance.ReceivingParty : alliance.RequestingParty;

            var partyName = $"{partner.Name} ({partner.Acronym})";

            return new Result<string>
            {
                IsSuccess = true,
                Data = $"{alliance.RequestingParty.Name} ({alliance.RequestingParty.Acronym})"
            };
        }


        private string GetStatusText(PoliticalAllianceStatus status)
        {
            switch (status)
            {
                case PoliticalAllianceStatus.PENDING:
                    return "En espera de respuesta";
                case PoliticalAllianceStatus.ACCEPTED:
                    return "Aceptada";
                case PoliticalAllianceStatus.REJECTED:
                    return "Rechazada";
                default:
                    return "";
            }
        }

        public async Task<bool> HasActiveElection()
        {
            bool active = await _electionRepository.GetAllQuery().AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            return active;
        }

        private async Task<bool> HasSharedCandidateAssignments(int partyA, int partyB)
        {
            return false;
        }
    }
}
