using IDCOL.CBS.ProductConfig.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class LoanProductConfiguration : IEntityTypeConfiguration<LoanProduct>
{
    public void Configure(EntityTypeBuilder<LoanProduct> builder)
    {
        builder.ToTable("PRODCFG_LOAN_PRODUCT");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PRODUCT_ID").ValueGeneratedNever();
        builder.Property(p => p.ProductCode).HasColumnName("PRODUCT_CODE").HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.ProductCode).IsUnique();
        builder.Property(p => p.ProductName).HasColumnName("PRODUCT_NAME").HasMaxLength(200).IsRequired();
        builder.Property(p => p.ProductType).HasColumnName("PRODUCT_TYPE").HasMaxLength(50);
        builder.Property(p => p.InterestType).HasColumnName("INTEREST_TYPE").HasMaxLength(20);
        builder.Property(p => p.RepaymentMethod).HasColumnName("REPAYMENT_METHOD").HasMaxLength(50);
        builder.Property(p => p.DayCountBasis).HasColumnName("DAY_COUNT_BASIS");
        builder.Property(p => p.GracePeriodMonths).HasColumnName("GRACE_PERIOD_MONTHS");
        builder.Property(p => p.PrepaymentAllowed).HasColumnName("PREPAYMENT_ALLOWED");
        builder.Property(p => p.PenaltyAllowed).HasColumnName("PENALTY_ALLOWED");
        builder.Property(p => p.SuggestedRatePercent).HasColumnName("SUGGESTED_RATE_PERCENT").HasPrecision(9, 6);
        builder.Property(p => p.LowerRatePercent).HasColumnName("LOWER_RATE_PERCENT").HasPrecision(9, 6);
        builder.Property(p => p.UpperRatePercent).HasColumnName("UPPER_RATE_PERCENT").HasPrecision(9, 6);
        builder.Property(p => p.IsActive).HasColumnName("IS_ACTIVE");
        builder.Property(p => p.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(p => p.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(p => p.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(p => p.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
    }
}
