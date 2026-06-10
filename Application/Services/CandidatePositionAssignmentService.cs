using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.CandidatePositionAssignment;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.CandidatePositionAssignment;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;

namespace eVote360.Core.Application.Services
{
    public class CandidatePositionAssignmentService : ICandidatePositionAssignmentService
    {
        private readonly ICandidatePositionAssignmentRepository _candidatePositionAssignmentRepository;
        private readonly IGenericRepository<Candidate> _candidateRepository;
        private readonly IGenericRepository<ElectivePosition> _electivePositionRepository;
        private readonly IGenericRepository<PoliticalParty> _politicalPartyRepository;
        private readonly IGenericRepository<PoliticalAlliance> _politicalAllianceRepository;

        public CandidatePositionAssignmentService(
            ICandidatePositionAssignmentRepository candidatePositionAssignmentRepository,
            IGenericRepository<Candidate> candidateRepository,
            IGenericRepository<ElectivePosition> electivePositionRepository,
            IGenericRepository<PoliticalParty> politicalPartyRepository,
            IGenericRepository<PoliticalAlliance> politicalAllianceRepository)
        {
            _candidatePositionAssignmentRepository = candidatePositionAssignmentRepository;
            _candidateRepository = candidateRepository;
            _electivePositionRepository = electivePositionRepository;
            _politicalPartyRepository = politicalPartyRepository;
            _politicalAllianceRepository = politicalAllianceRepository;
        }

        public async Task<Result<List<CandidatePositionAssignmentDto>>> GetAllByPoliticalPartyAsync(int politicalPartyId)
        {
            var assignments = await _candidatePositionAssignmentRepository.GetAllList();
            var candidates = await _candidateRepository.GetAllList();
            var positions = await _electivePositionRepository.GetAllList();
            var parties = await _politicalPartyRepository.GetAllList();

            var result = assignments
                .Where(a => a.PoliticalPartyId == politicalPartyId)
                .Select(a =>
                {
                    var candidate = candidates.FirstOrDefault(c => c.Id == a.CandidateId);
                    var position = positions.FirstOrDefault(p => p.Id == a.ElectivePositionId);
                    var party = parties.FirstOrDefault(p => p.Id == a.PoliticalPartyId);

                    var candidateParty = candidate is null
                        ? null
                        : parties.FirstOrDefault(p => p.Id == candidate.PoliticalPartyId);

                    return new CandidatePositionAssignmentDto
                    {
                        Id = a.Id,
                        CandidateId = a.CandidateId,
                        CandidateName = candidate?.Name ?? string.Empty,
                        CandidateLastName = candidate?.LastName ?? string.Empty,
                        CandidatePoliticalPartyId = candidate?.PoliticalPartyId ?? 0,
                        CandidatePoliticalPartyName = candidateParty?.Name ?? string.Empty,
                        ElectivePositionId = a.ElectivePositionId,
                        ElectivePositionName = position?.Name ?? string.Empty,
                        PoliticalPartyId = a.PoliticalPartyId,
                        PoliticalPartyName = party?.Name ?? string.Empty,
                        IsAlliedCandidate = a.IsAlliedCandidate
                    };
                })
                .ToList();

            return new Result<List<CandidatePositionAssignmentDto>>
            {
                IsSuccess = true,
                Message = "Asignaciones obtenidas correctamente.",
                Data = result
            };
        }

        public async Task<Result<CreateCandidatePositionAssignmentViewModel>> GetCreateViewModelAsync(int politicalPartyId)
        {
            var candidates = await _candidateRepository.GetAllList();
            var positions = await _electivePositionRepository.GetAllList();
            var parties = await _politicalPartyRepository.GetAllList();
            var alliances = await _politicalAllianceRepository.GetAllList();
            var assignments = await _candidatePositionAssignmentRepository.GetAllList();

            var ownCandidates = candidates
                .Where(c => c.Status && c.PoliticalPartyId == politicalPartyId)
                .ToList();

            var alliedPartyIds = alliances
                .Where(a =>
                    a.Status == "Accepted" &&
                    (a.RequestingPoliticalPartyId == politicalPartyId ||
                     a.RequestedPoliticalPartyId == politicalPartyId))
                .Select(a =>
                    a.RequestingPoliticalPartyId == politicalPartyId
                        ? a.RequestedPoliticalPartyId
                        : a.RequestingPoliticalPartyId)
                .Distinct()
                .ToList();

            var alliedCandidates = candidates
                .Where(c =>
                    c.Status &&
                    alliedPartyIds.Contains(c.PoliticalPartyId) &&
                    assignments.Any(a =>
                        a.PoliticalPartyId == c.PoliticalPartyId &&
                        a.CandidateId == c.Id))
                .ToList();

            var allCandidateOptions = ownCandidates
                .Concat(alliedCandidates)
                .DistinctBy(c => c.Id)
                .Select(c =>
                {
                    var party = parties.FirstOrDefault(p => p.Id == c.PoliticalPartyId);

                    return new CandidateAssignmentOptionViewModel
                    {
                        Id = c.Id,
                        FullName = $"{c.Name} {c.LastName}",
                        PoliticalPartyName = party?.Name ?? string.Empty,
                        IsAlliedCandidate = c.PoliticalPartyId != politicalPartyId
                    };
                })
                .ToList();

            var positionOptions = positions
                .Where(p => p.Status)
                .Select(p => new ElectivePositionAssignmentOptionViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToList();

            var vm = new CreateCandidatePositionAssignmentViewModel
            {
                PoliticalPartyId = politicalPartyId,
                Candidates = allCandidateOptions,
                ElectivePositions = positionOptions
            };

            return new Result<CreateCandidatePositionAssignmentViewModel>
            {
                IsSuccess = true,
                Message = "Formulario cargado correctamente.",
                Data = vm
            };
        }

        public async Task<Result> CreateAsync(CreateCandidatePositionAssignmentDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede asignar candidatos a puestos mientras exista una elección activa."
                };
            }

            var candidate = await _candidateRepository.GetById(dto.CandidateId);
            if (candidate is null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El candidato seleccionado no existe."
                };
            }

            if (!candidate.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El candidato seleccionado se encuentra inactivo."
                };
            }

            var position = await _electivePositionRepository.GetById(dto.ElectivePositionId);
            if (position is null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El puesto electivo seleccionado no existe."
                };
            }

            if (!position.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El puesto electivo seleccionado se encuentra inactivo."
                };
            }

            var politicalParty = await _politicalPartyRepository.GetById(dto.PoliticalPartyId);
            if (politicalParty is null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político del dirigente no existe."
                };
            }

            if (!politicalParty.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político del dirigente se encuentra inactivo."
                };
            }

            var assignments = await _candidatePositionAssignmentRepository.GetAllList();

            var partyAlreadyHasPosition = assignments.Any(a =>
                a.PoliticalPartyId == dto.PoliticalPartyId &&
                a.ElectivePositionId == dto.ElectivePositionId);

            if (partyAlreadyHasPosition)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este puesto electivo ya tiene un candidato asignado dentro del partido."
                };
            }

            var candidateAlreadyAssignedInParty = assignments.Any(a =>
                a.PoliticalPartyId == dto.PoliticalPartyId &&
                a.CandidateId == dto.CandidateId);

            if (candidateAlreadyAssignedInParty)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este candidato ya está asignado a un puesto dentro del partido."
                };
            }

            var isOwnCandidate = candidate.PoliticalPartyId == dto.PoliticalPartyId;
            var isAlliedCandidate = !isOwnCandidate;

            if (isAlliedCandidate)
            {
                var alliances = await _politicalAllianceRepository.GetAllList();

                var hasAcceptedAlliance = alliances.Any(a =>
                    a.Status == "Accepted" &&
                    (
                        (a.RequestingPoliticalPartyId == dto.PoliticalPartyId &&
                         a.RequestedPoliticalPartyId == candidate.PoliticalPartyId)
                        ||
                        (a.RequestingPoliticalPartyId == candidate.PoliticalPartyId &&
                         a.RequestedPoliticalPartyId == dto.PoliticalPartyId)
                    ));

                if (!hasAcceptedAlliance)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No existe una alianza vigente con el partido de este candidato."
                    };
                }

                var originAssignment = assignments.FirstOrDefault(a =>
                    a.PoliticalPartyId == candidate.PoliticalPartyId &&
                    a.CandidateId == candidate.Id);

                if (originAssignment is null)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "Este candidato aliado no tiene un puesto asignado en su partido de origen."
                    };
                }

                if (originAssignment.ElectivePositionId != dto.ElectivePositionId)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "Este candidato en su partido de origen aspira a un puesto diferente al seleccionado."
                    };
                }
            }

            var assignment = new CandidatePositionAssignment
            {
                CandidateId = dto.CandidateId,
                ElectivePositionId = dto.ElectivePositionId,
                PoliticalPartyId = dto.PoliticalPartyId,
                IsAlliedCandidate = isAlliedCandidate
            };

            await _candidatePositionAssignmentRepository.AddAsync(assignment);

            return new Result
            {
                IsSuccess = true,
                Message = "Asignación creada correctamente."
            };
        }

        public async Task<Result<DeleteCandidatePositionAssignmentDto?>> GetDeleteInfoAsync(int id, int politicalPartyId)
        {
            var assignment = await _candidatePositionAssignmentRepository.GetById(id);

            if (assignment is null || assignment.PoliticalPartyId != politicalPartyId)
            {
                return new Result<DeleteCandidatePositionAssignmentDto?>
                {
                    IsSuccess = false,
                    Message = "La asignación seleccionada no existe o no tiene permisos para consultarla.",
                    Data = null
                };
            }

            var candidate = await _candidateRepository.GetById(assignment.CandidateId);
            var position = await _electivePositionRepository.GetById(assignment.ElectivePositionId);
            var party = await _politicalPartyRepository.GetById(assignment.PoliticalPartyId);

            var dto = new DeleteCandidatePositionAssignmentDto
            {
                Id = assignment.Id,
                CandidateFullName = $"{candidate?.Name} {candidate?.LastName}".Trim(),
                ElectivePositionName = position?.Name ?? string.Empty,
                PoliticalPartyName = party?.Name ?? string.Empty
            };

            return new Result<DeleteCandidatePositionAssignmentDto?>
            {
                IsSuccess = true,
                Message = "Información de eliminación obtenida correctamente.",
                Data = dto
            };
        }

        public async Task<Result> DeleteAsync(int id, int politicalPartyId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar una asignación mientras exista una elección activa."
                };
            }

            var assignment = await _candidatePositionAssignmentRepository.GetById(id);

            if (assignment is null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La asignación seleccionada no existe o ya fue eliminada."
                };
            }

            if (assignment.PoliticalPartyId != politicalPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para eliminar esta asignación."
                };
            }

            await _candidatePositionAssignmentRepository.DeleteAsync(id);

            return new Result
            {
                IsSuccess = true,
                Message = "Asignación eliminada correctamente."
            };
        }

        public async Task<bool> HasActiveElection()
        {
            await Task.CompletedTask;

            // Temporal hasta que el equipo cree o conecte la entidad Election.
            return false;
        }
    }
}