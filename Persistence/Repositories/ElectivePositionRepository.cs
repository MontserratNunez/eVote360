using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Infrastructure.Persistence.Repositories
{
    public class ElectivePositionRepository : GenericRepository<ElectivePosition>, IElectivePositionRepository
    {
        public ElectivePositionRepository(eVote360AppContext context) : base(context)
        {
        }
    }
}
