namespace eVote360.Core.Application.ViewModels.Citizen
{
    public class CitizenViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string DocumentNumber { get; set; }
        public required bool Status { get; set; }
    }
}
