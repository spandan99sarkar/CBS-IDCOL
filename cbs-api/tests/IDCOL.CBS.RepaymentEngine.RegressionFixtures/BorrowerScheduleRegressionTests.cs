using System.Text.Json;
using IDCOL.CBS.RepaymentEngine.Domain;

namespace IDCOL.CBS.RepaymentEngine.RegressionFixtures;

/// <summary>
/// Proves the C# repayment engine reproduces the validated reference engine's output for every
/// one of IDCOL's 19 real borrower schedules (original loans, reschedules, restructures,
/// prepayments, capitalization, floating rates, and the reverse-engineered workbook overrides).
/// This is the hard CI gate the architecture plan calls for before the engine is wired into
/// disbursement.
/// </summary>
public class BorrowerScheduleRegressionTests
{
    // Same-algorithm port, so results should match to double precision. Tolerance allows 1 paisa
    // absolute, scaled up by 1e-9 relative for billion-scale balances where floating-point
    // operation-ordering (LINQ Sum vs JS reduce) can differ in the last significant digit.
    private static double Tolerance(double expected) => Math.Max(0.01, 1e-9 * Math.Abs(expected));

    private static readonly JsonSerializerOptions ParamsOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private static readonly JsonSerializerOptions RootOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static string GoldenDir =>
        Path.Combine(AppContext.BaseDirectory, "GoldenData");

    public static IEnumerable<object[]> Borrowers()
    {
        foreach (var file in Directory.EnumerateFiles(GoldenDir, "*.json"))
        {
            if (Path.GetFileName(file).StartsWith("_")) continue; // skip _manifest.json
            yield return new object[] { Path.GetFileNameWithoutExtension(file) };
        }
    }

    [Theory]
    [MemberData(nameof(Borrowers))]
    public void Engine_reproduces_reference_schedule(string borrowerKey)
    {
        var path = Path.Combine(GoldenDir, borrowerKey + ".json");
        var json = File.ReadAllText(path);

        // Two-pass parse: the root/rows use camelCase, the params block uses snake_case.
        using var doc = JsonDocument.Parse(json);
        var expectedRows = doc.RootElement.GetProperty("rows")
            .Deserialize<List<GoldenRow>>(RootOptions)!;
        var goldenParams = doc.RootElement.GetProperty("params")
            .Deserialize<GoldenParams>(ParamsOptions)!;

        var actual = RepaymentScheduleEngine.Generate(goldenParams.ToDomain());

        Assert.True(
            actual.Count == expectedRows.Count,
            $"{borrowerKey}: row count {actual.Count} != expected {expectedRows.Count}");

        for (var i = 0; i < expectedRows.Count; i++)
        {
            var e = expectedRows[i];
            var a = actual[i];
            AssertClose(borrowerKey, i, "openingBal", e.OpeningBal, a.OpeningBal);
            AssertClose(borrowerKey, i, "interest", e.Interest, a.Interest);
            AssertClose(borrowerKey, i, "cashInterest", e.CashInterest, a.CashInterest);
            AssertClose(borrowerKey, i, "capInterest", e.CapInterest, a.CapInterest);
            AssertClose(borrowerKey, i, "principal", e.Principal, a.Principal);
            AssertClose(borrowerKey, i, "tds", e.Tds, a.Tds);
            AssertClose(borrowerKey, i, "closingBal", e.ClosingBal, a.ClosingBal);
            Assert.True(e.Days == a.Days, $"{borrowerKey} row {i} days: expected {e.Days}, got {a.Days}");
        }
    }

    private static void AssertClose(string key, int row, string field, double expected, double actual)
    {
        var diff = Math.Abs(expected - actual);
        Assert.True(
            diff <= Tolerance(expected),
            $"{key} row {row} {field}: expected {expected:R}, got {actual:R} (diff {diff:R})");
    }

    [Fact]
    public void All_nineteen_borrowers_are_present()
    {
        var count = Directory.EnumerateFiles(GoldenDir, "*.json")
            .Count(f => !Path.GetFileName(f).StartsWith("_"));
        Assert.Equal(19, count);
    }
}
