using IDCOL.CBS.CreditSanction.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class LoanAgreementConfiguration : IEntityTypeConfiguration<LoanAgreement>
{
    public void Configure(EntityTypeBuilder<LoanAgreement> builder)
    {
        builder.ToTable("CREDIT_LOAN_AGREEMENT");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("AGREEMENT_ID").ValueGeneratedNever();
        builder.Property(a => a.SanctionId).HasColumnName("SANCTION_ID").HasMaxLength(40).IsRequired();
        builder.HasIndex(a => a.SanctionId).IsUnique();
        builder.Property(a => a.Version).HasColumnName("VERSION");
        builder.Property(a => a.CustomerId).HasColumnName("CUSTOMER_ID");
        builder.Property(a => a.CustomerNo).HasColumnName("CUSTOMER_NO").HasMaxLength(30);
        builder.Property(a => a.ProductCode).HasColumnName("PRODUCT_CODE").HasMaxLength(30);
        builder.Property(a => a.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200).IsRequired();
        builder.Property(a => a.IndustryType).HasColumnName("INDUSTRY_TYPE").HasMaxLength(100);
        builder.Property(a => a.LoanCurrency).HasColumnName("LOAN_CURRENCY").HasMaxLength(3);
        builder.Property(a => a.LoanAmount).HasColumnName("LOAN_AMOUNT").HasPrecision(20, 2);
        builder.Property(a => a.GrantCurrency).HasColumnName("GRANT_CURRENCY").HasMaxLength(3);
        builder.Property(a => a.GrantAmount).HasColumnName("GRANT_AMOUNT").HasPrecision(20, 2);
        builder.Property(a => a.AgreementDate).HasColumnName("AGREEMENT_DATE");
        builder.Property(a => a.ExpiryDate).HasColumnName("EXPIRY_DATE");
        builder.Property(a => a.InterestRateType).HasColumnName("INTEREST_RATE_TYPE").HasMaxLength(20);
        builder.Property(a => a.InitialInterestRatePercent).HasColumnName("INITIAL_INTEREST_RATE_PERCENT").HasPrecision(9, 6);
        builder.Property(a => a.LoanTenorMonths).HasColumnName("LOAN_TENOR_MONTHS");
        builder.Property(a => a.NoOfPrincipalRepayments).HasColumnName("NO_OF_PRINCIPAL_REPAYMENTS");
        builder.Property(a => a.InterestGracePeriodMonths).HasColumnName("INTEREST_GRACE_PERIOD_MONTHS");
        builder.Property(a => a.PrincipalMoratoriumMonths).HasColumnName("PRINCIPAL_MORATORIUM_MONTHS");
        builder.Property(a => a.RepaymentMethod).HasColumnName("REPAYMENT_METHOD").HasMaxLength(50);
        builder.Property(a => a.PrincipalFrequency).HasColumnName("PRINCIPAL_FREQUENCY");
        builder.Property(a => a.InterestFrequency).HasColumnName("INTEREST_FREQUENCY");
        builder.Property(a => a.DayCountBasis).HasColumnName("DAY_COUNT_BASIS");
        builder.Property(a => a.LpcRatePercent).HasColumnName("LPC_RATE_PERCENT").HasPrecision(9, 6);
        builder.Property(a => a.CreditRating).HasColumnName("CREDIT_RATING").HasMaxLength(20);
        builder.Property(a => a.Status).HasColumnName("STATUS").HasMaxLength(20);
        builder.Property(a => a.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(a => a.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(a => a.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(a => a.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
    }
}
