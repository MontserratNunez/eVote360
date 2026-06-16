using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.LeaderAssignment;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    internal class PoliticalLeaderAssignmentService : IPoliticalLeaderAssignmentService
    {
        private readonly IPoliticalLeaderAssignmentRepository _assignmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPoliticalPartyRepository _partyRepository;
        private readonly IElectionRepository _electionRepository;
        private readonly IAssignPositionRepository _assignPositionRepository;
        private readonly IPoliticalAllienceRepository _allianceRepository;
        public PoliticalLeaderAssignmentService(
            IPoliticalLeaderAssignmentRepository politicalLeaderAssignmentRepository, 
            IUserRepository userRepository, 
            IPoliticalPartyRepository politicalPartyRepository, 
            IElectionRepository election,
            IAssignPositionRepository assignPositionRepository,
            IPoliticalAllienceRepository politicalAllienceRepository
            )
        {
            _assignmentRepository = politicalLeaderAssignmentRepository;
            _userRepository = userRepository;
            _partyRepository = politicalPartyRepository;
            _electionRepository = election;
            _assignPositionRepository = assignPositionRepository;
            _allianceRepository = politicalAllienceRepository;
        }

        public async Task<Result<List<LeaderAssignmentDto>>> GetAllAsync()
        {
            var result = new Result<List<LeaderAssignmentDto>>();

            try
            {
                var assignments = await _assignmentRepository
                    .GetAllQueryWithInclude(new List<string> { "User", "PoliticalParty" })
                    .ToListAsync();

                if (assignments == null || !assignments.Any())
                {
                    result.IsSuccess = true;
                    result.Data = new List<LeaderAssignmentDto>();
                    return result;
                }

                result.IsSuccess = true;
                result.Data = assignments.Select(a => new LeaderAssignmentDto
                {
                    Id = a.Id,
                    FullName = $"{a.User.Name} {a.User.LastName}",
                    UserName = a.User.UserName,
                    PartyName = a.PoliticalParty.Name,
                    PartyAcronym = a.PoliticalParty.Acronym,
                    UserStatus = a.User.Status,
                    PartyStatus = a.PoliticalParty.Status
                }).ToList();
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error listando asignaciones de dirigentes";
            }

            return result;
        }

        public async Task<Result> CreateAsync(CreateAssignmentDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede crear una asignación de dirigente político mientras exista\r\nuna elección activa."
                };
            }


            var user = await _userRepository.GetAllQueryWithInclude(["PoliticalLeaderAssignment"])
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
                return new Result { IsSuccess = false, Message = "El usuario no existe." };

            if (!user.Status)
                return new Result { IsSuccess = false, Message = "El usuario no es válido para asignación." };

            if (user.Role != Role.DIRECTOR)
                return new Result { IsSuccess = false, Message = "El usuario seleccionado no tiene el rol de dirigente político." };

            if (user.PoliticalLeaderAssignment != null)
                return new Result { IsSuccess = false, Message = "Este dirigente ya tiene un partido asignado." };


            var party = await _partyRepository.GetAllQueryWithInclude(["PoliticalLeaderAssignment"])
               .FirstOrDefaultAsync(p => p.Id == dto.PoliticalPartyId);

            if (party == null)
                return new Result { IsSuccess = false, Message = "El partido político no existe." };

            if (!party.Status)
                return new Result { IsSuccess = false, Message = "El partido político está inactivo." };

            if (party.PoliticalLeaderAssignment != null)
                return new Result { IsSuccess = false, Message = "Este partido ya tiene un dirigente asignado." };

            PoliticalLeaderAssignment entity = new()
            {
                UserId = dto.UserId,
                PoliticalPartyId = dto.PoliticalPartyId
            };

            var created = await _assignmentRepository.AddAsync(entity);

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

        public async Task<(List<DropdownDto> Users, List<DropdownDto> Parties)> GetDropdowns()
        {
            try
            {
                var users = await _userRepository.GetAllQueryWithInclude(["PoliticalLeaderAssignment"])
                        .Where(u =>
                            u.Status &&
                            u.Role == Role.DIRECTOR &&
                            u.PoliticalLeaderAssignment == null)
                        .Select(u => new DropdownDto
                        {
                            Id = u.Id,
                            Name = $"{u.Name} {u.LastName} - {u.UserName}"
                        })
                        .ToListAsync();

                var parties = await _partyRepository.GetAllQueryWithInclude(["PoliticalLeaderAssignment"])
                    .Where(p =>
                        p.Status &&
                        p.PoliticalLeaderAssignment == null)
                    .Select(p => new DropdownDto
                    {
                        Id = p.Id,
                        Name = $"{p.Name} - {p.Acronym}"
                    })
                    .ToListAsync();

                return (users, parties);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar una asignación de dirigente político mientras exista una elección activa."
                };
            }

            var entity = await _assignmentRepository.GetById(id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "La asignación seleccionada no existe o ya fue eliminada."
                };
            }

            if (await HasActiveDependencies(entity.UserId, entity.PoliticalPartyId))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede eliminar esta asignación porque está siendo utilizada en una operación activa."
                };
            }

            await _assignmentRepository.DeleteAsync(entity.Id);

            return new Result
            {
                IsSuccess = true,
                Message = "Asignación eliminada correctamente."
            };
        }

        private async Task<bool> HasActiveDependencies(int userId, int partyId)
        {
            bool hasCurrentAssignments = await _assignPositionRepository.GetAllQuery().AnyAsync(a => a.PoliticalPartyId == partyId && a.ElectionId == null);

            if (hasCurrentAssignments)
            {
                return true;
            }

            bool hasActiveAlliances = await _allianceRepository.GetAllQuery()
            .AnyAsync(a =>
                (a.RequestingPartyId == partyId || a.ReceivingPartyId == partyId) &&
                (a.Status == PoliticalAllianceStatus.PENDING || a.Status == PoliticalAllianceStatus.ACCEPTED));

            if (hasActiveAlliances)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> HasActiveElection()
        {
            bool active = await _electionRepository.GetAllQuery().AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            return active;
        }
    }
}
