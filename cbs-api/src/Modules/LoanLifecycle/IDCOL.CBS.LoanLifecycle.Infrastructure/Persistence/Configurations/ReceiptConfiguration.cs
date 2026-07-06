using IDCOL.CBS.Collection.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("COLL_RECEIPT");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("RECEIPT_ID").ValueGeneratedNever();
        builder.Property(r => r.ReferenceNo).HasColumnName("REFERENCE_NO").HasMaxLength(40).IsRequired();
        builder.HasIndex(r => r.ReferenceNo).IsUnique();
        builder.Property(r => r.SanctionId).HasColumnName("SANCTION_ID");
        builder.Property(r => r.SanctionRef).HasColumnName("SANCTION_REF").HasMaxLength(40);
        builder.Property(r => r.CustomerNo).HasColumnName("CUSTOMER_NO").HasMaxLength(30);
        builder.Property(r => r.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200);
        builder.Property(r => r.Currency).HasColumnName("CURRENCY").HasMaxLength(3);
        builder.Property(r => r.PaymentMode).HasColumnName("PAYMENT_MODE").HasMaxLength(20);
        builder.Property(r => r.InstrumentNo).HasColumnName("INSTRUMENT_NO").HasMaxLength(50);
        builder.Property(r => r.BankName).HasColumnName("BANK_NAME").HasMaxLength(100);
        builder.Property(r => r.InstrumentAmount).HasColumnName("INSTRUMENT_AMOUNT").HasPrecision(20, 2);
        builder.Property(r => r.ValueDate).HasColumnName("VALUE_DATE");
        builder.Property(r => r.ReceiveDate).HasColumnName("RECEIVE_DATE");
        builder.Property(r => r.LpcDate).HasColumnName("LPC_DATE");
        builder.Property(r => r.PrincipalAmount).HasColumnName("PRINCIPAL_AMOUNT").HasPrecision(20, 2);
        builder.Property(r => r.InterestAmount).HasColumnName("INTEREST_AMOUNT").HasPrecision(20, 2);
        builder.Property(r => r.LpcAmount).HasColumnName("LPC_AMOUNT").HasPrecision(20, 2);
        builder.Property(r => r.Status).HasColumnName("STATUS").HasMaxLength(20);
        builder.Property(r => r.EnteredBy).HasColumnName("ENTERED_BY").HasMaxLength(50).IsRequired();
        builder.Property(r => r.EnteredAtUtc).HasColumnName("ENTERED_AT_UTC");
        builder.Property(r => r.VerifiedBy).HasColumnName("VERIFIED_BY").HasMaxLength(50);
        builder.Property(r => r.VerifiedAtUtc).HasColumnName("VERIFIED_AT_UTC");
        builder.Property(r => r.VerifyComment).HasColumnName("VERIFY_COMMENT").HasMaxLength(1000);
        builder.Property(r => r.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(r => r.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(r => r.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(r => r.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");

        builder.HasMany(r => r.GlLines)
            .WithOne()
            .HasForeignKey(l => l.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Receipt.GlLines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ReceiptGlLineConfiguration : IEntityTypeConfiguration<ReceiptGlLine>
{
    public void Configure(EntityTypeBuilder<ReceiptGlLine> builder)
    {
        builder.ToTable("COLL_RECEIPT_GL_LINE");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("GL_LINE_ID").ValueGeneratedNever();
        builder.Property(l => l.ReceiptId).HasColumnName("RECEIPT_ID");
        builder.Property(l => l.GlCode).HasColumnName("GL_CODE").HasMaxLength(30).IsRequired();
        builder.Property(l => l.GlDescription).HasColumnName("GL_DESCRIPTION").HasMaxLength(200);
        builder.Property(l => l.Debit).HasColumnName("DEBIT").HasPrecision(20, 2);
        builder.Property(l => l.Credit).HasColumnName("CREDIT").HasPrecision(20, 2);
    }
}
