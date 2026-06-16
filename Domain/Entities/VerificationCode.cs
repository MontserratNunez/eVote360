using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Domain.Entities
{
    public class VerificationCode
    {
        public int Id { get; set; }

        public int CitizenId { get; set; }
        public Citizen Citizen { get; set; }

        public int ElectionId { get; set; }
        public Election Election { get; set; }

        public string Code { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public bool Used { get; set; }
    }
}
