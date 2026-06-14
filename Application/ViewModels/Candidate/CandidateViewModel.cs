using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Candidate
{
    public class CandidateViewModel
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string PhotoPath { get; set; }

        public string? PositionName { get; set; }

        public bool Status { get; set; }
    }
}
