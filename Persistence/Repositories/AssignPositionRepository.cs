using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Contexts;

namespace eVote360.Infrastructure.Persistence.Repositories
{
    internal class AssignPositionRepository : GenericRepository<AssignPosition>, IAssignPositionRepository
    {
        private readonly eVote360AppContext _dbContext;

        public AssignPositionRepository(eVote360AppContext context) : base(context)
        {
            _dbContext = context;
        }
    }
}
