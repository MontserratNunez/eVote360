using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Candidate
{
    public class UpdateCandidateDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string? PhotoPath { get; set; }
        public string? ExistingPhotoPath { get; set; }

        public bool Status { get; set; }
    }
}
