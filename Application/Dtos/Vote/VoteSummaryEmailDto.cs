using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class VoteSummaryEmailDto
    {
        public required string CitizenName { get; set; }
        public required string CitizenEmail { get; set; }
        public required string ElectionName { get; set; }
        public required string ElectionDate { get; set; }
        public List<VoteSummaryLineDto> VotedPositions { get; set; } = new();
    }
}
