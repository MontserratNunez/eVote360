using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Infrastructure.Persistence.EntityConfiguration
{

    public class AssignPositionConfiguration : IEntityTypeConfiguration<AssignPosition>
    {
        public void Configure(EntityTypeBuilder<AssignPosition> builder)
        {
            builder.ToTable("AssignPositions");

            builder.HasKey(x => x.Id);

            builder.HasOne(a => a.Candidate)
                .WithMany(c => c.AssignPositions)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ElectivePosition)
                .WithMany(p => p.AssignPositions)
                .HasForeignKey(a => a.ElectivePositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.PoliticalParty)
                .WithMany(p => p.AssignPositions)
                .HasForeignKey(a => a.PoliticalPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.IsAlliance)
                .IsRequired();

            builder.HasIndex(a => new
            {
                a.ElectionId,
                a.CandidateId,
                a.ElectivePositionId,
                a.PoliticalPartyId
            }).IsUnique();
        }
    }
}
