using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Election;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class ElectionService : IElectionService
    {
        private readonly IElectivePositionRepository _electivePositionRepository;
        private readonly IPoliticalPartyRepository _partyRepository;
        private readonly IAssignPositionRepository _assignPositionRepository;
        private readonly IElectionRepository _electionRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly ICitizenRepository _citizenRepository;

        public ElectionService(
            IElectionRepository electionRepository,
            IAssignPositionRepository assignPositionRepository,
            IElectivePositionRepository electivePositionRepository,
            IPoliticalPartyRepository politicalPartyRepository,
            IVoteRepository voteRepository,
            ICitizenRepository citizenRepository
            )
        {
            _electionRepository = electionRepository;
            _assignPositionRepository = assignPositionRepository;
            _electivePositionRepository = electivePositionRepository;
            _partyRepository = politicalPartyRepository;
            _voteRepository = voteRepository;
            _citizenRepository = citizenRepository;
        }

        public async Task<Result> CreateAsync(CreateElectionDto dto)
        {
            bool hasActive = await _electionRepository
                .GetAllQuery()
                .AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            if (hasActive)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede crear una nueva elección mientras exista una elección activa."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return new Result { IsSuccess = false, Message = "El nombre de la elección es requerido." };
            }

            var positions = await _electivePositionRepository
                .GetAllQuery()
                .Where(p => p.Status)
                .ToListAsync();

            if (!positions.Any())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No hay puestos electivos activos para realizar una elección."
                };
            }

            var parties = await _partyRepository
                .GetAllQuery()
                .Where(p => p.Status)
                .ToListAsync();

            if (parties.Count < 2)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No hay suficientes partidos políticos para realizar una elección."
                };
            }

            var assignments = await _assignPositionRepository
                .GetAllQuery()
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.PoliticalParty)
                .Where(a => a.ElectionId == null)
                .ToListAsync();

            foreach (var party in parties)
            {
                var missingPositions = new List<string>();

                foreach (var position in positions)
                {
                    var assign = assignments.FirstOrDefault(a =>
                        a.PoliticalPartyId == party.Id &&
                        a.ElectivePositionId == position.Id);

                    if (assign == null ||
                        !assign.Candidate.Status ||
                        !assign.Candidate.PoliticalParty.Status)
                    {
                        missingPositions.Add(position.Name);
                    }
                }

                if (missingPositions.Any())
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = $"El partido político {party.Name} ({party.Acronym}) no tiene candidatos activos asignados para los siguientes puestos electivos: {string.Join(", ", missingPositions)}."
                    };
                }
            }

            Election entity = new()
            {
                Name = dto.Name.Trim(),
                Date = dto.Date,
                Status = ElectionStatus.PENDING,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _electionRepository.AddAsync(entity);

            if (created == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo crear la elección."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Elección creada correctamente."
            };
        }


        /*
        public async Task<Result<List<ElectionDto>>> GetAllAsync()
        {
            var result = new Result<List<ElectionDto>>();

            var elections = await _electionRepository
                .GetAllQuery()
                .OrderByDescending(e => e.Status == ElectionStatus.ACTIVE)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();

            var activeParties = await _partyRepository
                .GetAllQuery()
                .Where(p => p.Status)
                .CountAsync();

            var activePositions = await _electivePositionRepository
                .GetAllQuery()
                .Where(p => p.Status)
                .CountAsync();


            var votersCount = await _voteRepository
                .GetAllQuery()
                .CountAsync();


            result.IsSuccess = true;

            result.Data = elections.Select(e => new ElectionDto
            {
                Id = e.Id,
                Name = e.Name,
                Date = e.Date,
                StatusText = GetStatusText(e.Status),
                Status = e.Status,
                PartiesCount = activeParties,
                PositionsCount = activePositions,
                VotersCount = votersCount
            }).ToList();

            return result;
        }
        */

        public async Task<Result<List<ElectionDto>>> GetAllAsync()
        {
            var result = new Result<List<ElectionDto>>();

            var elections = await _electionRepository
                .GetAllQuery()
                .OrderByDescending(e => e.Status == ElectionStatus.ACTIVE)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();

            var globalActiveParties = await _partyRepository.GetAllQuery()
                .Where(p => p.Status == true)
                .CountAsync();

            var globalActivePositions = await _electivePositionRepository.GetAllQuery()
                .Where(p => p.Status == true)
                .CountAsync();

            var globalActiveCitizens = await _citizenRepository.GetAllQuery()
                .Where(c => c.Status == true)
                .CountAsync();

            var assignments = await _assignPositionRepository.GetAllQuery().ToListAsync();
            var votes = await _voteRepository.GetAllQuery().ToListAsync();

            var dtoPositionsList = new List<ElectionDto>();

            foreach (var e in elections)
            {
                int partiesCount = 0;
                int positionsCount = 0;
                int votersCount = 0;

                if (e.Status == ElectionStatus.PENDING)
                {
                    partiesCount = globalActiveParties;
                    positionsCount = globalActivePositions;
                    votersCount = globalActiveCitizens;
                }
                else
                {
                    var electionAssignments = assignments.Where(a => a.ElectionId == e.Id).ToList();

                    positionsCount = electionAssignments
                        .Select(a => a.ElectivePositionId)
                        .Distinct()
                        .Count();

                    partiesCount = electionAssignments
                        .Select(a => a.PoliticalPartyId)
                        .Distinct()
                        .Count();

                    votersCount = votes
                        .Where(v => v.ElectionId == e.Id)
                        .Select(v => v.CitizenId)
                        .Distinct()
                        .Count();
                }

                dtoPositionsList.Add(new ElectionDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Date = e.Date,
                    StatusText = GetStatusText(e.Status),
                    Status = e.Status,
                    PartiesCount = partiesCount,
                    PositionsCount = positionsCount,
                    VotersCount = votersCount
                });
            }

            result.IsSuccess = true;
            result.Data = dtoPositionsList;

            return result;
        }
        public async Task<Result> ActivateAsync(int id)
        {
            var election = await _electionRepository.GetById(id);

            if (election == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La elección no existe."
                };
            }

            if (election.Status != ElectionStatus.PENDING)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Solo se pueden activar elecciones en estado pendiente."
                };
            }

            bool existsActive = await _electionRepository
                .GetAllQuery()
                .AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            if (existsActive)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede activar esta elección porque ya existe una elección activa."
                };
            }

            var positions = await _electivePositionRepository
                .GetAllQuery()
                .Where(p => p.Status)
                .ToListAsync();

            if (!positions.Any())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No hay puestos electivos activos para activar esta elección."
                };
            }

            var parties = await _partyRepository
                .GetAllQuery()
                .Where(p => p.Status)
                .ToListAsync();

            if (parties.Count < 2)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No hay suficientes partidos políticos para activar esta elección."
                };
            }

            var assignments = await _assignPositionRepository
                .GetAllQuery()
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.PoliticalParty)
                .Include(a => a.ElectivePosition)
                .Where(a => a.ElectionId == null)
                .ToListAsync();

            foreach (var party in parties)
            {
                var missingPositions = new List<string>();

                foreach (var position in positions)
                {
                    var assign = assignments.FirstOrDefault(a =>
                        a.PoliticalPartyId == party.Id &&
                        a.ElectivePositionId == position.Id);

                    if (assign == null ||
                        !assign.Candidate.Status ||
                        !assign.Candidate.PoliticalParty.Status ||
                        !assign.ElectivePosition.Status)
                    {
                        missingPositions.Add(position.Name);
                    }
                }

                if (missingPositions.Any())
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = $"El partido político {party.Name} ({party.Acronym}) no tiene candidatos activos asignados para los siguientes puestos electivos: {string.Join(", ", missingPositions)}."
                    };
                }
            }

            if (!assignments.Any())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede activar la elección porque ningún partido político ha configurado candidatos en sus boletas actuales."
                };
            }

            foreach (var assignment in assignments)
            {
                assignment.ElectionId = election.Id;
                await _assignPositionRepository.UpdateAsync(assignment.Id, assignment);
            }

            election.Status = ElectionStatus.ACTIVE;
            election.ActivatedDate = DateTime.UtcNow;

            var updated = await _electionRepository.UpdateAsync(election.Id, election);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo activar la elección."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Elección activada correctamente."
            };
        }

        public async Task<Result> FinishAsync(int id)
        {
            var election = await _electionRepository.GetById(id);

            if (election == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La elección no existe."
                };
            }

            if (election.Status == ElectionStatus.FINISHED)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Esta elección ya se encuentra finalizada."
                };
            }

            if (election.Status != ElectionStatus.ACTIVE)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Solo se pueden finalizar elecciones activas."
                };
            }

            election.Status = ElectionStatus.FINISHED;
            election.FinishedDate = DateTime.UtcNow;

            var updated = await _electionRepository.UpdateAsync(election.Id, election);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo finalizar la elección."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Elección finalizada correctamente."
            };
        }

        public async Task<Result<ElectionResultDto>> GetResultsByElectionAsync(int electionId)
        {
            var result = new Result<ElectionResultDto>();

            try
            {
                var election = await _electionRepository.GetById(electionId);
                if (election == null)
                {
                    result.IsSuccess = false;
                    result.Message = "La elección especificada no existe.";
                    return result;
                }

                var allVotes = await _voteRepository.GetAllQuery()
                    .Include(v => v.Candidate)
                        .ThenInclude(c => c.PoliticalParty)
                    .Include(v => v.ElectivePosition)
                    .Where(v => v.ElectionId == electionId)
                    .ToListAsync();

                var assignments = await _assignPositionRepository.GetAllQuery()
                    .Include(a => a.Candidate)
                        .ThenInclude(c => c.PoliticalParty)
                    .Include(a => a.ElectivePosition)
                    .Where(a => a.ElectionId == electionId)
                    .ToListAsync();

                var electionResultDto = new ElectionResultDto
                {
                    ElectionId = election.Id,
                    ElectionName = election.Name,
                    Positions = new List<PositionResultSummaryDto>()
                };

                var positionsDisputed = assignments.Select(a => a.ElectivePosition)
                    .Concat(allVotes.Select(v => v.ElectivePosition))
                    .Where(p => p != null)
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .ToList();

                foreach (var position in positionsDisputed)
                {
                    var positionVotes = allVotes.Where(v => v.ElectivePositionId == position.Id).ToList();
                    int totalVotesInPosition = positionVotes.Count;

                    var candidateResults = new List<CandidateResultDto>();

                    var positionAssignments = assignments.Where(a => a.ElectivePositionId == position.Id).ToList();
                    foreach (var assign in positionAssignments)
                    {
                        int candidateVotes = positionVotes.Count(v => v.CandidateId == assign.CandidateId && !v.IsBlank);
                        decimal percentage = totalVotesInPosition > 0 ? Math.Round(((decimal)candidateVotes / totalVotesInPosition) * 100, 2) : 0;

                        candidateResults.Add(new CandidateResultDto
                        {
                            CandidateId = assign.CandidateId,
                            CandidateFullName = $"{assign.Candidate.Name} {assign.Candidate.LastName}",
                            PoliticalPartyName = assign.PoliticalParty?.Name,
                            VotesCount = candidateVotes,
                            Percentage = percentage,
                            IsWinner = false
                        });
                    }

                    int blankVotes = positionVotes.Count(v => v.IsBlank);
                    decimal blankPercentage = totalVotesInPosition > 0
                        ? Math.Round(((decimal)blankVotes / totalVotesInPosition) * 100, 2)
                        : 0;

                    candidateResults.Add(new CandidateResultDto
                    {
                        CandidateId = null,
                        CandidateFullName = "Ninguno",
                        PoliticalPartyName = "No aplica",
                        VotesCount = blankVotes,
                        Percentage = blankPercentage,
                        IsWinner = false
                    });

                    candidateResults = candidateResults.OrderByDescending(r => r.VotesCount).ToList();

                    bool isTie = false;
                    if (totalVotesInPosition > 0)
                    {
                        int maxVotes = candidateResults.First().VotesCount;

                        var topContenders = candidateResults.Where(r => r.VotesCount == maxVotes).ToList();

                        if (topContenders.Count > 1)
                        {
                            isTie = true;
                        }
                        else
                        {
                            var winner = candidateResults.First();
                            if (winner.CandidateId != null)
                            {
                                winner.IsWinner = true;
                            }
                        }
                    }

                    electionResultDto.Positions.Add(new PositionResultSummaryDto
                    {
                        PositionId = position.Id,
                        PositionName = position.Name,
                        TotalVotesInPosition = totalVotesInPosition,
                        IsTie = isTie,
                        Results = candidateResults
                    });
                }

                result.IsSuccess = true;
                result.Data = electionResultDto;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado al procesar los resultados de la elección.";
            }

            return result;
        }


        private string GetStatusText(ElectionStatus status)
        {
            return status switch
            {
                ElectionStatus.PENDING => "Pendiente",
                ElectionStatus.ACTIVE => "Activa",
                ElectionStatus.FINISHED => "Finalizada",
                _ => "Desconocido"
            };
        }

        public async Task<bool> HasActiveElection()
        {
            bool active = await _electionRepository.GetAllQuery().AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            return active;
        }
    }
}
