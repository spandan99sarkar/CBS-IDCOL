using IDCOL.CBS.RepaymentEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("REPAY_FACILITY");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("FACILITY_ID").ValueGeneratedNever();
        builder.Property(f => f.SanctionId).HasColumnName("SANCTION_ID");
        builder.Property(f => f.LenderCode).HasColumnName("LENDER_CODE").HasMaxLength(30).IsRequired();
        builder.Property(f => f.Currency).HasColumnName("CURRENCY").HasMaxLength(3);
        builder.Property(f => f.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(f => f.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(f => f.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(f => f.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
        builder.HasIndex(f => f.SanctionId);

        builder.HasMany(f => f.Versions)
            .WithOne()
            .HasForeignKey(v => v.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Facility.Versions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class FacilityVersionConfiguration : IEntityTypeConfiguration<FacilityVersion>
{
    public void Configure(EntityTypeBuilder<FacilityVersion> builder)
    {
        builder.ToTable("REPAY_FACILITY_VERSION");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("FACILITY_VERSION_ID").ValueGeneratedNever();
        builder.Property(v => v.FacilityId).HasColumnName("FACILITY_ID");
        builder.Property(v => v.VersionSequence).HasColumnName("VERSION_SEQUENCE");
        builder.Property(v => v.EventType).HasColumnName("EVENT_TYPE").HasConversion<string>().HasMaxLength(30);
        builder.Property(v => v.Status).HasColumnName("STATUS").HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
        builder.Property(v => v.Label).HasColumnName("LABEL").HasMaxLength(200).IsRequired();
        builder.Property(v => v.SourceFile).HasColumnName("SOURCE_FILE").HasMaxLength(200);
        builder.Property(v => v.RateBeforePercent).HasColumnName("RATE_BEFORE_PERCENT").HasPrecision(9, 6);
        builder.Property(v => v.RateAfterPercent).HasColumnName("RATE_AFTER_PERCENT").HasPrecision(9, 6);
        builder.Property(v => v.TenorMonthsBefore).HasColumnName("TENOR_MONTHS_BEFORE");
        builder.Property(v => v.TenorMonthsAfter).HasColumnName("TENOR_MONTHS_AFTER");
        builder.Property(v => v.CapitalizedAmount).HasColumnName("CAPITALIZED_AMOUNT").HasPrecision(20, 2);
        builder.Property(v => v.WaivedAmount).HasColumnName("WAIVED_AMOUNT").HasPrecision(20, 2);
        builder.Property(v => v.OverdueAmountRolledIn).HasColumnName("OVERDUE_AMOUNT_ROLLED_IN").HasPrecision(20, 2);
        builder.Property(v => v.RegulatoryReference).HasColumnName("REGULATORY_REFERENCE").HasMaxLength(300);
        // Unbounded on purpose (maps to CLOB on Oracle, TEXT on SQLite) - a schedule's serialized
        // parameters (disbursements, rate-change events, per-installment overrides) can be large.
        builder.Property(v => v.ParametersJson).HasColumnName("PARAMETERS_JSON").IsRequired();
        builder.Property(v => v.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(v => v.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(v => v.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(v => v.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
        builder.HasIndex(v => new { v.FacilityId, v.VersionSequence }).IsUnique();
    }
}
