using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class VoteSummaryLineDto
    {
        public required string PositionName { get; set; }
        public required string Selection { get; set; }
        public string PoliticalParty { get; set; } = string.Empty;
    }
}
