using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Events
{
    public record VoteSummaryEvent
    {
        public required string CitizenName { get; set; }
        public required string CitizenEmail { get; set; }
        public required string ElectionName { get; set; }
        public required string ElectionDate { get; set; }

        public required List<VoteSummaryLine> VotedPositions { get; init; }
    }

    public record VoteSummaryLine
    {
        public required string PositionName { get; init; }
        public required string Selection { get; init; }
        public string PoliticalParty { get; init; } = string.Empty;
    }
}
