    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.CandidatePositionAssignment
{
    public class CreateCandidatePositionAssignmentDto
    {
        public int CandidateId { get; set; }

        public int ElectivePositionId { get; set; }

        public int PoliticalPartyId { get; set; }
    }
}