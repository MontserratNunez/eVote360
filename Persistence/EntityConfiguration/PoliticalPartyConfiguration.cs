using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace eVote360.Infrastructure.Persistence.EntityConfigurations
{
    public class PoliticalPartyConfiguration : IEntityTypeConfiguration<PoliticalParty>
    {
        public void Configure(EntityTypeBuilder<PoliticalParty> builder)
        {
            builder.ToTable("PoliticalParties");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(p => p.Acronym)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(p => p.LogoPath)
                .IsRequired();

            builder.Property(p => p.Status)
                .IsRequired();

            builder.HasIndex(p => p.Name).IsUnique();
            builder.HasIndex(p => p.Acronym).IsUnique();
        }
    }
}