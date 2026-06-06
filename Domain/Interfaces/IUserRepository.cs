using eVote360.Core.Domain.Entities;

namespace eVote360.Core.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> LoginAsync(string userName, string password);
    }
}