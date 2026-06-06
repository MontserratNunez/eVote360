using eVote360.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Infrastructure.Persistence.EntityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            #region Basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Users");
            #endregion

            #region Property configurations
            builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
            builder.Property(u => u.Password).IsRequired().HasMaxLength(int.MaxValue);
            builder.HasIndex(c => c.UserName).IsUnique();
            builder.HasIndex(c => c.Email).IsUnique();
            #endregion
        }
    }
}
