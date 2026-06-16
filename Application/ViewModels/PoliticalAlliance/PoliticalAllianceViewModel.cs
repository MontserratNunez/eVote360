using eVote360.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.PoliticalAlliance
{
    public class PoliticalAllianceViewModel
    {
        public int Id { get; set; }

        public string PartyName { get; set; }

        public DateTime RequestDate { get; set; }

        public string StatusText { get; set; }
        public PoliticalAllianceStatus Status { get; set; }
    }
}
