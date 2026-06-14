using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.LeaderAssignment
{
    public class CreateAssignmentDto
    {
        public int UserId { get; set; }
        public int PoliticalPartyId { get; set; }
    }
}
