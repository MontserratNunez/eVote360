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
    public class VerificationCodeRepository : GenericRepository<VerificationCode>, IVerificationCodeRepository
    {
        private readonly eVote360AppContext _dbContext;

        public VerificationCodeRepository(eVote360AppContext context) : base(context)
        {
            _dbContext = context;
        }
    }
}
