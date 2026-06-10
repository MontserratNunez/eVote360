using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class PoliticalAlliance
    {
        public int Id { get; set; }

        public int RequestingPoliticalPartyId { get; set; }

        public PoliticalParty RequestingPoliticalParty { get; set; } = null!;

        public int RequestedPoliticalPartyId { get; set; }

        public PoliticalParty RequestedPoliticalParty { get; set; } = null!;

        public required string Status { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? ResponseDate { get; set; }
    }
}
