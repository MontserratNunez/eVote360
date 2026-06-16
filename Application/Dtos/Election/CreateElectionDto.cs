using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Dtos.Election
{
    public class CreateElectionDto
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
