using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.AssignPosition
{
    public class CreateAssignPositionDto
    {
        public int CandidateId { get; set; }

        public int ElectivePositionId { get; set; }
    }
}
