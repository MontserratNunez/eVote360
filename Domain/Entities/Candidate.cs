using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class Candidate
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string PhotoPath { get; set; }

        public bool Status { get; set; }

        public int PoliticalPartyId { get; set; }

        public PoliticalParty PoliticalParty { get; set; } = null!;
    }
}