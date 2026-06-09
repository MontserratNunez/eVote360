using eVote360.Core.Domain.Interfaces;
using eVote360.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using eVote360.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;


namespace eVote360.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<eVote360AppContext>(opt => opt.UseSqlServer(connectionString,
                m => m.MigrationsAssembly(typeof(eVote360AppContext).Assembly.FullName)), 
                ServiceLifetime.Transient);

            #endregion

            #region Repositories IOC
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ICitizenRepository, CitizenRepository>();
            services.AddTransient<IPoliticalPartyRepository, PoliticalPartyRepository>();
            services.AddTransient<IElectivePositionRepository, ElectivePositionRepository>();
            #endregion
        }
    }
}
