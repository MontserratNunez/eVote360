using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class AvailablePositionsDto
    {
        public int Id { get; set; }
        public required string PositionName { get; set; }
        public int TotalParties { get; set; }
        public int TotalRealCandidates { get; set; }
        public bool IsSelected { get; set; }
    }
}
