using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.CandidatePositionAssignment;
using eVote360.Core.Application.ViewModels.CandidatePositionAssignment;

namespace eVote360.Core.Application.Interfaces
{
    public interface ICandidatePositionAssignmentService
    {
        Task<Result<List<CandidatePositionAssignmentDto>>> GetAllByPoliticalPartyAsync(int politicalPartyId);

        Task<Result<CreateCandidatePositionAssignmentViewModel>> GetCreateViewModelAsync(int politicalPartyId);

        Task<Result> CreateAsync(CreateCandidatePositionAssignmentDto dto);

        Task<Result<DeleteCandidatePositionAssignmentDto?>> GetDeleteInfoAsync(int id, int politicalPartyId);

        Task<Result> DeleteAsync(int id, int politicalPartyId);

        Task<bool> HasActiveElection();
    }
}