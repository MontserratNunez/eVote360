using eVote360.Core.Application.Dtos.User;

namespace eVote360.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> AddAsync(SaveUserDto dto);
        Task<UserDto?> UpdateAsync(SaveUserDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<UserDto>> GetAll();
        Task<List<UserDto>> GetAllWithInclude();
        Task<UserDto?> GetById(int id);
        Task<UserDto?> LoginAsync(LoginDto dto);
    }
}