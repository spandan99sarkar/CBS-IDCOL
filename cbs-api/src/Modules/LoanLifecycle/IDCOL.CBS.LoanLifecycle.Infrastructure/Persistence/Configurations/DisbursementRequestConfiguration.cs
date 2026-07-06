using IDCOL.CBS.Disbursement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class DisbursementRequestConfiguration : IEntityTypeConfiguration<DisbursementRequest>
{
    public void Configure(EntityTypeBuilder<DisbursementRequest> builder)
    {
        builder.ToTable("DISB_REQUEST");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("DISBURSEMENT_ID").ValueGeneratedNever();
        builder.Property(d => d.ReferenceNo).HasColumnName("REFERENCE_NO").HasMaxLength(40).IsRequired();
        builder.HasIndex(d => d.ReferenceNo).IsUnique();
        builder.Property(d => d.DisbursementNo).HasColumnName("DISBURSEMENT_NO");
        builder.Property(d => d.SanctionId).HasColumnName("SANCTION_ID");
        builder.Property(d => d.SanctionRef).HasColumnName("SANCTION_REF").HasMaxLength(40);
        builder.Property(d => d.CustomerNo).HasColumnName("CUSTOMER_NO").HasMaxLength(30);
        builder.Property(d => d.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200);
        builder.Property(d => d.LoanCurrency).HasColumnName("LOAN_CURRENCY").HasMaxLength(3);
        builder.Property(d => d.Status).HasColumnName("STATUS").HasMaxLength(20);

        builder.Property(d => d.SuggestedLoanAmount).HasColumnName("SUGGESTED_LOAN_AMOUNT").HasPrecision(20, 2);
        builder.Property(d => d.SuggestedGrantAmount).HasColumnName("SUGGESTED_GRANT_AMOUNT").HasPrecision(20, 2);
        builder.Property(d => d.BuRemarks).HasColumnName("BU_REMARKS").HasMaxLength(1000);
        builder.Property(d => d.InitiatedBy).HasColumnName("INITIATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(d => d.InitiatedAtUtc).HasColumnName("INITIATED_AT_UTC");

        builder.Property(d => d.JustifiedLoanAmount).HasColumnName("JUSTIFIED_LOAN_AMOUNT").HasPrecision(20, 2);
        builder.Property(d => d.JustifiedGrantAmount).HasColumnName("JUSTIFIED_GRANT_AMOUNT").HasPrecision(20, 2);
        builder.Property(d => d.CadRemarks).HasColumnName("CAD_REMARKS").HasMaxLength(1000);
        builder.Property(d => d.ProposedBy).HasColumnName("PROPOSED_BY").HasMaxLength(50);
        builder.Property(d => d.ProposedAtUtc).HasColumnName("PROPOSED_AT_UTC");

        builder.Property(d => d.DisbursementMode).HasColumnName("DISBURSEMENT_MODE").HasMaxLength(20);
        builder.Property(d => d.ValueDate).HasColumnName("VALUE_DATE");
        builder.Property(d => d.PostedBy).HasColumnName("POSTED_BY").HasMaxLength(50);
        builder.Property(d => d.PostedAtUtc).HasColumnName("POSTED_AT_UTC");

        builder.Property(d => d.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(d => d.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(d => d.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(d => d.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");

        builder.HasMany(d => d.GlLines)
            .WithOne()
            .HasForeignKey(l => l.DisbursementRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(DisbursementRequest.GlLines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class DisbursementGlLineConfiguration : IEntityTypeConfiguration<DisbursementGlLine>
{
    public void Configure(EntityTypeBuilder<DisbursementGlLine> builder)
    {
        builder.ToTable("DISB_GL_LINE");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("GL_LINE_ID").ValueGeneratedNever();
        builder.Property(l => l.DisbursementRequestId).HasColumnName("DISBURSEMENT_ID");
        builder.Property(l => l.GlCode).HasColumnName("GL_CODE").HasMaxLength(30).IsRequired();
        builder.Property(l => l.GlDescription).HasColumnName("GL_DESCRIPTION").HasMaxLength(200);
        builder.Property(l => l.Debit).HasColumnName("DEBIT").HasPrecision(20, 2);
        builder.Property(l => l.Credit).HasColumnName("CREDIT").HasPrecision(20, 2);
    }
}
