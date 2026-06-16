using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.PoliticalParty
{
    public class PoliticalPartyViewModel
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public required string Acronym { get; set; }

        public required string LogoPath { get; set; }

        public required bool Status { get; set; }
    }
}
