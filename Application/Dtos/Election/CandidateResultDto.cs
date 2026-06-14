using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Election
{
    public class CandidateResultDto
    {
        public int? CandidateId { get; set; }
        public string CandidateFullName { get; set; }
        public string PoliticalPartyName { get; set; }
        public int VotesCount { get; set; }
        public decimal Percentage { get; set; }
        public bool IsWinner { get; set; }
    }
}
