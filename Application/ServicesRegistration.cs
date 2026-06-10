using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eVote360.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            services.AddTransient<ICandidatePositionAssignmentService, CandidatePositionAssignmentService>();
        }

    }
}
