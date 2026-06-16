

namespace eVote360.Core.Domain.Entities
{
    public class ElectivePosition
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public bool Status { get; set; }

        public ICollection<AssignPosition> AssignPositions { get; set; } = new List<AssignPosition>();
    }
}
