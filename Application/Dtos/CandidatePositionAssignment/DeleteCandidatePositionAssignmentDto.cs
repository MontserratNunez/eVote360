using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.CandidatePositionAssignment
{
    public class DeleteCandidatePositionAssignmentDto
    {
        public int Id { get; set; }

        public string CandidateFullName { get; set; } = string.Empty;

        public string ElectivePositionName { get; set; } = string.Empty;

        public string PoliticalPartyName { get; set; } = string.Empty;
    }
}