using eVote360.Core.Application.ViewModels.User;

namespace eVote360.Core.Application.Interfaces
{
    public interface IUserSession
    {
        UserViewModel? GetUserSession();
        bool HasUser();

        bool IsAdmin();
    }
}