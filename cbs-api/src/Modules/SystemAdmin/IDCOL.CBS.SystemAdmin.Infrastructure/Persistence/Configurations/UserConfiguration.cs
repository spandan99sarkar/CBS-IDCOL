using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("SYSAD_USER");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("USER_ID").ValueGeneratedNever();
        builder.Property(u => u.Username).HasColumnName("USERNAME").HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.DisplayName).HasColumnName("DISPLAY_NAME").HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasColumnName("EMAIL").HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(500).IsRequired();
        builder.Property(u => u.IsActive).HasColumnName("IS_ACTIVE");
        builder.Property(u => u.BusinessUnitCode).HasColumnName("BUSINESS_UNIT_CODE").HasMaxLength(30);
        builder.Property(u => u.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(u => u.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(u => u.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(u => u.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");

        builder.HasMany(u => u.RoleAssignments)
            .WithOne()
            .HasForeignKey(ra => ra.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(User.RoleAssignments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(u => u.Roles)
            .WithMany()
            .UsingEntity("SYSAD_USER_ROLE");
        builder.Metadata.FindNavigation(nameof(User.Roles))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
