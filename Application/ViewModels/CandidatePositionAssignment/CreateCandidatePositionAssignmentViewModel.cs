using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.ViewModels.CandidatePositionAssignment
{
    public class CreateCandidatePositionAssignmentViewModel
    {
        [Required(ErrorMessage = "You must select a candidate.")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a candidate.")]
        public int CandidateId { get; set; }

        [Required(ErrorMessage = "You must select an elective position.")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select an elective position.")]
        public int ElectivePositionId { get; set; }

        public int PoliticalPartyId { get; set; }

        public List<CandidateAssignmentOptionViewModel> Candidates { get; set; } = new();

        public List<ElectivePositionAssignmentOptionViewModel> ElectivePositions { get; set; } = new();
    }

    public class CandidateAssignmentOptionViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string PoliticalPartyName { get; set; } = string.Empty;

        public bool IsAlliedCandidate { get; set; }

        public string DisplayName => IsAlliedCandidate
            ? $"{FullName} - {PoliticalPartyName} (Allied)"
            : $"{FullName} - {PoliticalPartyName}";
    }

    public class ElectivePositionAssignmentOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}