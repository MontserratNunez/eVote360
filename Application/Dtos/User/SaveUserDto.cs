using eVote360.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Core.Application.Dtos.User
{
    public class SaveUserDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public required Role Role { get; set; }
        public required bool Status { get; set; }
    }
}
