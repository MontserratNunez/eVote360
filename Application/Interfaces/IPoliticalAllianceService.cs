using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.PoliticalAlliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IPoliticalAllianceService
    {
        Task<Result> CreateAsync(CreatePoliticalAllianceDto dto, int userId);
        Task<List<DropdownDto>> GetAvailableParties(int userId);
        Task<Result<List<PoliticalAllianceDto>>> GetAllSentAsync(int userId);
        Task<Result<List<CurrentAllianceDto>>> GetCurrentAsync(int userId);
        Task<Result<List<PendingAllianceDto>>> GetPendingAsync(int userId);
        Task<Result> AcceptAsync(int id, int userId);
        Task<Result> RejectAsync(int id, int userId);
        Task<Result> DeleteRequestAsync(int id, int userId);
        Task<Result> DeleteCurrentAsync(int id, int userId);
        Task<Result<string>> GetPartyName(int id);
        Task<Result<string>> GetPartyNameCurrent(int id, int userId);
        Task<bool> HasActiveElection();
    }
}
