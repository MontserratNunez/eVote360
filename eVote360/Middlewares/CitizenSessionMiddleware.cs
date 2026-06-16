using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.Helpers;
using eVote360.Core.Application.ViewModels.Citizen;


namespace eVote360.Middlewares
{
    public class CitizenSessionMiddleware : ICitizenSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CitizenSessionMiddleware(IHttpContextAccessor http)
        {
            _httpContextAccessor = http;
        }

        public CitizenSession Get()
        {
            return _httpContextAccessor.HttpContext.Session.Get<CitizenSession>("citizenSession");
        }

        public void Set(CitizenSession session)
        {
            _httpContextAccessor.HttpContext.Session.Set("citizenSession", session);
        }

        public void Remove()
        {
            _httpContextAccessor.HttpContext.Session.Remove("citizenSession");
        }
    }
}
