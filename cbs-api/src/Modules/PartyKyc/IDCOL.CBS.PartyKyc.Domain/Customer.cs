using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.PartyKyc.Domain;

/// <summary>
/// A borrower/counterparty record. IDCOL's AML system is the system of record for KYC; this is a
/// local, queryable projection so Sanction/Account/Disbursement can reference customers without a
/// live AML call. <see cref="Source"/> records whether a row was synced from AML or captured
/// locally (until the AML integration lands, records are captured locally).
/// </summary>
public class Customer : AggregateRoot<Guid>, IAuditable
{
    public string CustomerNo { get; private set; } = default!;
    public string CustomerType { get; private set; } = default!; // Individual | Institutional | Joint
    public string Name { get; private set; } = default!;
    public string BusinessUnitCode { get; private set; } = default!;
    public string? Mobile { get; private set; }
    public string? Email { get; private set; }
    public string? SectorCode { get; private set; }
    public string KycStatus { get; private set; } = "Pending"; // Pending | Verified
    public string RiskLevel { get; private set; } = "Low"; // Low | Medium | High
    public string Source { get; private set; } = "Local"; // Local | AmlSync
    public bool IsActive { get; private set; } = true;

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private Customer()
    {
    }

    public static Customer Create(
        Guid id, string customerNo, string customerType, string name, string businessUnitCode,
        string? mobile, string? email, string? sectorCode, string kycStatus, string riskLevel,
        string source, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(customerNo)) throw new ArgumentException("Customer number is required.", nameof(customerNo));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Customer name is required.", nameof(name));

        return new Customer
        {
            Id = id,
            CustomerNo = customerNo.Trim(),
            CustomerType = customerType,
            Name = name.Trim(),
            BusinessUnitCode = businessUnitCode,
            Mobile = mobile,
            Email = email,
            SectorCode = sectorCode,
            KycStatus = string.IsNullOrWhiteSpace(kycStatus) ? "Pending" : kycStatus,
            RiskLevel = string.IsNullOrWhiteSpace(riskLevel) ? "Low" : riskLevel,
            Source = string.IsNullOrWhiteSpace(source) ? "Local" : source,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
