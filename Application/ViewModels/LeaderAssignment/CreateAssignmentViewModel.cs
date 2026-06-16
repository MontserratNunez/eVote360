using eVote360.Core.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.LeaderAssignment
{
    public class CreateAssignmentViewModel
    {
        public int UserId { get; set; }

        public int PoliticalPartyId { get; set; }

        public List<DropdownDto?> Users { get; set; } = new();
        public List<DropdownDto?> Parties { get; set; } = new();
    }
}
