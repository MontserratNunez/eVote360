using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Citizen
{
    public class CitizenSession
    {
        public int CitizenId { get; set; }

        public int ElectionId { get; set; }

        public bool IdentityValidated { get; set; }

        public bool CodeValidated { get; set; }

        public bool HasFinalizedVoted { get; set; }

        public Dictionary<int, int?> SelectedVotes { get; set; } = new();
    }
}
