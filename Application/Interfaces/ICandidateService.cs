using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Candidate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface ICandidateService
    {
        Task<Result<List<CandidateDto>>> GetAllAsync(int userId);
        Task<Result<CandidateDto>> CreateAsync(CreateCandidateDto dto, int userId);
        Task<Result<CandidateDto>> GetById(int id, int userId);
        Task<Result> UpdateAsync(UpdateCandidateDto dto, int userId);
        Task UpdatePhoto(int id, string photoPath);
        Task<Result> ActivateAsync(int id, int userId);
        Task<Result> DeactivateAsync(int id, int userId);
        Task<bool> HasActiveElection();
        Task<bool> HasCandidateParticipated(int candidateId);
    }
}
