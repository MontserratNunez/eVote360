using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.AssignPosition
{
    public class AssignPositionViewModel
    {
        public int Id { get; set; }

        public string CandidateName { get; set; }

        public string CandidateLastName { get; set; }

        public string CandidatePartyName { get; set; }

        public string PositionName { get; set; }

        public string CandidateType { get; set; }

        public bool IsAlliance { get; set; }
        public bool IsEditable { get; set; }
    }
}
