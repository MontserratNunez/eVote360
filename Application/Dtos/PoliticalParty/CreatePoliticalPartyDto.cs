using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.PoliticalParty
{
    public class CreatePoliticalPartyDto
    {
        public required string Name { get; set; } = null!;

        public string? Description { get; set; }

        public required string Acronym { get; set; } = null!;

        public required string LogoPath { get; set; } = null!;

        public required bool Status { get; set; }
    }
}
