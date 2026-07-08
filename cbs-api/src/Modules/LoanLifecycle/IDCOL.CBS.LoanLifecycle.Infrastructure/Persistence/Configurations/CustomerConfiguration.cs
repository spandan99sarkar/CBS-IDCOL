using IDCOL.CBS.PartyKyc.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("PARTYKYC_CUSTOMER");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("CUSTOMER_ID").ValueGeneratedNever();
        builder.Property(c => c.CustomerNo).HasColumnName("CUSTOMER_NO").HasMaxLength(30).IsRequired();
        builder.HasIndex(c => c.CustomerNo).IsUnique();
        builder.Property(c => c.CustomerType).HasColumnName("CUSTOMER_TYPE").HasMaxLength(20);
        builder.Property(c => c.Name).HasColumnName("NAME").HasMaxLength(200).IsRequired();
        builder.Property(c => c.BusinessUnitCode).HasColumnName("BUSINESS_UNIT_CODE").HasMaxLength(30);
        builder.Property(c => c.Mobile).HasColumnName("MOBILE").HasMaxLength(30);
        builder.Property(c => c.Email).HasColumnName("EMAIL").HasMaxLength(200);
        builder.Property(c => c.SectorCode).HasColumnName("SECTOR_CODE").HasMaxLength(30);
        builder.Property(c => c.KycStatus).HasColumnName("KYC_STATUS").HasMaxLength(20);
        builder.Property(c => c.RiskLevel).HasColumnName("RISK_LEVEL").HasMaxLength(20);
        builder.Property(c => c.Source).HasColumnName("SOURCE").HasMaxLength(20);
        builder.Property(c => c.IsActive).HasColumnName("IS_ACTIVE");
        builder.Property(c => c.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(50).IsRequired();
        builder.Property(c => c.CreatedAtUtc).HasColumnName("CREATED_AT_UTC");
        builder.Property(c => c.LastModifiedBy).HasColumnName("LAST_MODIFIED_BY").HasMaxLength(50);
        builder.Property(c => c.LastModifiedAtUtc).HasColumnName("LAST_MODIFIED_AT_UTC");
    }
}
