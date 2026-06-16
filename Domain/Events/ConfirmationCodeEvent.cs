using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Events
{
    public record ConfirmationCodeEvent
    {
        public required string Code { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
    }
}
