using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.AssignPosition
{
    public class AssignPositionListDto
    {
        public List<AssignPositionDto> CurrentAssignments { get; set; } = new();
        public List<AssignPositionDto> PastAssignments { get; set; } = new();
    }
}
