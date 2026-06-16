using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class PoliticalLeaderAssignment
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int PoliticalPartyId { get; set; }
        public PoliticalParty PoliticalParty { get; set; }
    }
}
