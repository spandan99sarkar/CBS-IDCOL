using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Disbursement.Domain;

/// <summary>
/// One debit or credit line of the GL posting produced when Accounts posts a disbursement
/// (e.g. Dr Loan Account / Cr Bank). Seeds the GL journal the reporting/classification modules
/// will consume; total debits must equal total credits on the posted request.
/// </summary>
public class DisbursementGlLine : Entity<Guid>
{
    public Guid DisbursementRequestId { get; private set; }
    public string GlCode { get; private set; } = default!;
    public string GlDescription { get; private set; } = default!;
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    private DisbursementGlLine()
    {
    }

    public static DisbursementGlLine Create(
        Guid id, Guid disbursementRequestId, string glCode, string glDescription, decimal debit, decimal credit)
    {
        if (debit < 0 || credit < 0) throw new ArgumentException("GL amounts cannot be negative.");
        if (debit > 0 && credit > 0) throw new ArgumentException("A GL line is either a debit or a credit, not both.");

        return new DisbursementGlLine
        {
            Id = id,
            DisbursementRequestId = disbursementRequestId,
            GlCode = glCode,
            GlDescription = glDescription,
            Debit = debit,
            Credit = credit,
        };
    }
}
