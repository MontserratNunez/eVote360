using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace eVote360.Core.Application.ViewModels.CandidatePositionAssignment
{
    public class DeleteCandidatePositionAssignmentViewModel
    {
        public int Id { get; set; }

        public string CandidateFullName { get; set; } = string.Empty;

        public string ElectivePositionName { get; set; } = string.Empty;

        public string PoliticalPartyName { get; set; } = string.Empty;
    }
}