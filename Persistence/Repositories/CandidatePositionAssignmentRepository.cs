using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Contexts;

namespace eVote360.Infrastructure.Persistence.Repositories
{
    public class CandidatePositionAssignmentRepository
        : GenericRepository<CandidatePositionAssignment>, ICandidatePositionAssignmentRepository
    {
        public CandidatePositionAssignmentRepository(eVote360AppContext context) : base(context)
        {
        }
    }
}