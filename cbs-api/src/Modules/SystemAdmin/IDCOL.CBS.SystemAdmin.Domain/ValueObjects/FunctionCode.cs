using IDCOL.CBS.BuildingBlocks.Domain;

namespace IDCOL.CBS.SystemAdmin.Domain.ValueObjects;

/// <summary>
/// Identifies a gated function for maker-checker purposes, e.g. "DISBURSEMENT_POST",
/// "PARAMETER_CHANGE", "ACCOUNT_OPEN". Kept as a simple normalized code rather than an enum so
/// new bounded contexts can register their own function codes without changing this module.
/// </summary>
public sealed class FunctionCode : ValueObject
{
    public string Value { get; }

    private FunctionCode(string value) => Value = value;

    public static FunctionCode Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Function code is required.", nameof(value));

        return new FunctionCode(value.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
