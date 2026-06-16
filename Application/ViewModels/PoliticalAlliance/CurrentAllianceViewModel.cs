using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.PoliticalAlliance
{
    public class CurrentAllianceViewModel
    {
        public int Id { get; set; }

        public string PartnerName { get; set; }

        public DateTime AcceptanceDate { get; set; }
    }
}
