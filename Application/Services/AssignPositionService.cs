using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.AssignPosition;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class AssignPositionService : IAssignPositionService
    {
        private readonly IAssignPositionRepository _assignPositionRepository;
        
        private readonly ICandidateRepository _candidateRepository;

        private readonly IElectivePositionRepository _electivePositionRepository;


        private readonly IPoliticalAllienceRepository _allianceRepository;
        private readonly IPoliticalLeaderAssignmentRepository _leaderAssignmentRepository;

        private readonly IPoliticalPartyRepository _partyRepository;

        private readonly IElectionRepository _electionRepository;

        private readonly IVoteRepository _voteRepository;

        public AssignPositionService(
            IAssignPositionRepository assignPositionRepository,
            
            ICandidateRepository candidateRepository,

            IElectivePositionRepository electivePositionRepository,
            
            IPoliticalAllienceRepository politicalAllienceRepository,
            
            IPoliticalLeaderAssignmentRepository leaderAssignmentRepository,
            IPoliticalPartyRepository politicalPartyRepository,

            IElectionRepository electionRepository,
            IVoteRepository voteRepository
        ) 
        {
            _allianceRepository = politicalAllienceRepository;
            _assignPositionRepository = assignPositionRepository;
            _candidateRepository = candidateRepository;
            _electivePositionRepository = electivePositionRepository;
            _leaderAssignmentRepository = leaderAssignmentRepository;
            _partyRepository = politicalPartyRepository;
            _electionRepository = electionRepository;
            _voteRepository = voteRepository;
        }

        public async Task<Result<AssignPositionListDto>> GetAllAsync(int userId)
        {
            var result = new Result<AssignPositionListDto>();

            var assignment = await _leaderAssignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                result.IsSuccess = false;
                result.Message = "No tiene un partido político asignado.";
                return result;
            }

            int myPartyId = assignment.PoliticalPartyId;

            var assignPositions = await _assignPositionRepository
                .GetAllQuery()
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.PoliticalParty)
                .Include(a => a.ElectivePosition)
                .Where(a => a.PoliticalPartyId == myPartyId)
                .ToListAsync();

            var allDtos = assignPositions.Select(a => new AssignPositionDto
            {
                Id = a.Id,
                CandidateName = a.Candidate.Name,
                CandidateLastName = a.Candidate.LastName,
                CandidatePartyName = $"{a.Candidate.PoliticalParty.Name} ({a.Candidate.PoliticalParty.Acronym})",
                PositionName = a.ElectivePosition.Name,
                CandidateType = a.IsAlliance ? "Aliado" : "Propio",
                IsAlliance = a.IsAlliance,
                IsEditable = a.ElectionId == null
            }).ToList();

            var listDto = new AssignPositionListDto
            {
                CurrentAssignments = allDtos.Where(dto => assignPositions.Any(ap => ap.Id == dto.Id && ap.ElectionId == null)).ToList(),

                PastAssignments = allDtos.Where(dto => assignPositions.Any(ap => ap.Id == dto.Id && ap.ElectionId != null)).ToList()
            };

            result.IsSuccess = true;
            result.Data = listDto;

            return result;
        }

        public async Task<Result> CreateAsync(CreateAssignPositionDto dto, int userId)
        {
            try
            {
                if (await HasActiveElection())
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede asignar candidatos a puestos mientras exista una elección activa."
                    };
                }

                var assignment = await _leaderAssignmentRepository.GetAllQuery().FirstOrDefaultAsync(a => a.UserId == userId);

                if (assignment == null)
                    return new Result { IsSuccess = false, Message = "No tiene un partido político asignado." };

                int myPartyId = assignment.PoliticalPartyId;

                var myParty = await _partyRepository.GetById(myPartyId);

                if (myParty == null || !myParty.Status)
                    return new Result { IsSuccess = false, Message = "El partido político del dirigente está inactivo." };

                var candidate = await _candidateRepository
                    .GetAllQuery()
                    .Include(c => c.PoliticalParty)
                    .FirstOrDefaultAsync(c => c.Id == dto.CandidateId);

                if (candidate == null)
                    return new Result { IsSuccess = false, Message = "El candidato no existe." };

                if (!candidate.Status)
                    return new Result { IsSuccess = false, Message = "El candidato no está activo." };

                var position = await _electivePositionRepository.GetById(dto.ElectivePositionId);

                if (position == null)
                    return new Result { IsSuccess = false, Message = "El puesto no existe." };

                if (!position.Status)
                    return new Result { IsSuccess = false, Message = "El puesto no está activo." };

                bool positionUsed = await _assignPositionRepository
                    .GetAllQuery()
                    .AnyAsync(a =>
                        a.ElectionId == null &&
                        a.PoliticalPartyId == myPartyId &&
                        a.ElectivePositionId == dto.ElectivePositionId);

                if (positionUsed)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "Este puesto electivo ya tiene un candidato asignado dentro del partido."
                    };
                }

                bool candidateUsed = await _assignPositionRepository
                    .GetAllQuery()
                    .AnyAsync(a =>
                        a.ElectionId == null &&
                        a.PoliticalPartyId == myPartyId &&
                        a.CandidateId == dto.CandidateId);

                if (candidateUsed)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "Este candidato ya está asignado a un puesto dentro del partido."
                    };
                }

                bool isAlliance = false;

                if (candidate.PoliticalPartyId == myPartyId)
                {
                    isAlliance = false;
                }
                else
                {
                    bool allianceExists = await _allianceRepository
                        .GetAllQuery()
                        .AnyAsync(a =>
                            (
                                a.RequestingPartyId == myPartyId && a.ReceivingPartyId == candidate.PoliticalPartyId ||

                                a.RequestingPartyId == candidate.PoliticalPartyId && a.ReceivingPartyId == myPartyId
                            )
                            && a.Status == PoliticalAllianceStatus.ACCEPTED);

                    if (!allianceExists)
                    {
                        return new Result
                        {
                            IsSuccess = false,
                            Message = "No existe una alianza vigente con el partido de este candidato."
                        };
                    }

                    if (!candidate.PoliticalParty.Status)
                    {
                        return new Result
                        {
                            IsSuccess = false,
                            Message = "El partido de origen del candidato está inactivo."
                        };
                    }

                    var originAssign = await _assignPositionRepository
                        .GetAllQuery()
                        .FirstOrDefaultAsync(a =>
                            a.ElectionId == null &&
                            a.CandidateId == candidate.Id &&
                            a.PoliticalPartyId == candidate.PoliticalPartyId);

                    if (originAssign == null)
                    {
                        return new Result
                        {
                            IsSuccess = false,
                            Message = "Este candidato aliado no tiene un puesto asignado en su partido de origen."
                        };
                    }

                    if (originAssign.ElectivePositionId != dto.ElectivePositionId)
                    {
                        return new Result
                        {
                            IsSuccess = false,
                            Message = "Este candidato en su partido de origen aspira a un puesto diferente al seleccionado."
                        };
                    }

                    isAlliance = true;
                }

                AssignPosition entity = new()
                {
                    ElectionId = null,
                    CandidateId = dto.CandidateId,
                    ElectivePositionId = dto.ElectivePositionId,
                    PoliticalPartyId = myPartyId,
                    IsAlliance = isAlliance
                };

                var created = await _assignPositionRepository.AddAsync(entity);

                if (created == null)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se pudo crear la asignación."
                    };
                }

                return new Result
                {
                    IsSuccess = true,
                    Message = "Asignación creada correctamente."
                };
            }
            catch (Exception)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Error al crear la asignación."
                };
            }
        }

        public async Task<(List<DropdownDto> Candidates, List<DropdownDto> Positions)> GetDropdowns(int userId)
        {
            var assignment = await _leaderAssignmentRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
                return (new List<DropdownDto>(), new List<DropdownDto>());

            int myPartyId = assignment.PoliticalPartyId;

            var myAssignments = await _assignPositionRepository
                .GetAllQuery()
                .Where(a => a.PoliticalPartyId == myPartyId && a.ElectionId == null)
                .ToListAsync();

            var alliances = await _allianceRepository
                .GetAllQuery()
                .Where(a =>
                    a.Status == PoliticalAllianceStatus.ACCEPTED &&
                    (a.RequestingPartyId == myPartyId || a.ReceivingPartyId == myPartyId))
                .ToListAsync();

            var alliedPartyIds = alliances
                .Select(a =>
                    a.RequestingPartyId == myPartyId
                        ? a.ReceivingPartyId
                        : a.RequestingPartyId)
                .ToHashSet();

            var assignedCandidateIds = myAssignments
                .Select(a => a.CandidateId)
                .ToHashSet();


            var ownCandidates = await _candidateRepository
                .GetAllQuery()
                .Where(c =>
                    c.Status &&
                    c.PoliticalPartyId == myPartyId &&
                    !assignedCandidateIds.Contains(c.Id))
                .Select(c => new DropdownDto
                {
                    Id = c.Id,
                    Name = $"{c.Name} {c.LastName} (Propio)"
                })
                .ToListAsync();

            var alliedCandidates = await _candidateRepository
                .GetAllQuery()
                .Include(c => c.PoliticalParty)
                .Where(c =>
                    c.Status &&
                    alliedPartyIds.Contains(c.PoliticalPartyId) &&
                    c.PoliticalParty.Status &&
                    !assignedCandidateIds.Contains(c.Id))
                .ToListAsync();

            var validAlliedCandidates = new List<DropdownDto>();

            foreach (var candidate in alliedCandidates)
            {
                bool hasOriginAssign = await _assignPositionRepository
                    .GetAllQuery()
                    .AnyAsync(a =>
                        a.ElectionId == null &&
                        a.CandidateId == candidate.Id &&
                        a.PoliticalPartyId == candidate.PoliticalPartyId);

                if (hasOriginAssign)
                {
                    validAlliedCandidates.Add(new DropdownDto
                    {
                        Id = candidate.Id,
                        Name = $"{candidate.Name} {candidate.LastName} ({candidate.PoliticalParty.Acronym})"
                    });
                }
            }

            var candidates = ownCandidates
                .Concat(validAlliedCandidates)
                .ToList();

            var assignedPositionIds = myAssignments
                .Select(a => a.ElectivePositionId)
                .ToHashSet();

            var positions = await _electivePositionRepository
                .GetAllQuery()
                .Where(p =>
                    p.Status &&
                    !assignedPositionIds.Contains(p.Id))
                .Select(p => new DropdownDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return (candidates, positions);
        }

        public async Task<Result> DeleteAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar una asignación mientras exista una elección activa."
                };
            }

            var assignment = await _leaderAssignmentRepository.GetAllQuery().FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene un partido político asignado."
                };
            }

            int myPartyId = assignment.PoliticalPartyId;

            var entity = await _assignPositionRepository.GetAllQuery()
                .Include(a => a.Election)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La asignación seleccionada no existe o ya fue eliminada."
                };
            }

            if (entity.PoliticalPartyId != myPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para eliminar esta asignación."
                };
            }

            if (entity.Election != null && entity.Election.Status != ElectionStatus.PENDING)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Esta asignación no se puede eliminar porque pertenece a una elección que ya fue iniciada o finalizada, y representa un registro histórico inalterable."
                };
            }

            await _assignPositionRepository.DeleteAsync(entity.Id);

            return new Result
            {
                IsSuccess = true,
                Message = "Asignación eliminada correctamente."
            };
        }



        public async Task<bool> HasActiveElection()
        {
            bool active = await _electionRepository.GetAllQuery().AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            return active;
        }
    }
}
