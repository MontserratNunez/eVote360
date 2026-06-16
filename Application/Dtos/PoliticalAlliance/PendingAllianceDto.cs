using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.PoliticalAlliance
{
    public class PendingAllianceDto
    {
        public int Id { get; set; }

        public string PartyName { get; set; }

        public DateTime RequestDate { get; set; }

        public string Status { get; set; }
    }
}
