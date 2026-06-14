using eVote360.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.ViewModels.Election
{
    public class ElectionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public ElectionStatus Status { get; set; }
        public string StatusText { get; set; }

        public int PartiesCount { get; set; }

        public int PositionsCount { get; set; }

        public int VotersCount { get; set; }
    }
}
