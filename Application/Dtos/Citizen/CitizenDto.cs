
namespace eVote360.Core.Application.Dtos.Citizen
{
    public class CitizenDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public required string DocumentNumber { get; set; }

        public required bool Status { get; set; }
    }
}