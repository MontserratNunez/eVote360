using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class VerifyCodeDto
    {
        public int CitizenId { get; set; }
        public int ElectionId { get; set; }
        public required string Code { get; set; }
    }
}
