using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class Vote
    {
        public int Id { get; set; }

        public int CitizenId { get; set; }
        public Citizen Citizen { get; set; }

        public int ElectionId { get; set; }
        public Election Election { get; set; }

        public int ElectivePositionId { get; set; }
        public ElectivePosition ElectivePosition { get; set; }

        public int? CandidateId { get; set; }
        public Candidate? Candidate { get; set; }

        public DateTime CreatedDate { get; set; } 

        public bool IsBlank { get; set; }


    }
}
