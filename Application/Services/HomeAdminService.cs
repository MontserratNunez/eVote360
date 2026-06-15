using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.HomeAdmin;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Infrastructure.Shared.Services
{
    public class HomeAdminService : IHomeAdminService
    {
        private readonly IElectionRepository _electionRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IAssignPositionRepository _assignPositionRepository;
        private readonly ICitizenRepository _citizenRepository;

        public HomeAdminService(
            IElectionRepository electionRepository,
            IVoteRepository voteRepository,
            IAssignPositionRepository assignPositionRepository,
            ICitizenRepository citizenRepository)
        {
            _electionRepository = electionRepository;
            _voteRepository = voteRepository;
            _assignPositionRepository = assignPositionRepository;
            _citizenRepository = citizenRepository;
        }

        public async Task<Result<AdminDashboardDto>> GetDashboardInitialDataAsync()
        {
            var result = new Result<AdminDashboardDto>();
            try
            {
                var dashboard = new AdminDashboardDto();

                var activeElection = await _electionRepository.GetAllQuery()
                    .FirstOrDefaultAsync(e => e.Status == ElectionStatus.ACTIVE);

                if (activeElection != null)
                {
                    var currentAssignments = await _assignPositionRepository.GetAllQuery()
                        .Where(a => a.ElectionId == activeElection.Id)
                        .ToListAsync();

                    int partiesCount = currentAssignments.Select(a => a.PoliticalPartyId).Distinct().Count();
                    int candidatesCount = currentAssignments.Select(a => a.CandidateId).Distinct().Count();

                    int citizensVoted = await _voteRepository.GetAllQuery()
                        .Where(v => v.ElectionId == activeElection.Id)
                        .Select(v => v.CitizenId)
                        .Distinct()
                        .CountAsync();

                    int totalActiveCitizens = await _citizenRepository.GetAllQuery()
                        .CountAsync(c => c.Status == true);

                    dashboard.ActiveElection = new ActiveElectionSummaryDto
                    {
                        ElectionId = activeElection.Id,
                        ElectionName = activeElection.Name,
                        TotalParties = partiesCount,
                        TotalCandidates = candidatesCount,
                        TotalCitizensVoted = citizensVoted,
                        TotalCitizensPending = Math.Max(0, totalActiveCitizens - citizensVoted)
                    };
                }

                var electionYears = await _electionRepository.GetAllQuery()
                    .Where(e => e.Status == ElectionStatus.FINISHED)
                    .Select(e => e.Date.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                dashboard.AvailableYears = electionYears;
                dashboard.DefaultYear = electionYears.FirstOrDefault();

                result.IsSuccess = true;
                result.Data = dashboard;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al recopilar la información del panel de administración.";
            }
            
            return result;
        }

        public async Task<Result<List<YearlyElectionSummaryDto>>> GetElectionSummaryByYearAsync(int year)
        {
            var result = new Result<List<YearlyElectionSummaryDto>>();
            try
            {
                if (year <= 0)
                {
                    result.IsSuccess = false;
                    result.Message = "Debe seleccionar un año para consultar el resumen electoral.";
                    return result;
                }

                var currentYearElections = await _electionRepository.GetAllQuery()
                    .Where(e => e.Date.Year == year && e.Status == ElectionStatus.FINISHED)
                    .ToListAsync();

                if (!currentYearElections.Any())
                {
                    result.IsSuccess = false;
                    result.Message = "No existen elecciones finalizadas para el año seleccionado.";
                    return result;
                }

                var summaryList = new List<YearlyElectionSummaryDto>();

                foreach (var election in currentYearElections)
                {
                    var electionAssignments = await _assignPositionRepository.GetAllQuery()
                        .Where(a => a.ElectionId == election.Id)
                        .ToListAsync();

                    int totalParties = electionAssignments.Select(a => a.PoliticalPartyId).Distinct().Count();

                    int totalCandidates = electionAssignments.Select(a => a.CandidateId).Distinct().Count();

                    int totalCitizensVoted = await _voteRepository.GetAllQuery()
                        .Where(v => v.ElectionId == election.Id)
                        .Select(v => v.CitizenId)
                        .Distinct()
                        .CountAsync();

                    summaryList.Add(new YearlyElectionSummaryDto
                    {
                        ElectionName = election.Name,
                        ElectionDate = election.CreatedDate,
                        TotalParties = totalParties,
                        TotalCandidates = totalCandidates,
                        TotalCitizensVoted = totalCitizensVoted
                    });
                }

                result.IsSuccess = true;
                result.Data = summaryList;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado al consultar el resumen del año seleccionado.";
            }

            return result;
        }


    }
}