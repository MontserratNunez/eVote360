
namespace eVote360.Core.Application.ViewModels.User
{
    public class UserViewModel
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required int Role { get; set; }
        public required bool Status { get; set; }
    }
}
