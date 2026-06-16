using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.LeaderAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IPoliticalLeaderAssignmentService
    {
        Task<Result<List<LeaderAssignmentDto>>> GetAllAsync();
        Task<Result> CreateAsync(CreateAssignmentDto dto);
        Task<(List<DropdownDto> Users, List<DropdownDto> Parties)> GetDropdowns();
        Task<Result> DeleteAsync(int id);
        Task<bool> HasActiveElection();
    }
}
