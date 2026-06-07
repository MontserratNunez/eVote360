using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.User;

namespace eVote360.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result> AddAsync(CreateUserDto dto);
        Task<Result> UpdateAsync(SaveUserDto dto, int currentUserId);
        Task<Result<List<UserDto>>> GetAll();
        Task<Result<UserDto?>> GetById(int id);
        Task<Result<UserDto>> LoginAsync(LoginDto dto);

        Task<Result> ActivateAsync(int id);

        Task<Result> DeactivateAsync(int id, int currentUserId);
    }
}