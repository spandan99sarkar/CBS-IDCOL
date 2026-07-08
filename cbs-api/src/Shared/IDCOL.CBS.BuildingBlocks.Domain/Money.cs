namespace IDCOL.CBS.BuildingBlocks.Domain;

/// <summary>
/// Money is never a bare decimal in this codebase. Stored/posted amounts round to 2dp at the
/// currency boundary (see architecture plan Oracle schema strategy: NUMBER(20,2) columns) -
/// intermediate calculations that need more precision should stay in raw decimal until the
/// point of persistence, not round through this type repeatedly.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code (e.g. BDT, USD).", nameof(currency));

        return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => Of(0m, currency);

    public bool IsZero => Amount == 0m;

    public bool IsNegative => Amount < 0m;

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => Of(Amount * factor, Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on Money values in different currencies ({Currency} vs {other.Currency}).");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
