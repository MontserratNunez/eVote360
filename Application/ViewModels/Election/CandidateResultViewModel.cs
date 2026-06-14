using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Election
{
    public class CandidateResultViewModel
    {
        public int? CandidateId { get; set; }
        public string CandidateFullName { get; set; } = string.Empty;
        public string PoliticalPartyName { get; set; } = string.Empty;
        public int VotesCount { get; set; }
        public decimal Percentage { get; set; }
        public bool IsWinner { get; set; }
    }
}
