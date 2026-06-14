using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.AssignPosition;

namespace eVote360.Core.Application.Interfaces
{
    public interface IAssignPositionService
    {
        //Task<Result<List<AssignPositionDto>>> GetAllAsync(int userId);
        Task<Result<AssignPositionListDto>> GetAllAsync(int userId);
        Task<Result> CreateAsync(CreateAssignPositionDto dto, int userId);
        Task<(List<DropdownDto> Candidates, List<DropdownDto> Positions)> GetDropdowns(int userId);
        Task<Result> DeleteAsync(int id, int userId);

        Task<bool> HasActiveElection();
    }
}
