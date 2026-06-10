using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.User;
using eVote360.Core.Application.Helpers;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Result<UserDto>> LoginAsync(LoginDto dto)
        {
            User? user = await _userRepository.LoginAsync(dto.UserName, dto.Password);

            if (user == null)
            {
                return new Result<UserDto>
                {
                    IsSuccess = false,
                    Message = "Usuario o contraseña incorrectos."
                };
            }

            if (!user.Status)
            {
                return new Result<UserDto>
                {
                    IsSuccess = false,
                    Message = "Este usuario se encuentra inactivo."
                };
            }

            if (user.Role == Role.DIRECTOR)
            {
                bool hasParty = await HasPoliticalAssignment(user.Id);

                if (!hasParty)
                {
                    return new Result<UserDto>
                    {
                        IsSuccess = false,
                        Message = "El usuario no tiene un partido político asignado."
                    };
                }
            }

            return new Result<UserDto>
            {
                IsSuccess = true,
                Message = "Login exitoso.",
                Data = new UserDto
                {
                    Email = user.Email,
                    Id = user.Id,
                    LastName = user.LastName,
                    Name = user.Name,
                    Role = user.Role,
                    UserName = user.UserName,
                    Status = user.Status,
                    PoliticalPartyId = user.PoliticalPartyId
                }
            };
        }


        public async Task<Result> AddAsync(CreateUserDto dto)
        {
            Result result = new();

            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede crear un usuario mientras exista una elección activa."
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

            if (string.IsNullOrWhiteSpace(dto.UserName))
                return new Result { IsSuccess = false, Message = "El nombre de usuario es requerido." };

            if (string.IsNullOrWhiteSpace(dto.Password))
                return new Result { IsSuccess = false, Message = "La contraseña es requerida." };

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                return new Result { IsSuccess = false, Message = "La confirmación de contraseña es requerida." };

            if (dto.Password != dto.ConfirmPassword)
                return new Result { IsSuccess = false, Message = "La contraseña y la confirmación de contraseña no coinciden." };

            if (!IsValidPassword(dto.Password))
                return new Result { IsSuccess = false, Message = "La contraseña debe tener al menos 8 caracteres, una letra y un número." };

            if (dto.Role != Role.ADMIN && dto.Role != Role.DIRECTOR)
                return new Result { IsSuccess = false, Message = "Debe seleccionar un rol válido para el usuario." };

            string userName = dto.UserName.Trim();
            string email = dto.Email.Trim().ToLower();

            if (await UserNameExists(userName))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario registrado con este nombre de usuario."
                };
            }

            if (await EmailExists(email))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario registrado con este correo electrónico."
                };
            }

            User entity = new()
            {
                Name = dto.Name.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                UserName = userName,
                Password = PasswordEncryptation.ComputeSha256Hash(dto.Password),
                Role = dto.Role,
                Status = dto.Status
            };

            User? returnEntity = await _userRepository.AddAsync(entity);

            if (returnEntity == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo registrar el usuario."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Usuario creado correctamente."
            };
        }




        public async Task<Result> UpdateAsync(SaveUserDto dto, int currentUserId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede editar un usuario mientras exista una elección activa."
                };
            }

            var entityDb = await _userRepository.GetById(dto.Id);

            if (entityDb == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario no existe."
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

            if (string.IsNullOrWhiteSpace(dto.UserName))
                return new Result { IsSuccess = false, Message = "El nombre de usuario es requerido." };

            if (dto.Role != Role.ADMIN && dto.Role != Role.DIRECTOR)
                return new Result { IsSuccess = false, Message = "Debe seleccionar un rol válido para el usuario." };

            if (!string.IsNullOrWhiteSpace(dto.Password) || !string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            {
                if (string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "Debe completar ambos campos de contraseña."
                    };
                }

                if (dto.Password != dto.ConfirmPassword)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "La contraseña y la confirmación no coinciden."
                    };
                }

                if (!IsValidPassword(dto.Password))
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "La contraseña debe tener al menos 8 caracteres, una letra y un número."
                    };
                }
            }

            string userName = dto.UserName.Trim();
            string email = dto.Email.Trim().ToLower();

            if (await _userRepository
                .GetAllQuery()
                .AnyAsync(u => u.UserName.ToLower() == userName.ToLower() && u.Id != dto.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario registrado con este nombre de usuario."
                };
            }

            if (await _userRepository
                .GetAllQuery()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != dto.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario registrado con este correo electrónico."
                };
            }

            if (entityDb.Role == Role.DIRECTOR && dto.Role == Role.ADMIN)
            {
                if (await HasPoliticalAssignment(entityDb.Id))
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede cambiar el rol de este usuario porque tiene un partido político asignado como dirigente."
                    };
                }
            }

            if (entityDb.Role == Role.ADMIN)
            {
                bool isChangingRole = dto.Role != Role.ADMIN;
                bool isDeactivating = dto.Status == false;

                if (isChangingRole || isDeactivating)
                {
                    if (await IsOnlyActiveAdmin(entityDb.Id))
                    {
                        return new Result
                        {
                            IsSuccess = false,
                            Message = "No se puede modificar este usuario porque es el único administrador activo del sistema."
                        };
                    }
                }
            }

            if (dto.Id == currentUserId)
            {
                if (dto.Role != entityDb.Role || dto.Status != entityDb.Status)
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No puede cambiar su propio rol ni desactivar su propio usuario mientras está autenticado."
                    };
                }
            }

            entityDb.Name = dto.Name.Trim();
            entityDb.LastName = dto.LastName.Trim();
            entityDb.Email = email;
            entityDb.UserName = userName;
            entityDb.Role = dto.Role;
            entityDb.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                entityDb.Password = PasswordEncryptation.ComputeSha256Hash(dto.Password);
            }

            var updated = await _userRepository.UpdateAsync(entityDb.Id, entityDb);

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
                Message = "Usuario actualizado correctamente."
            };
        }

        public async Task<Result<List<UserDto>>> GetAll()
        {
            Result<List<UserDto>> result = new();
            try
            {
                var listEntities = await _userRepository.GetAllList();

                var listEntityDtos = listEntities.Select(s =>
                    new UserDto()
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Email = s.Email,
                        UserName = s.UserName,
                        LastName = s.LastName,
                        Role = s.Role,
                        Status = s.Status
                    }).ToList();

                result.IsSuccess = true;
                result.Data = listEntityDtos;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al cargar usuarios";
            }

            return result;
        }

        public async Task<Result<UserDto?>> GetById(int id)
        {
            Result<UserDto?> result = new();
            try
            {
                var entity = await _userRepository.GetById(id);

                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no encontrado";
                    return result;
                }

                UserDto dto = new()
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Email = entity.Email,
                    UserName = entity.UserName,
                    LastName = entity.LastName,
                    Role = entity.Role,
                    Status= entity.Status
                }; 
                
                result.Data = dto;
                result.IsSuccess = true;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener usuario";
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
                    Message = "No se puede activar un usuario mientras exista una elección activa."
                };
            }

            var user = await _userRepository.GetById(id);

            if (user == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario no existe."
                };
            }

            if (user.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este usuario ya se encuentra activo."
                };
            }

            if (user.Role != Role.ADMIN && user.Role != Role.DIRECTOR)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario tiene un rol inválido."
                };
            }

            if (await _userRepository
                .GetAllQuery()
                .AnyAsync(u => u.UserName.ToLower() == user.UserName.ToLower() && u.Id != user.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario registrado con este nombre de usuario."
                };
            }

            if (await _userRepository
                .GetAllQuery()
                .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.Id != user.Id))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario registrado con este correo electrónico."
                };
            }

            user.Status = true;

            var updated = await _userRepository.UpdateAsync(user.Id, user);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo activar el usuario."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Usuario activado correctamente."
            };
        }

        public async Task<Result> DeactivateAsync(int id, int currentUserId)
        {
            if (await HasActiveElection())
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se puede desactivar un usuario mientras exista una elección activa."
                };
            }

            var user = await _userRepository.GetById(id);

            if (user == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El usuario no existe."
                };
            }

            if (!user.Status)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este usuario ya se encuentra inactivo."
                };
            }

            if (user.Id == currentUserId)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No puede desactivar su propio usuario mientras está autenticado."
                };
            }

            if (user.Role == Role.ADMIN)
            {
                if (await IsOnlyActiveAdmin(user.Id))
                {
                    return new Result
                    {
                        IsSuccess = false,
                        Message = "No se puede desactivar este usuario porque es el único administrador activo del sistema."
                    };
                }
            }

            user.Status = false;

            var updated = await _userRepository.UpdateAsync(user.Id, user);

            if (updated == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No se pudo desactivar el usuario."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Usuario desactivado correctamente."
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


        private bool IsValidPassword(string password)
        {
            if (password.Length < 8) return false;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasNumber = password.Any(char.IsDigit);

            return hasLetter && hasNumber;
        }

        private async Task<bool> UserNameExists(string userName)
        {
            return await _userRepository.GetAllQuery().AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
        }

        private async Task<bool> EmailExists(string email)
        {
            return await _userRepository.GetAllQuery().AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        private async Task<bool> HasActiveElection()
        {
            return false;
        }

        private async Task<bool> HasPoliticalAssignment(int userId)
        {
            var user = await _userRepository.GetById(userId);

            return user != null && user.PoliticalPartyId.HasValue;
        }


        private async Task<bool> IsOnlyActiveAdmin(int userId)
        {
            var admins = await _userRepository
                .GetAllQuery()
                .Where(u => u.Role == Role.ADMIN && u.Status == true)
                .ToListAsync();

            return admins.Count == 1 && admins[0].Id == userId;
        }
    }
}
