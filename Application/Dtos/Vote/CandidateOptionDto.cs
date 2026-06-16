using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Vote
{
    public class CandidateOptionDto
    {
        public int AssignPositionId { get; set; }
        public int CandidateId { get; set; }
        public required string CandidateFullName { get; set; }
        public required string CandidatePhotoPath { get; set; }
        public int PoliticalPartyId { get; set; }
        public required string PoliticalPartyName { get; set; }
        public required string PoliticalPartyLogoPath { get; set; }
    }
}
