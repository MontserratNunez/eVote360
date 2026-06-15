using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Candidate;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IPoliticalLeaderAssignmentRepository _assignmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPoliticalPartyRepository _politicalPartyRepository;
        private readonly IElectionRepository _electionRepository;
        private readonly IAssignPositionRepository _assignPositionRepository;

        public CandidateService(ICandidateRepository candidateRepository,
            IPoliticalLeaderAssignmentRepository assignmentRepository, IUserRepository userRepository,
            IPoliticalPartyRepository politicalPartyRepository, 
            IElectionRepository electionRepository,
            IAssignPositionRepository assignPositionRepository
            )
        {
            _candidateRepository = candidateRepository;
            _assignmentRepository = assignmentRepository;
            _userRepository = userRepository;
            _politicalPartyRepository = politicalPartyRepository;
            _electionRepository = electionRepository;
            _assignPositionRepository = assignPositionRepository;
        }

        public async Task<Result<List<CandidateDto>>> GetAllAsync(int userId)
        {
            var result = new Result<List<CandidateDto>>();

            try
            {
                var assignment = await _assignmentRepository.GetAllQuery()
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (assignment == null)
                {
                    result.IsSuccess = false;
                    result.Message = "El dirigente no tiene un partido asignado.";
                    return result;
                }

                int partyId = assignment.PoliticalPartyId;

                var candidates = await _candidateRepository.GetAllQueryWithInclude(["AssignPositions.ElectivePosition"]).AsNoTracking()
                    .Where(c => c.PoliticalPartyId == partyId || 
                    c.AssignPositions.Any(ap => ap.PoliticalPartyId == partyId && ap.ElectionId == null))
                    .ToListAsync();


                result.IsSuccess = true;

                result.Data = candidates.Select(c => {
                    var partyAssignment = c.AssignPositions.FirstOrDefault(ap => ap.PoliticalPartyId == partyId && ap.ElectionId == null);

                    return new CandidateDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        LastName = c.LastName,
                        PhotoPath = c.PhotoPath,
                        PositionName = partyAssignment?.ElectivePosition?.Name ?? null,
                        Status = c.Status
                    };
                }).ToList();
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al listar candidatos.";
            }
            
            return result;
        }

        public async Task<Result<CandidateDto>> CreateAsync(CreateCandidateDto dto, int userId)
        {
            

            if (await HasActiveElection())
            {
                return new Result<CandidateDto>
                {
                    IsSuccess = false,
                    Message = "No se puede crear un candidato mientras exista una elección activa."
                };
            }

            var user = await _userRepository.GetById(userId);

            if (user == null || user.Role != Role.DIRECTOR)
            {
                return new Result<CandidateDto>
                {
                    IsSuccess = false,
                    Message = "El usuario no es válido para crear candidatos."
                };
            }

            var assignment = await _assignmentRepository.GetAllQuery().FirstOrDefaultAsync(a => a.UserId == userId);

            if (assignment == null)
            {
                return new Result<CandidateDto>
                {
                    IsSuccess = false,
                    Message = "No puede crear candidatos porque no tiene un partido político asignado."
                };
            }

            var party = await _politicalPartyRepository.GetById(assignment.PoliticalPartyId);

            if (party == null || !party.Status)
            {
                return new Result<CandidateDto>
                {
                    IsSuccess = false,
                    Message = "No puede crear candidatos porque el partido político asignado se encuentra inactivo."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result<CandidateDto> { IsSuccess = false, Message = "El nombre del candidato es requerido." };

            if (string.IsNullOrWhiteSpace(dto.LastName))
                return new Result<CandidateDto> { IsSuccess = false, Message = "El apellido del candidato es requerido." };

            Candidate entity = new()
            {
                Name = dto.Name.Trim(),
                LastName = dto.LastName.Trim(),
                PhotoPath = dto.PhotoPath,
                PoliticalPartyId = assignment.PoliticalPartyId,
                Status = true
            };

            var created = await _candidateRepository.AddAsync(entity);


            if (created == null)
            {
                return new Result<CandidateDto>
                {
                    IsSuccess = false,
                    Message = "No se pudo crear el candidato."
                };
            }

            return new Result<CandidateDto>
            {
                IsSuccess = true,
                Message = "Candidato creado correctamente.",
                Data = new CandidateDto()
                {
                    Id = created.Id,
                    Name = created.Name,
                    LastName = created.LastName,
                    PhotoPath = created.PhotoPath,
                }
            };
        }

        public async Task<Result<CandidateDto>> GetById(int id, int userId)
        {
            var result = new Result<CandidateDto>();

            try
            {
                var user = await _userRepository.GetById(userId);

                if (user == null || user.Role != Role.DIRECTOR)
                {
                    result.IsSuccess = false;
                    result.Message = "El usuario no es válido.";
                    return result;
                }

                var assignment = await _assignmentRepository
                    .GetAllQuery()
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (assignment == null)
                {
                    result.IsSuccess = false;
                    result.Message = "No tiene un partido político asignado.";
                    return result;
                }

                var entity = await _candidateRepository.GetById(id);

                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Message = "El candidato no existe.";
                    return result;
                }

                if (entity.PoliticalPartyId != assignment.PoliticalPartyId)
                {
                    result.IsSuccess = false;
                    result.Message = "No tiene permisos para acceder a este candidato.";
                    return result;
                }

                result.IsSuccess = true;
                result.Data = new CandidateDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    LastName = entity.LastName,
                    PhotoPath = entity.PhotoPath,
                    Status = entity.Status
                };
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener candidato";
            }

            return result;
        }

        public async Task<Result> UpdateAsync(UpdateCandidateDto dto, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede editar un candidato mientras exista una elección activa."
                };
            }

            var user = await _userRepository.GetById(userId);

            if (user == null || user.Role != Role.DIRECTOR)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario no es válido."
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

            var entity = await _candidateRepository.GetById(dto.Id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El candidato no existe."
                };
            }

            if (entity.PoliticalPartyId != assignment.PoliticalPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para modificar este candidato."
                };
            }

            var party = await _politicalPartyRepository.GetById(assignment.PoliticalPartyId);

            if (party == null || !party.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político asignado se encuentra inactivo."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result { IsSuccess = false, Message = "El nombre del candidato es requerido." };

            if (string.IsNullOrWhiteSpace(dto.LastName))
                return new Result { IsSuccess = false, Message = "El apellido del candidato es requerido." };

            if (await HasCandidateParticipated(entity.Id))
            {
                if (entity.Name != dto.Name.Trim() ||
                    entity.LastName != dto.LastName.Trim() ||
                    (!string.IsNullOrWhiteSpace(dto.PhotoPath)))
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se pueden modificar los datos principales de este candidato porque ya participó en una elección."
                    };
                }
            }

            entity.Name = dto.Name.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.PhotoPath))
            {
                entity.PhotoPath = dto.PhotoPath;
            }

            var updated = await _candidateRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudieron guardar los cambios."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Candidato actualizado correctamente."
            };
        }

        public async Task<Result> ActivateAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede activar un candidato mientras exista una elección activa."
                };
            }

            var user = await _userRepository.GetById(userId);

            if (user == null || user.Role != Role.DIRECTOR)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario no es válido."
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

            var entity = await _candidateRepository.GetById(id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El candidato no existe."
                };
            }

            if (entity.PoliticalPartyId != assignment.PoliticalPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para activar este candidato."
                };
            }

            if (entity.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este candidato ya se encuentra activo."
                };
            }

            var party = await _politicalPartyRepository.GetById(entity.PoliticalPartyId);

            if (party == null || !party.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede activar este candidato porque su partido político se encuentra inactivo."
                };
            }

            entity.Status = true;

            var updated = await _candidateRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo activar el candidato."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Candidato activado correctamente."
            };
        }

        public async Task<Result> DeactivateAsync(int id, int userId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar un candidato mientras exista una elección activa."
                };
            }

            var user = await _userRepository.GetById(userId);

            if (user == null || user.Role != Role.DIRECTOR)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario no es válido."
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

            var entity = await _candidateRepository.GetById(id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El candidato no existe."
                };
            }

            if (entity.PoliticalPartyId != assignment.PoliticalPartyId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No tiene permisos para desactivar este candidato."
                };
            }

            if (!entity.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este candidato ya se encuentra inactivo."
                };
            }

            if (await HasActivePositionAssignment(entity.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar este candidato porque está asignado a un puesto electivo."
                };
            }

            entity.Status = false;

            var updated = await _candidateRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo desactivar el candidato."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Candidato desactivado correctamente."
            };
        }

        public async Task UpdatePhoto(int id, string photoPath)
        {
            var entity = await _candidateRepository.GetById(id);

            if (entity != null)
            {
                entity.PhotoPath = photoPath;
                await _candidateRepository.UpdateAsync(entity.Id, entity);
            }
        }

        public async Task<bool> HasActiveElection()
        {
            bool active = await _electionRepository.GetAllQuery().AnyAsync(e => e.Status == ElectionStatus.ACTIVE);

            return active;
        }

        public async Task<bool> HasCandidateParticipated(int candidateId)
        {
            return await _assignPositionRepository.GetAllQuery().AnyAsync(a => a.CandidateId == candidateId && a.ElectionId != null);
        }

        private async Task<bool> HasActivePositionAssignment(int candidateId)
        {
            return await _assignPositionRepository.GetAllQuery().AnyAsync(a => a.CandidateId == candidateId && a.ElectionId == null);
        }
    }
}
