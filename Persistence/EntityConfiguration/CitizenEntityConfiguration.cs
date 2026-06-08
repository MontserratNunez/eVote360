using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Infrastructure.Persistence.EntityConfigurations
{
    public class CitizenEntityConfiguration : IEntityTypeConfiguration<Citizen>
    {
        public void Configure(EntityTypeBuilder<Citizen> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("Citizens");

            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
            builder.Property(c => c.LastName).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
            builder.Property(c => c.DocumentNumber).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Status).IsRequired();

            builder.HasIndex(c => c.Email).IsUnique();
            builder.HasIndex(c => c.DocumentNumber).IsUnique();
        }
    }
}