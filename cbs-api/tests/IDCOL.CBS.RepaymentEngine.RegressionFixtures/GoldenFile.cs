using System.Text.Json.Serialization;
using IDCOL.CBS.RepaymentEngine.Application.Seeding;

namespace IDCOL.CBS.RepaymentEngine.RegressionFixtures;

/// <summary>Root of a golden fixture file: the exact input params plus the reference engine's rows.</summary>
public sealed class GoldenFile
{
    public string Key { get; set; } = "";
    public SnakeCaseScheduleParameters Params { get; set; } = new();
    public List<GoldenRow> Rows { get; set; } = new();
}

/// <summary>One expected row from the reference engine. camelCase to match the golden JSON.</summary>
public sealed class GoldenRow
{
    [JsonPropertyName("idx")] public int Idx { get; set; }
    [JsonPropertyName("payDate")] public double PayDate { get; set; }
    [JsonPropertyName("openingBal")] public double OpeningBal { get; set; }
    [JsonPropertyName("periodRate")] public double PeriodRate { get; set; }
    [JsonPropertyName("interest")] public double Interest { get; set; }
    [JsonPropertyName("cashInterest")] public double CashInterest { get; set; }
    [JsonPropertyName("capInterest")] public double CapInterest { get; set; }
    [JsonPropertyName("principal")] public double Principal { get; set; }
    [JsonPropertyName("tds")] public double Tds { get; set; }
    [JsonPropertyName("closingBal")] public double ClosingBal { get; set; }
    [JsonPropertyName("days")] public int Days { get; set; }
}
