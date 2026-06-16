using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Election
{
    public class ElectionResultViewModel
    {
        public int ElectionId { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public List<PositionResultSummaryViewModel> Positions { get; set; } = new();
    }
}
