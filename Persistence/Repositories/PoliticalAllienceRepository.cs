using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Contexts;
using eVote360.Core.Domain.Entities;

namespace eVote360.Infrastructure.Persistence.Repositories
{
    public class PoliticalAllienceRepository : GenericRepository<PoliticalAlliance>, IPoliticalAllienceRepository
    {
        private readonly eVote360AppContext _dbContext;

        public PoliticalAllienceRepository(eVote360AppContext context) : base(context)
        {
            _dbContext = context;
        }
    }
}
