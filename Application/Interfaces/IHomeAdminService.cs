using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.HomeAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IHomeAdminService
    {
        Task<Result<AdminDashboardDto>> GetDashboardInitialDataAsync();
        Task<Result<List<YearlyElectionSummaryDto>>> GetElectionSummaryByYearAsync(int year);
    }
}
