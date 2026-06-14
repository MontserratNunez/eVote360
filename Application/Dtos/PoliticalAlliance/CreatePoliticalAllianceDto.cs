using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.PoliticalAlliance
{
    public class CreatePoliticalAllianceDto
    {
        public int RequestingPartyId { get; set; }
        public int ReceivingPartyId { get; set; }
    }
}
