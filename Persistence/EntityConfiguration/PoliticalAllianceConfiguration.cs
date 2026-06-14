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
    public class PoliticalAllianceConfiguration : IEntityTypeConfiguration<PoliticalAlliance>
    {
        public void Configure(EntityTypeBuilder<PoliticalAlliance> builder)
        {
            builder.ToTable("PoliticalAlliances");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.RequestDate)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            builder.HasOne(a => a.RequestingParty)
                .WithMany(p => p.AlliancesSent)
                .HasForeignKey(a => a.RequestingPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ReceivingParty)
                .WithMany(p => p.AlliancesReceived)
                .HasForeignKey(a => a.ReceivingPartyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.RequestingPartyId, a.ReceivingPartyId })
                .IsUnique();
        }
    }
}
