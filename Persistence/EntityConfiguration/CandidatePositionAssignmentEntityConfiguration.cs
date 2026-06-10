using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Infrastructure.Persistence.EntityConfiguration
{
    public class CandidatePositionAssignmentEntityConfiguration : IEntityTypeConfiguration<CandidatePositionAssignment>
    {
        public void Configure(EntityTypeBuilder<CandidatePositionAssignment> builder)
        {
            builder.ToTable("CandidatePositionAssignments");

            builder.HasKey(cpa => cpa.Id);

            builder.Property(cpa => cpa.IsAlliedCandidate)
                .IsRequired();

            builder.HasOne(cpa => cpa.Candidate)
                .WithMany()
                .HasForeignKey(cpa => cpa.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cpa => cpa.ElectivePosition)
                .WithMany()
                .HasForeignKey(cpa => cpa.ElectivePositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cpa => cpa.PoliticalParty)
                .WithMany()
                .HasForeignKey(cpa => cpa.PoliticalPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cpa => new { cpa.PoliticalPartyId, cpa.ElectivePositionId })
                .IsUnique();

            builder.HasIndex(cpa => new { cpa.PoliticalPartyId, cpa.CandidateId })
                .IsUnique();
        }
    }
}