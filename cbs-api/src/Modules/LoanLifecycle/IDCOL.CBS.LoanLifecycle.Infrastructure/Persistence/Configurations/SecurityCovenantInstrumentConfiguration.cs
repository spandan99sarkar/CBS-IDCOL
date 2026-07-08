using IDCOL.CBS.Security.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class SecurityCovenantInstrumentConfiguration : IEntityTypeConfiguration<SecurityCovenantInstrument>
{
    public void Configure(EntityTypeBuilder<SecurityCovenantInstrument> builder)
    {
        builder.ToTable("SEC_INSTRUMENT");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("INSTRUMENT_ID").ValueGeneratedNever();
        builder.Property(x => x.Category).HasColumnName("CATEGORY").HasMaxLength(20).IsRequired();
        builder.Property(x => x.InstrumentFamily).HasColumnName("INSTRUMENT_FAMILY").HasMaxLength(40).IsRequired();
        builder.Property(x => x.LoanType).HasColumnName("LOAN_TYPE").HasMaxLength(40);
        builder.Property(x => x.SanctionId).HasColumnName("SANCTION_ID");
        builder.Property(x => x.ClientName).HasColumnName("CLIENT_NAME").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200).IsRequired();
        builder.Property(x => x.StatementName).HasColumnName("STATEMENT_NAME").HasMaxLength(200);
        builder.Property(x => x.InstrumentNumber).HasColumnName("INSTRUMENT_NUMBER").HasMaxLength(80);
        builder.Property(x => x.IssuingBank).HasColumnName("ISSUING_BANK").HasMaxLength(150);
        builder.Property(x => x.IssuingBranch).HasColumnName("ISSUING_BRANCH").HasMaxLength(150);
        builder.Property(x => x.Currency).HasColumnName("CURRENCY").HasMaxLength(3);
        builder.Property(x => x.LeafValueOrInitialAmount).HasColumnName("LEAF_VALUE").HasPrecision(20, 2);
        builder.Property(x => x.CurrentBalance).HasColumnName("CURRENT_BALANCE").HasPrecision(20, 2);
        builder.Property(x => x.IssueDate).HasColumnName("ISSUE_DATE");
        builder.Property(x => x.ExpiryDate).HasColumnName("EXPIRY_DATE");
        builder.Property(x => x.VerificationStatus).HasColumnName("VERIFICATION_STATUS").HasMaxLength(20);
        builder.Property(x => x.AutoRenewal).HasColumnName("AUTO_RENEWAL");
        builder.Property(x => x.LifecycleState).HasColumnName("LIFECYCLE_STATE").HasMaxLength(30);
        builder.Property(x => x.ActionTaken).HasColumnName("ACTION_TAKEN").HasMaxLength(80);
        builder.Property(x => x.Remarks).HasColumnName("REMARKS").HasMaxLength(500);
        builder.Property(x => x.MarketValue).HasColumnName("MARKET_VALUE").HasPrecision(20, 2);
        builder.Property(x => x.ForcedSaleValue).HasColumnName("FORCED_SALE_VALUE").HasPrecision(20, 2);
        builder.Property(x => x.IdcolPortionPercent).HasColumnName("IDCOL_PORTION_PCT").HasPrecision(9, 4);
        builder.Property(x => x.EligibleSecurityPercent).HasColumnName("ELIGIBLE_SECURITY_PCT").HasPrecision(9, 4);
        builder.Property(x => x.Provider).HasColumnName("PROVIDER").HasMaxLength(150);
        builder.Property(x => x.Rating).HasColumnName("RATING").HasMaxLength(20);
        builder.Property(x => x.Location).HasColumnName("LOCATION").HasMaxLength(300);
        builder.Property(x => x.CovenantType).HasColumnName("COVENANT_TYPE").HasMaxLength(80);
        builder.Property(x => x.ComplianceStatus).HasColumnName("COMPLIANCE_STATUS").HasMaxLength(30);
        builder.Property(x => x.NextDueDate).HasColumnName("NEXT_DUE_DATE");
        builder.Property(x => x.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(x => x.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
        builder.HasIndex(x => x.SanctionId);
        builder.HasIndex(x => new { x.Category, x.InstrumentFamily });
    }
}
