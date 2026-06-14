using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Vote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IVoteService
    {
        Task<Result<(int CitizenId, int ElectionId)>> StartAsync(StartVoteDto dto);
        Task<Result> ValidateIdentityAsync(OcrValidationDto dto);
        Task<Result> VerifyCodeAsync(VerifyCodeDto dto);
        Task<Result<List<AvailablePositionsDto>>> GetAvailablePositionsAsync(Dictionary<int, int?> selectedVotes);
        Task<Result<CandidatesDto>> GetCandidatesByPositionAsync(int positionId);
        Task<Result> FinalizeVotingAsync(FinalizeVoteDto dto);
    }
}
