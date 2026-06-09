using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Citizen;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Citizen;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class CitizenService : ICitizenService
    {
        private readonly ICitizenRepository _citizenRepository;

        public CitizenService(ICitizenRepository citizenRepository)
        {
            _citizenRepository = citizenRepository;
        }

        public async Task<Result<List<CitizenDto>>> GetAllAsync()
        {
            var result = new Result<List<CitizenDto>>();

            try
            {
                var entities = await _citizenRepository.GetAllList();

                result.IsSuccess = true;
                result.Data = entities.Select(c => new CitizenDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    LastName = c.LastName,
                    Email = c.Email,
                    DocumentNumber = c.DocumentNumber,
                    Status = c.Status
                }).ToList();

            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error listando ciudadanos";
            }

            return result;
        }

        public async Task<Result> CreateAsync(SaveCitizenDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede crear un ciudadano mientras exista una elección activa."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result { IsSuccess = false, Message = "El nombre es requerido." };

            if (string.IsNullOrWhiteSpace(dto.LastName))
                return new Result { IsSuccess = false, Message = "El apellido es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Email))
                return new Result { IsSuccess = false, Message = "El correo electrónico es requerido." };

            if (!IsValidEmail(dto.Email))
                return new Result { IsSuccess = false, Message = "Debe ingresar un correo electrónico válido." };

            if (string.IsNullOrWhiteSpace(dto.DocumentNumber))
                return new Result { IsSuccess = false, Message = "El número de documento de identidad es requerido." };

            string email = dto.Email.Trim().ToLower();
            string documentNumber = dto.DocumentNumber.Trim();

            if (await _citizenRepository.GetAllQuery().AnyAsync(c => c.Email.ToLower() == email))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un ciudadano registrado con este correo electrónico."
                };
            }

            if (await _citizenRepository
                .GetAllQuery()
                .AnyAsync(c => c.DocumentNumber == documentNumber))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un ciudadano registrado con este número de documento de identidad."
                };
            }

            Citizen entity = new()
            {
                Name = dto.Name.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                DocumentNumber = documentNumber,
                Status = true
            };

            var created = await _citizenRepository.AddAsync(entity);

            if (created == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo registrar el ciudadano."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Ciudadano creado correctamente."
            };
        }

        public async Task<Result> UpdateAsync(SaveCitizenDto dto)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede editar un ciudadano mientras exista una elección activa."
                };
            }

            var entity = await _citizenRepository.GetById(dto.Id);

            if (entity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El ciudadano no existe."
                };
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Result { IsSuccess = false, Message = "El nombre es requerido." };

            if (string.IsNullOrWhiteSpace(dto.LastName))
                return new Result { IsSuccess = false, Message = "El apellido es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Email))
                return new Result { IsSuccess = false, Message = "El correo electrónico es requerido." };

            if (!IsValidEmail(dto.Email))
                return new Result { IsSuccess = false, Message = "Debe ingresar un correo electrónico válido." };

            if (string.IsNullOrWhiteSpace(dto.DocumentNumber))
                return new Result { IsSuccess = false, Message = "El número de documento de identidad es requerido." };

            string email = dto.Email.Trim().ToLower();
            string documentNumber = dto.DocumentNumber.Trim();

            if (await _citizenRepository.GetAllQuery()
                .AnyAsync(c => c.Email.ToLower() == email && c.Id != dto.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un ciudadano registrado con este correo electrónico."
                };
            }

            if (await _citizenRepository.GetAllQuery()
                .AnyAsync(c => c.DocumentNumber == documentNumber && c.Id != dto.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un ciudadano registrado con este número de documento de identidad."
                };
            }

            if (await HasCitizenParticipated(entity.Id))
            {
                if (entity.DocumentNumber != documentNumber)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede modificar el número de documento de identidad de este ciudadano porque ya participó en una elección."
                    };
                }
            }

            entity.Name = dto.Name.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.Email = email;
            entity.DocumentNumber = documentNumber;
            entity.Status = dto.Status;

            var updated = await _citizenRepository.UpdateAsync(entity.Id, entity);

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
                Message = "Ciudadano actualizado correctamente."
            };
        }

        public async Task<Result<SaveCitizenViewModel?>> GetById(int id)
        {
            var result = new Result<SaveCitizenViewModel?>();
            
            try
            {
                var entity = await _citizenRepository.GetById(id);

                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Message = "No se encontró el ciudadano.";
                    return result;
                }

                result.IsSuccess = true;
                result.Data = new SaveCitizenViewModel
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    LastName = entity.LastName,
                    Email = entity.Email,
                    DocumentNumber = entity.DocumentNumber,
                    Status = entity.Status
                };
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al buscar ciudadano.";
                
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
                    Message = "No se puede activar un ciudadano mientras exista una elección activa."
                };
            }

            var citizen = await _citizenRepository.GetById(id);

            if (citizen == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El ciudadano no existe."
                };
            }

            if (citizen.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este ciudadano ya se encuentra activo."
                };
            }

            if (await _citizenRepository.GetAllQuery()
                .AnyAsync(c => c.DocumentNumber == citizen.DocumentNumber && c.Id != citizen.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un ciudadano registrado con este número de documento de identidad."
                };
            }

            citizen.Status = true;

            var updated = await _citizenRepository.UpdateAsync(citizen.Id, citizen);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo activar el ciudadano."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Ciudadano activado correctamente."
            };
        }

        public async Task<Result> DeactivateAsync(int id)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar un ciudadano mientras exista una elección activa."
                };
            }

            var citizen = await _citizenRepository.GetById(id);

            if (citizen == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El ciudadano no existe."
                };
            }

            if (!citizen.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este ciudadano ya se encuentra inactivo."
                };
            }

            citizen.Status = false;

            var updated = await _citizenRepository.UpdateAsync(citizen.Id, citizen);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo desactivar el ciudadano."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Ciudadano desactivado correctamente."
            };
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasActiveElection()
        {
            return false;
        }

        public async Task<bool> HasCitizenParticipated(int citizenId)
        {
            return false;
        }
    }
}