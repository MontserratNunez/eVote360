using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.HomeDirector;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class HomeDirectorService : IHomeDirectorService
    {
        private readonly IPoliticalLeaderAssignmentRepository _leaderAssignmentRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IPoliticalAllienceRepository _allianceRequestRepository;
        private readonly IAssignPositionRepository _assignPositionRepository;

        public HomeDirectorService(
            IPoliticalLeaderAssignmentRepository leaderAssignmentRepository,
            ICandidateRepository candidateRepository,
            IPoliticalAllienceRepository allianceRequestRepository,
            IAssignPositionRepository assignPositionRepository)
        {
            _leaderAssignmentRepository = leaderAssignmentRepository;
            _candidateRepository = candidateRepository;
            _allianceRequestRepository = allianceRequestRepository;
            _assignPositionRepository = assignPositionRepository;
        }

        public async Task<Result<DirectorDashboardDto>> GetDirectorDashboardDataAsync(int userId)
        {
            var result = new Result<DirectorDashboardDto>();
            try
            {
                var assignment = await _leaderAssignmentRepository.GetAllQuery()
                    .Include(a => a.PoliticalParty)
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (assignment == null || assignment.PoliticalParty == null)
                {
                    result.IsSuccess = false;
                    result.Message = "El usuario actual no se encuentra asignado a ningún partido político.";
                    return result;
                }

                var party = assignment.PoliticalParty;
                int partyId = party.Id;

                var partyCandidates = await _candidateRepository.GetAllQuery()
                    .Where(c => c.PoliticalPartyId == partyId)
                    .ToListAsync();

                int activeCandidates = partyCandidates.Count(c => c.Status == true);
                int inactiveCandidates = partyCandidates.Count(c => c.Status == false);

                int approvedAlliances = await _allianceRequestRepository.GetAllQuery()
                    .CountAsync(a => (a.RequestingPartyId == partyId || a.ReceivingPartyId == partyId)
                                     && a.Status == PoliticalAllianceStatus.ACCEPTED);

                int pendingRequests = await _allianceRequestRepository.GetAllQuery()
                    .CountAsync(a => a.ReceivingPartyId == partyId
                                     && a.RequestingPartyId != partyId
                                     && a.Status == PoliticalAllianceStatus.PENDING);

                int assignedCandidates = await _assignPositionRepository.GetAllQuery()
                    .Where(ap => ap.PoliticalPartyId == partyId && ap.ElectionId == null)
                    .Select(ap => ap.CandidateId)
                    .Distinct()
                    .CountAsync();

                var dashboardDto = new DirectorDashboardDto
                {
                    PoliticalPartyId = partyId,
                    PartyName = party.Name,
                    Acronym = party.Acronym,
                    LogoPath = party.LogoPath,
                    ActiveCandidatesCount = activeCandidates,
                    InactiveCandidatesCount = inactiveCandidates,
                    ApprovedAlliancesCount = approvedAlliances,
                    PendingAllianceRequestsCount = pendingRequests,
                    CandidatesAssignedToPositionsCount = assignedCandidates
                };

                result.IsSuccess = true;
                result.Data = dashboardDto;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado al procesar las estadísticas del partido político.";
            }

            return result;
        }
    }
}
