using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.HomeDirector;

namespace eVote360.Core.Application.Interfaces
{
    public interface IHomeDirectorService
    {
        Task<Result<DirectorDashboardDto>> GetDirectorDashboardDataAsync(int userId);
    }
}
