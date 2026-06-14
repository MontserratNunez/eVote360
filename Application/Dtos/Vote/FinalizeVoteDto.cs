using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class FinalizeVoteDto
    {
        public int CitizenId { get; set; }
        public int ElectionId { get; set; }
        public Dictionary<int, int?> SelectedVotes { get; set; } = new();
    }
}
