using IDCOL.CBS.SystemAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence.Configurations;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("SYSAD_AUDIT_LOG_ENTRY");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("AUDIT_LOG_ENTRY_ID").ValueGeneratedNever();
        builder.Property(a => a.ActorUserId).HasColumnName("ACTOR_USER_ID").HasMaxLength(50).IsRequired();
        builder.Property(a => a.ActionName).HasColumnName("ACTION_NAME").HasMaxLength(200).IsRequired();
        builder.Property(a => a.EntityType).HasColumnName("ENTITY_TYPE").HasMaxLength(200);
        builder.Property(a => a.EntityId).HasColumnName("ENTITY_ID").HasMaxLength(100);
        builder.Property(a => a.DetailsJson).HasColumnName("DETAILS_JSON").HasColumnType("CLOB");
        builder.Property(a => a.OccurredAtUtc).HasColumnName("OCCURRED_AT_UTC");

        builder.HasIndex(a => a.OccurredAtUtc);
    }
}
