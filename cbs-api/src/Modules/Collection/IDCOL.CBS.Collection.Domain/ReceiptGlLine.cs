using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Collection.Domain;

/// <summary>One debit/credit line of the GL posting produced when Accounts verifies a receipt.</summary>
public class ReceiptGlLine : Entity<Guid>
{
    public Guid ReceiptId { get; private set; }
    public string GlCode { get; private set; } = default!;
    public string GlDescription { get; private set; } = default!;
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    private ReceiptGlLine()
    {
    }

    public static ReceiptGlLine Create(
        Guid id, Guid receiptId, string glCode, string glDescription, decimal debit, decimal credit)
    {
        if (debit < 0 || credit < 0) throw new ArgumentException("GL amounts cannot be negative.");
        if (debit > 0 && credit > 0) throw new ArgumentException("A GL line is either a debit or a credit, not both.");

        return new ReceiptGlLine
        {
            Id = id,
            ReceiptId = receiptId,
            GlCode = glCode,
            GlDescription = glDescription,
            Debit = debit,
            Credit = credit,
        };
    }
}
