using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.CandidatePositionAssignment
{
    public class CandidatePositionAssignmentViewModel
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public string CandidateName { get; set; } = string.Empty;

        public string CandidateLastName { get; set; } = string.Empty;

        public string CandidateFullName => $"{CandidateName} {CandidateLastName}";

        public int CandidatePoliticalPartyId { get; set; }

        public string CandidatePoliticalPartyName { get; set; } = string.Empty;

        public int ElectivePositionId { get; set; }

        public string ElectivePositionName { get; set; } = string.Empty;

        public int PoliticalPartyId { get; set; }

        public string PoliticalPartyName { get; set; } = string.Empty;

        public bool IsAlliedCandidate { get; set; }

        public string CandidateType => IsAlliedCandidate ? "Allied Candidate" : "Own Candidate";
    }
}