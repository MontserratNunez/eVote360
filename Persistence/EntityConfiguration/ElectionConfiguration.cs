using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Infrastructure.Persistence.EntityConfiguration
{

    public class ElectionConfiguration : IEntityTypeConfiguration<Election>
    {
        public void Configure(EntityTypeBuilder<Election> builder)
        {
            builder.ToTable("Elections");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Date)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .IsRequired();

            builder.Property(e => e.ActivatedDate);

            builder.Property(e => e.FinishedDate);

            builder.HasIndex(e => e.Status);
        }
    }

}
