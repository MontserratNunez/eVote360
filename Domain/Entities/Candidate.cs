using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class Candidate
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string PhotoPath { get; set; }

        public required int PoliticalPartyId { get; set; }
        public PoliticalParty? PoliticalParty { get; set; }

        public int? ElectivePositionId { get; set; }
        public ElectivePosition? ElectivePosition { get; set; }

        public required bool Status { get; set; }

        public ICollection<AssignPosition> AssignPositions { get; set; } = new List<AssignPosition>();
        
    }
}
