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

    public class PoliticalLeaderAssignmentConfiguration : IEntityTypeConfiguration<PoliticalLeaderAssignment>
    {
        public void Configure(EntityTypeBuilder<PoliticalLeaderAssignment> builder)
        {
            builder.ToTable("PoliticalLeaderAssignments");

            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.User)
                .WithOne(u => u.PoliticalLeaderAssignment)
                .HasForeignKey<PoliticalLeaderAssignment>(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.PoliticalParty)
                .WithOne(p => p.PoliticalLeaderAssignment)
                .HasForeignKey<PoliticalLeaderAssignment>(a => a.PoliticalPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.UserId).IsUnique();
            builder.HasIndex(a => a.PoliticalPartyId).IsUnique();
        }
    }
}
