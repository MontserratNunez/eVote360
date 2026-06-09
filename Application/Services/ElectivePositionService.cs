using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.ElectivePosition;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Citizen;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class ElectivePositionService : IElectivePositionService
    {
        public readonly IElectivePositionRepository _electivePositionRepository;

        public ElectivePositionService(IElectivePositionRepository electivePositionRepository)
        {
            _electivePositionRepository = electivePositionRepository;
        }

        public async Task<Result<List<ElectivePositionDto>>> GetAllAsync()
        {
            var result = new Result<List<ElectivePositionDto>>();
            
            try
            {
                var entities = await _electivePositionRepository.GetAllList();

                if (entities == null || !entities.Any())
                {
                    result.IsSuccess = true;
                    result.Data = new List<ElectivePositionDto>();
                    return result;
                }

                result.IsSuccess = true;
                result.Data = entities.Select(e => new ElectivePositionDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Status = e.Status
                }).ToList();

               
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al listar puestos electivos.";
            }

            return result;
        }

        public async Task<Result> CreateAsync(CreateElectivePositionDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede crear un puesto electivo mientras exista una elección activa."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result { IsSuccess = false, Message = "El nombre del puesto es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Description))
                return new Result { IsSuccess = false, Message = "La descripción es requerida." };

            string name = dto.Name.Trim();

            if (await _electivePositionRepository.GetAllQuery().AnyAsync(e => e.Name.ToLower() == name.ToLower()))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un puesto electivo registrado con este nombre."
                };
            }

            ElectivePosition entity = new()
            {
                Name = name,
                Description = dto.Description.Trim(),
                Status = true
            };

            var created = await _electivePositionRepository.AddAsync(entity);

            if (created == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo registrar el puesto electivo."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Puesto electivo creado correctamente."
            };
        }

        public async Task<Result> UpdateAsync(UpdateElectivePositionDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede editar un puesto electivo mientras exista una elección activa."
                };
            }

            var entity = await _electivePositionRepository.GetById(dto.Id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El puesto electivo no existe."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result { IsSuccess = false, Message = "El nombre del puesto es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Description))
                return new Result { IsSuccess = false, Message = "La descripción es requerida." };

            string name = dto.Name.Trim();

            if (await _electivePositionRepository.GetAllQuery().AnyAsync(e => e.Name.ToLower() == name.ToLower() && e.Id != dto.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un puesto electivo registrado con este nombre."
                };
            }

            if (await HasPositionBeenUsed(entity.Id))
            {
                if (entity.Name != name)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede modificar el nombre de este puesto electivo porque ya fue utilizado en una elección."
                    };
                }
            }

            if (entity.Status == true && dto.Status == false )
            {
                if (await HasActiveCandidates(entity.Id))
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede desactivar este puesto electivo porque tiene candidatos activos asignados."
                    };
                }
            }


            entity.Name = name;
            entity.Description = dto.Description.Trim();
            entity.Status = dto.Status;

            var updated = await _electivePositionRepository.UpdateAsync(entity.Id, entity);

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
                Message = "Puesto electivo actualizado correctamente."
            };
        }

        public async Task<Result<UpdateElectivePositionDto?>> GetById(int id)
        {

            var result = new Result<UpdateElectivePositionDto?>();

            try
            {
                var entity = await _electivePositionRepository.GetById(id);

                if (entity == null)
                    return null;

                result.Data = new UpdateElectivePositionDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description ?? "",
                    Status = entity.Status
                };

            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al buscar posición electiva.";

            }

            return result;
        }

        public async Task<Result> ActivateAsync(int id)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede activar un puesto electivo mientras exista una elección activa."
                };
            }

            var entity = await _electivePositionRepository.GetById(id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El puesto electivo no existe."
                };
            }

            if (entity.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este puesto electivo ya se encuentra activo."
                };
            }

            if (await _electivePositionRepository.GetAllQuery()
                .AnyAsync(e => e.Name.ToLower() == entity.Name.ToLower() && e.Id != entity.Id&& e.Status)
                )
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un puesto electivo activo con este nombre."
                };
            }

            entity.Status = true;

            var updated = await _electivePositionRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo activar el puesto electivo."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Puesto electivo activado correctamente."
            };
        }

        public async Task<Result> DeactivateAsync(int id)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar un puesto electivo mientras exista una elección activa."
                };
            }

            var entity = await _electivePositionRepository.GetById(id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El puesto electivo no existe."
                };
            }

            if (!entity.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este puesto electivo ya se encuentra inactivo."
                };
            }

            if (await HasActiveCandidates(entity.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar este puesto electivo porque tiene candidatos activos asignados."
                };
            }

            entity.Status = false;

            var updated = await _electivePositionRepository.UpdateAsync(entity.Id, entity);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo desactivar el puesto electivo."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Puesto electivo desactivado correctamente."
            };
        }



        public async Task<bool> HasPositionBeenUsed(int positionId)
        {
            return false;
        }


        public async Task<bool> HasActiveElection()
        {
            return false;
        }


        private async Task<bool> HasActiveCandidates(int positionId)
        {
            return false;
        }

    }
}
