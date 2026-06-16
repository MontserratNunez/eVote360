using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Election;

namespace eVote360.Core.Application.Interfaces
{
    public interface IElectionService
    {
        Task<Result> CreateAsync(CreateElectionDto dto);
        Task<Result<List<ElectionDto>>> GetAllAsync();
        Task<Result> ActivateAsync(int id);
        Task<Result> FinishAsync(int id);

        Task<Result<ElectionResultDto>> GetResultsByElectionAsync(int electionId);
        Task<bool> HasActiveElection();
    }
}