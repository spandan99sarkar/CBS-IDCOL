using IDCOL.CBS.Classification.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class ClassificationThresholdConfiguration : IEntityTypeConfiguration<ClassificationThreshold>
{
    public void Configure(EntityTypeBuilder<ClassificationThreshold> builder)
    {
        builder.ToTable("CLASS_THRESHOLD");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("THRESHOLD_ID").ValueGeneratedNever();
        builder.Property(t => t.FinanceType).HasColumnName("FINANCE_TYPE").HasMaxLength(20).IsRequired();
        builder.Property(t => t.TenorBucket).HasColumnName("TENOR_BUCKET").HasMaxLength(20);
        builder.Property(t => t.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired();
        builder.Property(t => t.MinOverdueMonths).HasColumnName("MIN_OVERDUE_MONTHS").HasPrecision(6, 2);
        builder.Property(t => t.MaxOverdueMonths).HasColumnName("MAX_OVERDUE_MONTHS").HasPrecision(6, 2);
        builder.Property(t => t.CircularRef).HasColumnName("CIRCULAR_REF").HasMaxLength(60);
        builder.Property(t => t.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
    }
}

public class ProvisioningRateConfiguration : IEntityTypeConfiguration<ProvisioningRate>
{
    public void Configure(EntityTypeBuilder<ProvisioningRate> builder)
    {
        builder.ToTable("CLASS_PROVISIONING_RATE");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("RATE_ID").ValueGeneratedNever();
        builder.Property(r => r.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired();
        builder.Property(r => r.IsCmsme).HasColumnName("IS_CMSME");
        builder.Property(r => r.ProvisionType).HasColumnName("PROVISION_TYPE").HasMaxLength(20);
        builder.Property(r => r.RatePercent).HasColumnName("RATE_PERCENT").HasPrecision(6, 2);
        builder.Property(r => r.CircularRef).HasColumnName("CIRCULAR_REF").HasMaxLength(60);
        builder.Property(r => r.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
    }
}

public class LoanClassificationConfiguration : IEntityTypeConfiguration<LoanClassification>
{
    public void Configure(EntityTypeBuilder<LoanClassification> builder)
    {
        builder.ToTable("CLASS_LOAN_CLASSIFICATION");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("CLASSIFICATION_ID").ValueGeneratedNever();
        builder.Property(c => c.RunId).HasColumnName("RUN_ID");
        builder.Property(c => c.AsOfDate).HasColumnName("AS_OF_DATE");
        builder.Property(c => c.AccountId).HasColumnName("ACCOUNT_ID");
        builder.Property(c => c.AccountRef).HasColumnName("ACCOUNT_REF").HasMaxLength(40);
        builder.Property(c => c.CustomerNo).HasColumnName("CUSTOMER_NO").HasMaxLength(30);
        builder.Property(c => c.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200);
        builder.Property(c => c.Currency).HasColumnName("CURRENCY").HasMaxLength(3);
        builder.Property(c => c.FinanceType).HasColumnName("FINANCE_TYPE").HasMaxLength(20);
        builder.Property(c => c.TenorMonths).HasColumnName("TENOR_MONTHS");
        builder.Property(c => c.TenorBucket).HasColumnName("TENOR_BUCKET").HasMaxLength(20);
        builder.Property(c => c.IsCmsme).HasColumnName("IS_CMSME");
        builder.Property(c => c.OutstandingAmount).HasColumnName("OUTSTANDING_AMOUNT").HasPrecision(20, 2);
        builder.Property(c => c.OverdueMonths).HasColumnName("OVERDUE_MONTHS").HasPrecision(6, 2);
        builder.Property(c => c.InterestSuspense).HasColumnName("INTEREST_SUSPENSE").HasPrecision(20, 2);
        builder.Property(c => c.EligibleCollateral).HasColumnName("ELIGIBLE_COLLATERAL").HasPrecision(20, 2);
        builder.Property(c => c.Status).HasColumnName("STATUS").HasMaxLength(20);
        builder.Property(c => c.IsQualitativeOverride).HasColumnName("IS_QUALITATIVE_OVERRIDE");
        builder.Property(c => c.QualitativeReason).HasColumnName("QUALITATIVE_REASON").HasMaxLength(500);
        builder.Property(c => c.ProvisionType).HasColumnName("PROVISION_TYPE").HasMaxLength(20);
        builder.Property(c => c.ProvisionRatePercent).HasColumnName("PROVISION_RATE_PERCENT").HasPrecision(6, 2);
        builder.Property(c => c.ProvisionBase).HasColumnName("PROVISION_BASE").HasPrecision(20, 2);
        builder.Property(c => c.ProvisionRequired).HasColumnName("PROVISION_REQUIRED").HasPrecision(20, 2);
        builder.Property(c => c.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(c => c.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(c => c.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(c => c.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
        builder.HasIndex(c => c.RunId);
    }
}
