using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class PoliticalParty
    {
        public int Id { get; set; }

        public  required string Name { get; set; }

        public string? Description { get; set; }

        public required string Acronym { get; set; }

        public required string LogoPath { get; set; }

        public required bool Status { get; set; }

        public PoliticalLeaderAssignment? PoliticalLeaderAssignment { get; set; }

        public ICollection<PoliticalAlliance> AlliancesSent { get; set; } = new List<PoliticalAlliance>();

        public ICollection<PoliticalAlliance> AlliancesReceived { get; set; } = new List<PoliticalAlliance>();

        public ICollection<AssignPosition> AssignPositions { get; set; } = new List<AssignPosition>();
    }
}
