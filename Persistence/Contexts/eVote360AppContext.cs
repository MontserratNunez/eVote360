using Microsoft.EntityFrameworkCore;
using eVote360.Core.Domain.Entities;
using System.Reflection;


namespace eVote360.Infrastructure.Persistence.Contexts
{
    public class eVote360AppContext : DbContext
    {
        public eVote360AppContext(DbContextOptions<eVote360AppContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<PoliticalParty> PoliticalParties { get; set; }
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<ElectivePosition> ElectivePositions { get; set; }
        public DbSet<PoliticalLeaderAssignment> PoliticalLeaderAssignments { get; set; }
        public DbSet<PoliticalAlliance> PoliticalAlliances { get; set; }
        public DbSet<AssignPosition> AssignPositions { get; set; }

        public DbSet<Election> Elections { get; set; }

        public DbSet<Vote> Votes {  get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
