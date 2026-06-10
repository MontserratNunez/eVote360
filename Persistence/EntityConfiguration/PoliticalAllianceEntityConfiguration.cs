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
    public class PoliticalAllianceEntityConfiguration : IEntityTypeConfiguration<PoliticalAlliance>
    {
        public void Configure(EntityTypeBuilder<PoliticalAlliance> builder)
        {
            builder.ToTable("PoliticalAlliances");

            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pa => pa.RequestDate)
                .IsRequired();

            builder.Property(pa => pa.ResponseDate)
                .IsRequired(false);

            builder.HasOne(pa => pa.RequestingPoliticalParty)
                .WithMany()
                .HasForeignKey(pa => pa.RequestingPoliticalPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pa => pa.RequestedPoliticalParty)
                .WithMany()
                .HasForeignKey(pa => pa.RequestedPoliticalPartyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}