using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.AssignPosition
{
    public class AssignPositionIndexViewModel
    {
        public List<AssignPositionViewModel> CurrentAssignments { get; set; } = new();
        public List<AssignPositionViewModel> PastAssignments { get; set; } = new();
    }
}
