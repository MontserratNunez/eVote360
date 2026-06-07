using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.PoliticalParty;
using eVote360.Core.Application.Dtos.User;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class PoliticalPartyService : IPoliticalPartyService
    {
        private readonly IPoliticalPartyRepository _politicalPartyRepository;

        public PoliticalPartyService(IPoliticalPartyRepository politicalPartyRepository)
        {
            _politicalPartyRepository = politicalPartyRepository;
        }

        public async Task<Result<List<PoliticalPartyDto>>> GetAllAsync()
        {
            Result<List<PoliticalPartyDto>> result = new();
            try
            {
                var entities = await _politicalPartyRepository.GetAllList();

                var entitiesDto = entities.Select(p => new PoliticalPartyDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Acronym = p.Acronym,
                    LogoPath = p.LogoPath,
                    Status = p.Status
                }).ToList();

                result.IsSuccess = true;
                result.Data = entitiesDto;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al cargar partidos politicos";
            }

            return result;
        }

        public async Task<Result<PoliticalPartyDto?>> CreateAsync(CreatePoliticalPartyDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result<PoliticalPartyDto?>
                {
                    IsSuccess = false,
                    Message = "No se puede crear un partido político mientras exista una elección activa."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result<PoliticalPartyDto?> { IsSuccess = false, Message = "El nombre del partido es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Acronym))
                return new Result<PoliticalPartyDto?> { IsSuccess = false, Message = "Las siglas son requeridas." };

            string acronym = dto.Acronym.Trim().ToUpper();

            if (await _politicalPartyRepository
                .GetAllQuery()
                .AnyAsync(p => p.Acronym.ToLower() == acronym.ToLower()))
            {
                return new Result<PoliticalPartyDto?>
                {
                    IsSuccess = false,
                    Message = "Ya existe un partido político registrado con estas siglas."
                };
            }

            PoliticalParty entity = new()
            {
                Name = dto.Name.Trim(),
                Description = dto.Description ?? "",
                Acronym = acronym,
                LogoPath = dto.LogoPath ?? "",
                Status = true
            };

            var created = await _politicalPartyRepository.AddAsync(entity);

            if (created == null)
            {
                return new Result<PoliticalPartyDto?>
                {
                    IsSuccess = false,
                    Message = "No se pudo crear el partido político."
                };
            }

            return new Result<PoliticalPartyDto?>
            {
                Data = new PoliticalPartyDto()
                {
                    Id = created.Id,
                    Name = created.Name,
                    Description = created.Description,
                    Acronym = created.Acronym,
                    LogoPath = created.LogoPath,
                    Status = created.Status
                },
                IsSuccess = true,
                Message = "Partido político creado correctamente."
            };
        }

        public async Task<Result> UpdateAsync(UpdatePoliticalPartyDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede editar un partido político mientras exista una elección activa."
                };
            }

            var entity = await _politicalPartyRepository.GetById(dto.Id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político no existe."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result { IsSuccess = false, Message = "El nombre del partido es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Acronym))
                return new Result { IsSuccess = false, Message = "Las siglas son requeridas." };

            string acronym = dto.Acronym.Trim().ToUpper();

            if (await _politicalPartyRepository
                .GetAllQuery()
                .AnyAsync(p => p.Acronym.ToLower() == acronym.ToLower() && p.Id != dto.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un partido político registrado con estas siglas."
                };
            }

            if (await HasElectionParticipation(entity.Id))
            {
                if (entity.Acronym != acronym)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se pueden modificar las siglas de este partido político porque ya participó en una elección."
                    };
                }

                if (entity.Name != dto.Name.Trim())
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede modificar el nombre del partido porque ya participó en una elección."
                    };
                }
            }

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description ?? "";
            entity.Acronym = acronym;

            if (!string.IsNullOrWhiteSpace(dto.LogoPath))
            {
                entity.LogoPath = dto.LogoPath;
            }

            entity.Status = dto.Status;

            var updated = await _politicalPartyRepository.UpdateAsync(entity.Id, entity);

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
                Message = "Partido político actualizado correctamente."
            };
        }

        public async Task<Result<PoliticalPartyDto?>> GetById(int id)
        {
            Result<PoliticalPartyDto?> result = new();
            try
            {
                var entity = await _politicalPartyRepository.GetById(id);

                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Partido politico no encontrado";
                    return result;
                }

                PoliticalPartyDto dto = new()
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    Acronym = entity.Acronym,
                    LogoPath = entity.LogoPath,
                    Status = entity.Status
                };

                result.Data = dto;
                result.IsSuccess = true;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener partido politico";
            }

            return result;
        }

        public async Task UpdateLogo(int id, string logoPath)
        {
            var entity = await _politicalPartyRepository.GetById(id);

            if (entity != null)
            {
                entity.LogoPath = logoPath;
                await _politicalPartyRepository.UpdateAsync(entity.Id, entity);
            }
        }


        public async Task<Result> ActivateAsync(int id)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede activar un partido político mientras exista una elección activa."
                };
            }

            var party = await _politicalPartyRepository.GetById(id);

            if (party == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político no existe."
                };
            }

            if (party.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este partido político ya se encuentra activo."
                };
            }

            if (await _politicalPartyRepository
                .GetAllQuery()
                .AnyAsync(p => p.Acronym.ToLower() == party.Acronym.ToLower() && p.Id != party.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un partido político registrado con estas siglas."
                };
            }

            party.Status = true;

            var updated = await _politicalPartyRepository.UpdateAsync(party.Id, party);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo activar el partido político."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Partido político activado correctamente."
            };
        }

        public async Task<Result> DeactivateAsync(int id)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar un partido político mientras exista una elección activa."
                };
            }

            var party = await _politicalPartyRepository.GetById(id);

            if (party == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El partido político no existe."
                };
            }

            if (!party.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este partido político ya se encuentra inactivo."
                };
            }

            if (await HasActiveCandidates(party.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar este partido político porque tiene candidatos activos registrados."
                };
            }

            if (await HasActiveDirector(party.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar este partido político porque tiene un dirigente político asignado."
                };
            }

            party.Status = false;

            var updated = await _politicalPartyRepository.UpdateAsync(party.Id, party);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo desactivar el partido político."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Partido político desactivado correctamente."
            };
        }



        private async Task<bool> HasActiveElection()
        {
            return false;
        }

        private async Task<bool> HasElectionParticipation(int partyId)
        {
            return false;
        }


        private async Task<bool> HasActiveDirector(int partyId)
        {
            return false;
        }


        private async Task<bool> HasActiveCandidates(int partyId)
        {
            return false;
        }

    }

}
