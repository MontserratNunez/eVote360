using eVote360.Core.Domain.Common.Enums;

namespace eVote360.Core.Application.Dtos.PoliticalAlliance
{
    public class PoliticalAllianceDto
    {
        public int Id { get; set; }

        public string PartyName { get; set; }

        public DateTime RequestDate { get; set; }

        public string StatusText { get; set; }
        public PoliticalAllianceStatus Status { get; set; }
    }
}
