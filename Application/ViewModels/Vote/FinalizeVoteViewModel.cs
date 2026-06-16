using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Vote
{
    public class FinalizeVoteViewModel
    {
        public bool HasErrors { get; set; }
        public List<string> PendingPositions { get; set; } = new();
        public string ErrorMessage { get; set; }
    }
}
