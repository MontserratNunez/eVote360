using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Election
{
    public class ElectionResultDto
    {
        public int ElectionId { get; set; }
        public string ElectionName { get; set; }
        public List<PositionResultSummaryDto> Positions { get; set; } = new();
    }
}
