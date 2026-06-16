using eVote360.Core.Application.Dtos.HomeDirector;

namespace eVote360.Core.Application.ViewModels.HomeDirector
{
    public class DirectorHomeViewModel
    {
        public DirectorDashboardDto DashboardData { get; set; }
        public bool HasAssignedParty => DashboardData != null;
    }
}
