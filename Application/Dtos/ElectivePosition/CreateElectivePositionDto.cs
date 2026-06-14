using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.ElectivePosition
{
    public class CreateElectivePositionDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }
    }
}
