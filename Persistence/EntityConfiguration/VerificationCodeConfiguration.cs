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
    public class VerificationCodeConfiguration : IEntityTypeConfiguration<VerificationCode>
    {
        public void Configure(EntityTypeBuilder<VerificationCode> builder)
        {
            builder.ToTable("VerificationCodes");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Code)
                .IsRequired()
                .HasMaxLength(6);

            builder.Property(v => v.CreatedDate)
                .IsRequired();

            builder.Property(v => v.ExpirationDate)
                .IsRequired();

            builder.Property(v => v.Used)
                .IsRequired();

            builder.HasOne(v => v.Citizen)
                .WithMany()
                .HasForeignKey(v => v.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Election)
                .WithMany()
                .HasForeignKey(v => v.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(v => new
            {
                v.CitizenId,
                v.ElectionId,
                v.Code
            });
        }
    }
}
