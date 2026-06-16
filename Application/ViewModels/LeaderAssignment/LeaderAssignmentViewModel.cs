using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.LeaderAssignment
{
    public class LeaderAssignmentViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        public string PartyName { get; set; }

        public string PartyAcronym { get; set; }

        public bool UserStatus { get; set; }

        public bool PartyStatus { get; set; }
    }
}
