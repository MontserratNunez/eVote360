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
    public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
    {
        public void Configure(EntityTypeBuilder<Candidate> builder)
        {
            builder.ToTable("Candidates");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.PhotoPath)
                .IsRequired();

            builder.Property(c => c.Status)
                .IsRequired();

            builder.HasOne(c => c.PoliticalParty)
                .WithMany()
                .HasForeignKey(c => c.PoliticalPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ElectivePosition)
                .WithMany()
                .HasForeignKey(c => c.ElectivePositionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
