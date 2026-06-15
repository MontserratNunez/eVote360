using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.HomeAdmin
{
    public class AdminDashboardDto
    {
        public ActiveElectionSummaryDto ActiveElection { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
        public int DefaultYear { get; set; }
    }
}
