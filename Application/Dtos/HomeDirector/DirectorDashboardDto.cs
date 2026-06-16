using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.HomeDirector
{
    public class DirectorDashboardDto
    {
        public int PoliticalPartyId { get; set; }
        public string PartyName { get; set; }
        public string Acronym { get; set; }
        public string LogoPath { get; set; }

        public int ActiveCandidatesCount { get; set; }
        public int InactiveCandidatesCount { get; set; }
        public int ApprovedAlliancesCount { get; set; }
        public int PendingAllianceRequestsCount { get; set; }
        public int CandidatesAssignedToPositionsCount { get; set; }
    }
}
