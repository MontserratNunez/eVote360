using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.Services;
using eVote360.Core.Domain.Interfaces;
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
            services.AddTransient<IPoliticalLeaderAssignmentService, PoliticalLeaderAssignmentService>();
            services.AddTransient<ICandidateService, CandidateService>();
            services.AddTransient<IPoliticalAllianceService, PoliticalAllianceService>();
            services.AddTransient<IAssignPositionService, AssignPositionService>();
            services.AddTransient<IElectionService, ElectionService>();
            services.AddTransient<IVoteService, VoteService>();
            #endregion
        }

    }
}
