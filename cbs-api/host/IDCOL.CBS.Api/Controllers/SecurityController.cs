using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.Security.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.Api.Controllers;

/// <summary>
/// Loan Security &amp; Covenant register - IDCOL's collateral instruments (BG, FDR, MTDR, DSRA, land
/// mortgage, insurance, credit rating, financial statements, PDC, monitoring fee) and covenant
/// obligations, with the expiry-driven "recommended action" engine that powers the security
/// dashboard and reminder-letter generation.
/// </summary>
[ApiController]
[Route("api/security")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly LoanLifecycleDbContext _db;

    public SecurityController(LoanLifecycleDbContext db) => _db = db;

    // Fixed reference "today" for the dev dataset so days-left buckets are deterministic.
    private static readonly DateOnly DefaultAsOf = new(2026, 7, 7);

    /// <summary>The security &amp; covenant dashboard grid with computed days-left, recommended action and eligibility.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? category, [FromQuery] string? family, [FromQuery] string? asOf, CancellationToken ct)
    {
        var asOfDate = DateOnly.TryParse(asOf, out var d) ? d : DefaultAsOf;
        var items = await _db.SecurityInstruments.AsNoTracking().ToListAsync(ct);

        var rows = items
            .Where(i => category is null || i.Category == category)
            .Where(i => family is null || i.InstrumentFamily == family)
            .OrderBy(i => i.DaysLeft(asOfDate) ?? int.MaxValue)
            .Select(i => ToDto(i, asOfDate))
            .ToList();

        return Ok(rows);
    }

    /// <summary>Dashboard summary: counts by lifecycle state + expiry buckets + total eligible security.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] string? asOf, CancellationToken ct)
    {
        var asOfDate = DateOnly.TryParse(asOf, out var d) ? d : DefaultAsOf;
        var items = await _db.SecurityInstruments.AsNoTracking().ToListAsync(ct);
        var withExpiry = items.Where(i => i.ExpiryDate.HasValue).ToList();

        return Ok(new
        {
            asOf = asOfDate.ToString("yyyy-MM-dd"),
            total = items.Count,
            security = items.Count(i => i.Category == SecurityCategory.Security),
            covenant = items.Count(i => i.Category == SecurityCategory.Covenant),
            expired = withExpiry.Count(i => i.DaysLeft(asOfDate) < 0),
            expiringIn30 = withExpiry.Count(i => i.DaysLeft(asOfDate) is >= 0 and <= 30),
            expiringIn90 = withExpiry.Count(i => i.DaysLeft(asOfDate) is > 30 and <= 90),
            totalEligibleSecurity = items.Sum(i => i.EligibleAmount),
            byFamily = InstrumentFamily.All
                .Select(f => new { family = f, count = items.Count(i => i.InstrumentFamily == f) })
                .Where(x => x.count > 0),
            actionsNeeded = items
                .Select(i => i.ComputeRecommendedAction(asOfDate))
                .Where(a => a != RecommendedAction.NoActionRequired)
                .GroupBy(a => a)
                .Select(g => new { action = g.Key, count = g.Count() }),
        });
    }

    /// <summary>The catalogue of ~30 templated letters, optionally filtered to one instrument family.</summary>
    [HttpGet("letters")]
    public IActionResult Letters([FromQuery] string? family)
    {
        var templates = family is null ? SecurityLetterCatalog.Templates : SecurityLetterCatalog.ForFamily(family);
        return Ok(templates.Select(t => new { t.Family, t.LetterType, t.Purpose }));
    }

    /// <summary>Renders a merged letter body for an instrument and letter type (document generation).</summary>
    [HttpPost("{id:guid}/letters/generate")]
    public async Task<IActionResult> GenerateLetter(Guid id, [FromBody] GenerateLetterRequest request, CancellationToken ct)
    {
        var instrument = await _db.SecurityInstruments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (instrument is null) return NotFound();

        var seq = Math.Abs(id.GetHashCode()) % 1000;
        var refNo = $"IDCOL/CAD/IF/{instrument.ClientName}/{Abbrev(request.LetterType)}/{DefaultAsOf:yyyy}/{DefaultAsOf:MM}/{seq:D3}";
        var body = SecurityLetterCatalog.Render(instrument, request.LetterType, refNo, DefaultAsOf);
        return Ok(new { refNo, letterType = request.LetterType, body });
    }

    /// <summary>The lightweight "Update Amount and Maturity Date" action (FDR/MTDR refresh).</summary>
    [HttpPatch("{id:guid}/amount")]
    public async Task<IActionResult> UpdateAmount(Guid id, [FromBody] UpdateAmountRequest request, CancellationToken ct)
    {
        var instrument = await _db.SecurityInstruments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (instrument is null) return NotFound();
        var expiry = DateOnly.TryParse(request.ExpiryDate, out var e) ? e : (DateOnly?)null;
        instrument.UpdateAmountAndMaturity(request.CurrentBalance, expiry, User.Identity?.Name ?? "system");
        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(instrument, DefaultAsOf));
    }

    private static object ToDto(SecurityCovenantInstrument i, DateOnly asOf) => new
    {
        i.Id,
        i.Category,
        i.InstrumentFamily,
        i.LoanType,
        i.ClientName,
        i.ProjectName,
        i.StatementName,
        i.InstrumentNumber,
        i.IssuingBank,
        i.IssuingBranch,
        i.Currency,
        i.LeafValueOrInitialAmount,
        i.CurrentBalance,
        issueDate = i.IssueDate?.ToString("yyyy-MM-dd"),
        expiryDate = i.ExpiryDate?.ToString("yyyy-MM-dd"),
        i.VerificationStatus,
        i.AutoRenewal,
        i.LifecycleState,
        i.ActionTaken,
        daysLeft = i.DaysLeft(asOf),
        recommendedAction = i.ComputeRecommendedAction(asOf),
        i.MarketValue,
        i.ForcedSaleValue,
        i.IdcolPortionPercent,
        i.EligibleSecurityPercent,
        forcedSaleValueIdcolPortion = i.ForcedSaleValueIdcolPortion,
        eligibleAmount = i.EligibleAmount,
        i.Provider,
        i.Rating,
        i.Location,
        i.CovenantType,
        i.ComplianceStatus,
        nextDueDate = i.NextDueDate?.ToString("yyyy-MM-dd"),
        i.Remarks,
    };

    private static string Abbrev(string letterType)
    {
        var parts = letterType.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Take(3).Select(p => char.ToUpperInvariant(p[0])));
    }
}

public sealed record GenerateLetterRequest(string LetterType);
public sealed record UpdateAmountRequest(decimal CurrentBalance, string? ExpiryDate);
