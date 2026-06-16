using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Infrastructure.Persistence.EntityConfiguration
{
    public class VoteConfiguration : IEntityTypeConfiguration<Vote>
    {
        public void Configure(EntityTypeBuilder<Vote> builder)
        {
            builder.ToTable("Votes");

            builder.HasKey(v => v.Id);

            builder.HasOne(v => v.Citizen)
                .WithMany()
                .HasForeignKey(v => v.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Election)
                .WithMany()
                .HasForeignKey(v => v.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.ElectivePosition)
                .WithMany()
                .HasForeignKey(v => v.ElectivePositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Candidate)
                .WithMany()
                .HasForeignKey(v => v.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(v => v.CreatedDate)
                .IsRequired();

            builder.HasIndex(v => new
            {
                v.CitizenId,
                v.ElectionId,
                v.ElectivePositionId
            }).IsUnique();
        }
    }
}
