using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Persistence.Repositories
{
    public class CitizenRepository : GenericRepository<Citizen>, ICitizenRepository
    {
        private readonly eVote360AppContext _dbContext;

        public CitizenRepository(eVote360AppContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<bool> ExistsByDocumentAsync(string documentNumber, int idToIgnore = 0)
        {
            return await _dbContext.Set<Citizen>().AnyAsync(c =>
                c.DocumentNumber.Trim() == documentNumber.Trim() && c.Id != idToIgnore);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int idToIgnore = 0)
        {
            return await _dbContext.Set<Citizen>().AnyAsync(c =>
                c.Email.Trim().ToLower() == email.Trim().ToLower() && c.Id != idToIgnore);
        }
    }
}