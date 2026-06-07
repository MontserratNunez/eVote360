using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.PoliticalParty;
using eVote360.Core.Application.Dtos.User;

namespace eVote360.Core.Application.Interfaces
{
    public interface IPoliticalPartyService
    {
        Task<Result<List<PoliticalPartyDto>>> GetAllAsync();
        Task<Result<PoliticalPartyDto?>> CreateAsync(CreatePoliticalPartyDto dto);
        Task UpdateLogo(int id, string logoPath);
        Task<Result> UpdateAsync(UpdatePoliticalPartyDto dto);
        Task<Result<PoliticalPartyDto?>> GetById(int id);
        Task<Result> ActivateAsync(int id);
        Task<Result> DeactivateAsync(int id);
    }
}