using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Election
{
    public class PositionResultSummaryViewModel
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public int TotalVotesInPosition { get; set; }
        public bool IsTie { get; set; }
        public List<CandidateResultViewModel> Results { get; set; } = new();
    }
}
