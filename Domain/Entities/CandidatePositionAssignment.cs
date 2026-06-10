using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class CandidatePositionAssignment
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public Candidate Candidate { get; set; } = null!;

        public int ElectivePositionId { get; set; }

        public ElectivePosition ElectivePosition { get; set; } = null!;

        public int PoliticalPartyId { get; set; }

        public PoliticalParty PoliticalParty { get; set; } = null!;

        public bool IsAlliedCandidate { get; set; }
    }
}
