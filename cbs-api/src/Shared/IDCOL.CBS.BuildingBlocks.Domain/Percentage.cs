namespace IDCOL.CBS.BuildingBlocks.Domain;

/// <summary>
/// Stores a rate as its percent value (e.g. 9.5 for 9.5%), matching how rates are captured
/// throughout the source requirements (Interest Rate, LPC Rate, provisioning percentages).
/// </summary>
public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage(decimal value) => Value = value;

    public static Percentage Of(decimal percentValue)
    {
        if (percentValue < 0)
            throw new ArgumentException("A percentage cannot be negative.", nameof(percentValue));

        return new Percentage(percentValue);
    }

    public decimal AsDecimalFraction => Value / 100m;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:0.######}%";
}
