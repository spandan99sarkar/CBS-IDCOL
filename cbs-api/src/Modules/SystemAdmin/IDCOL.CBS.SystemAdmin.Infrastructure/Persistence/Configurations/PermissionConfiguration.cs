using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("SYSAD_PERMISSION");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PERMISSION_ID").ValueGeneratedNever();
        builder.Property(p => p.Code).HasColumnName("CODE").HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
    }
}
