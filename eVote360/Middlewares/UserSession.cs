using eVote360.Core.Application.Helpers;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.User;
using eVote360.Core.Domain.Common.Enums;

namespace eVote360.Middlewares
{
    public class UserSession : IUserSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasUser()
        {
            UserViewModel? userViewModel = _httpContextAccessor.HttpContext?
                .Session.Get<UserViewModel>("User");

            if (userViewModel == null)
            {
                return false;
            }

            return true;
        }

        public UserViewModel? GetUserSession()
        {
            UserViewModel? userViewModel = _httpContextAccessor.HttpContext?
                .Session.Get<UserViewModel>("User");

            if (userViewModel == null)
            {
                return null;
            }

            return userViewModel;
        }


        public bool IsAdmin()
        {
            UserViewModel? userViewModel = _httpContextAccessor.HttpContext?
                .Session.Get<UserViewModel>("User");

            if (userViewModel == null)
            {
                return false;
            }

            return userViewModel.Role == Role.ADMIN;
        }
    }
}
