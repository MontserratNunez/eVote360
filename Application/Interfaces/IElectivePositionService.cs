using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.ElectivePosition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IElectivePositionService
    {
        Task<Result<List<ElectivePositionDto>>> GetAllAsync();

        Task<Result> CreateAsync(CreateElectivePositionDto dto);

        Task<Result<UpdateElectivePositionDto?>> GetById(int id);
        Task<Result> UpdateAsync(UpdateElectivePositionDto dto);

        Task<Result> ActivateAsync(int id);
        Task<Result> DeactivateAsync(int id);


        Task<bool> HasActiveElection();
        Task<bool> HasPositionBeenUsed(int positionId);
    }
}
