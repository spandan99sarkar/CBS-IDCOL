using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("SYSAD_ROLE");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("ROLE_ID").ValueGeneratedNever();
        builder.Property(r => r.Code).HasColumnName("CODE").HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.Code).IsUnique();
        builder.Property(r => r.Name).HasColumnName("NAME").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
        builder.Property(r => r.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(r => r.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(r => r.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(r => r.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");

        builder.HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity("SYSAD_ROLE_PERMISSION");
        // Permissions is a many-to-many skip navigation (not a plain collection navigation),
        // so it must be looked up via FindSkipNavigation, not FindNavigation.
        builder.Metadata.FindSkipNavigation(nameof(Role.Permissions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
