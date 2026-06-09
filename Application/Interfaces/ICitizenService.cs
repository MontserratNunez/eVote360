using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Citizen;
using eVote360.Core.Application.ViewModels.Citizen;

namespace eVote360.Core.Application.Interfaces
{
    public interface ICitizenService
    {
        Task<Result<List<CitizenDto>>> GetAllAsync();
        Task<Result> CreateAsync(SaveCitizenDto dto);
        Task<Result<SaveCitizenViewModel?>> GetById(int id);
        Task<Result> UpdateAsync(SaveCitizenDto dto);
        Task<Result> ActivateAsync(int id);
        Task<Result> DeactivateAsync(int id);
        Task<bool> HasActiveElection();
        Task<bool> HasCitizenParticipated(int citizenId);
    }
}