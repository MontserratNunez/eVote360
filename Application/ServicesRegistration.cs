using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eVote360.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Services IOC
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICitizenService, CitizenService>();
            services.AddTransient<IPoliticalPartyService, PoliticalPartyService>();
            services.AddTransient<IElectivePositionService, ElectivePositionService>();
            #endregion
        }

    }
}
