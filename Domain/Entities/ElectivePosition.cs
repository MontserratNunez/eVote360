

namespace eVote360.Core.Domain.Entities
{
    public class ElectivePosition
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool Status { get; set; }
    }
}
