using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.HomeAdmin
{
    public class YearlyElectionSummaryDto
    {
        public string ElectionName { get; set; }
        public DateTime ElectionDate { get; set; }
        public int TotalParties { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalCitizensVoted { get; set; }
    }
}
