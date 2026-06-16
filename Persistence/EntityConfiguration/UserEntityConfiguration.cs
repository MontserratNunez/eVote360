using eVote360.Core.Application.Helpers;
using eVote360.Core.Domain.Common.Enums;
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

            #region Seed
            builder.HasData(new User
            {
                Id = 1,
                Name = "Admin",
                LastName = "Admin",
                Email = "Admin@admin.com",
                UserName = "Admin",
                Password = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                Role = Role.ADMIN,
                Status = true
            });
            #endregion
        }
    }
}
