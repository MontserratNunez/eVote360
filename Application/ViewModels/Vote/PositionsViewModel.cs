using eVote360.Core.Application.Dtos.Vote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Vote
{
    public class PositionsViewModel
    {
        public List<AvailablePositionsViewModel> Positions { get; set; } = new();
        public bool CanFinalizeVoting { get; set; }
    }
}
