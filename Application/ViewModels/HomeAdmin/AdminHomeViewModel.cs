using eVote360.Core.Application.Dtos;
using eVote360.Core.Application.Dtos.HomeAdmin;

namespace eVote360.Core.Application.ViewModels.HomeAdmin
{
    public class AdminHomeViewModel
    {
        public ActiveElectionSummaryDto ActiveElection { get; set; }

        public int? SelectedYear { get; set; }
        public List<DropdownDto> YearsList { get; set; } = new List<DropdownDto>();

        public List<YearlyElectionSummaryDto> YearlyElections { get; set; } = new List<YearlyElectionSummaryDto>();

        public bool HasYearlyRecords => YearsList != null && YearsList.Any();
    }
}
