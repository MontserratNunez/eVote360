using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Election
{
    public class PositionResultSummaryDto
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; }
        public int TotalVotesInPosition { get; set; }
        public bool IsTie { get; set; }
        public List<CandidateResultDto> Results { get; set; } = new();
    }
}
