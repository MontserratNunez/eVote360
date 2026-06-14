using eVote360.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class Election
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required DateTime Date { get; set; }

        public ElectionStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ActivatedDate { get; set; }

        public DateTime? FinishedDate { get; set; }
    }
}
