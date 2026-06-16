using eVote360.Core.Domain.Common.Enums;
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

        public int RequestingPartyId { get; set; }

        public int ReceivingPartyId { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? ResponseDate { get; set; }

        public PoliticalAllianceStatus Status { get; set; }

        public PoliticalParty RequestingParty { get; set; }

        public PoliticalParty ReceivingParty { get; set; }
    }
}
