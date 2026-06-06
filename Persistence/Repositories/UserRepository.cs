using eVote360.Core.Application.Helpers;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly eVote360AppContext _dbContext;
        public UserRepository(eVote360AppContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<User?> LoginAsync(string userName, string password)
        {
            string passwordEncrypt = PasswordEncryptation.ComputeSha256Hash(password);

            User? user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => 
                u.UserName == userName && u.Password == passwordEncrypt);
            return user;
        }
    }
}