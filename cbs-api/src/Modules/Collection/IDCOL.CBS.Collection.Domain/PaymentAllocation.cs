namespace IDCOL.CBS.Collection.Domain;

/// <summary>How a received amount splits across the three obligation buckets.</summary>
public readonly record struct PaymentAllocation(decimal Lpc, decimal Interest, decimal Principal)
{
    public decimal Total => Lpc + Interest + Principal;
}

/// <summary>
/// The payment-application waterfall IDCOL follows (per BB guidance, confirmed in the loan
/// lifecycle notes): a received amount clears Late Payment Charge first, then Interest, then
/// Principal. Anything left over after all three dues are cleared falls to Principal (prepayment).
/// The order is a configurable product rule in the fuller design; this is the default.
/// </summary>
public static class PaymentWaterfall
{
    public static PaymentAllocation Allocate(decimal received, decimal dueLpc, decimal dueInterest, decimal duePrincipal)
    {
        if (received < 0) throw new ArgumentException("Received amount cannot be negative.", nameof(received));

        var remaining = received;

        var lpc = Math.Min(remaining, Math.Max(0, dueLpc));
        remaining -= lpc;

        var interest = Math.Min(remaining, Math.Max(0, dueInterest));
        remaining -= interest;

        // Everything still remaining goes to principal, including any excess beyond principal due
        // (an over-payment / prepayment reduces principal further).
        var principal = remaining;

        return new PaymentAllocation(lpc, interest, principal);
    }
}
