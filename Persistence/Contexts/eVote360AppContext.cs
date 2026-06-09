using Microsoft.EntityFrameworkCore;
using eVote360.Core.Domain.Entities;
using System.Reflection;


namespace eVote360.Infrastructure.Persistence.Contexts
{
    public class eVote360AppContext : DbContext
    {
        public eVote360AppContext(DbContextOptions<eVote360AppContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<PoliticalParty> PoliticalParties { get; set; }
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<ElectivePosition> ElectivePositions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
