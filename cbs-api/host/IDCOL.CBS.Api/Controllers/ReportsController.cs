using IDCOL.CBS.Classification.Domain;
using IDCOL.CBS.Collection.Domain;
using IDCOL.CBS.CreditSanction.Domain;
using IDCOL.CBS.Disbursement.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.RepaymentEngine.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.Api.Controllers;

/// <summary>
/// The CAD / F&amp;A reporting read side. Reports are pure read-only projections over the loan
/// portfolio (sanctions, facilities/schedules, disbursements, collections, classification), so
/// they read the shared read model directly rather than going through the command/MediatR path.
/// Every endpoint returns the same <see cref="ReportResult"/> envelope (title + typed columns +
/// row dictionaries + totals) so a single Angular grid can render any report - mirroring the eFS
/// CAD report screens' shared filter/grid shell.
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly LoanLifecycleDbContext _db;

    public ReportsController(LoanLifecycleDbContext db) => _db = db;

    /// <summary>Catalogue of available reports, for the report picker.</summary>
    [HttpGet("catalog")]
    public IActionResult Catalog() => Ok(new[]
    {
        new { key = "sanction", name = "Sanction Report", group = "CAD", description = "All loan agreements with terms and status." },
        new { key = "disbursement", name = "Disbursement Statement", group = "CAD", description = "Posted (processed) disbursements over a period." },
        new { key = "due", name = "Principal & Interest Due", group = "CAD", description = "Scheduled installments falling due in a period." },
        new { key = "collection", name = "Borrower Payment Info", group = "CAD", description = "Collection receipts with principal / interest / LPC split." },
        new { key = "classification", name = "Classification & Provisioning", group = "CAD", description = "Latest DFIM 04/2021 classification run." },
        new { key = "principal-movement", name = "Principal Movement", group = "CAD", description = "Disbursed vs repaid principal per borrower." },
        new { key = "reschedule", name = "Reschedule & Restructure Return", group = "CAD", description = "Every reschedule/restructure/prepayment event." },
        new { key = "provisioning", name = "Provisioning Summary (F&A)", group = "F&A", description = "Provision required by classification status." },
    });

    // ---- CAD reports ----

    [HttpGet("sanction")]
    public async Task<IActionResult> Sanction(CancellationToken ct)
    {
        var entities = await _db.Set<LoanAgreement>().AsNoTracking()
            .OrderBy(a => a.SanctionId).ToListAsync(ct);
        var rows = entities.Select(a => new Dictionary<string, object?>
        {
            ["borrower"] = a.CustomerNo,
            ["project"] = a.ProjectName,
            ["creditNo"] = a.SanctionId,
            ["currency"] = a.LoanCurrency,
            ["sanctionAmount"] = a.LoanAmount,
            ["tenorMonths"] = a.LoanTenorMonths,
            ["ratePercent"] = a.InitialInterestRatePercent,
            ["agreementDate"] = a.AgreementDate.ToString("yyyy-MM-dd"),
            ["status"] = a.Status,
        }).ToList();

        return Ok(new ReportResult("Sanction Report", new[]
        {
            Col("borrower", "Borrower", "text"), Col("project", "Project", "text"),
            Col("creditNo", "Credit No", "text"), Col("currency", "Ccy", "text"),
            Col("sanctionAmount", "Sanction Amount", "money"), Col("tenorMonths", "Tenor (mo)", "int"),
            Col("ratePercent", "Rate %", "rate"), Col("agreementDate", "Agreement", "date"),
            Col("status", "Status", "text"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["sanctionAmount"] = rows.Sum(r => (decimal)(r["sanctionAmount"] ?? 0m)),
        }));
    }

    [HttpGet("disbursement")]
    public async Task<IActionResult> Disbursement([FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        var (fromD, toD) = ParseRange(from, to);
        var list = await _db.Set<DisbursementRequest>().AsNoTracking()
            .Where(d => d.Status == "Processed")
            .ToListAsync(ct);

        var rows = list
            .Where(d => d.ValueDate == null || (d.ValueDate >= fromD && d.ValueDate <= toD))
            .OrderBy(d => d.SanctionRef).ThenBy(d => d.DisbursementNo)
            .Select(d => new Dictionary<string, object?>
            {
                ["borrower"] = d.CustomerNo,
                ["project"] = d.ProjectName,
                ["creditNo"] = d.SanctionRef,
                ["currency"] = d.LoanCurrency,
                ["disbursementNo"] = d.DisbursementNo,
                ["mode"] = d.DisbursementMode,
                ["valueDate"] = d.ValueDate?.ToString("yyyy-MM-dd"),
                ["amount"] = d.EffectiveLoanAmount,
            }).ToList();

        return Ok(new ReportResult("Disbursement Statement", new[]
        {
            Col("borrower", "Borrower", "text"), Col("project", "Project", "text"),
            Col("creditNo", "Credit No", "text"), Col("currency", "Ccy", "text"),
            Col("disbursementNo", "Disb. No", "int"), Col("mode", "Mode", "text"),
            Col("valueDate", "Disb. Date", "date"), Col("amount", "Disb. Amount", "money"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["amount"] = rows.Sum(r => (decimal)(r["amount"] ?? 0m)),
        }));
    }

    [HttpGet("due")]
    public async Task<IActionResult> Due([FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        var (fromD, toD) = ParseRange(from, to, defaultFrom: new DateOnly(2026, 1, 1), defaultTo: new DateOnly(2026, 12, 31));
        var fromSerial = SerialDate.FromIso(fromD.ToString("yyyy-MM-dd"))!.Value;
        var toSerial = SerialDate.FromIso(toD.ToString("yyyy-MM-dd"))!.Value;

        var agreements = await _db.Set<LoanAgreement>().AsNoTracking().ToListAsync(ct);
        var byId = agreements.ToDictionary(a => a.Id);
        var facilities = await _db.Set<Facility>().Include(f => f.Versions).AsNoTracking().ToListAsync(ct);

        var rows = new List<Dictionary<string, object?>>();
        foreach (var f in facilities.Where(f => f.LenderCode == "IDCOL"))
        {
            if (!byId.TryGetValue(f.SanctionId, out var a)) continue;
            List<ScheduleRow> schedule;
            try { schedule = f.CurrentVersion.ComputeSchedule(); }
            catch { continue; }

            foreach (var r in schedule.Where(r => r.PayDate >= fromSerial && r.PayDate <= toSerial && (r.Principal > 0.005 || r.Interest > 0.005)))
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["borrower"] = a.CustomerNo,
                    ["project"] = a.ProjectName,
                    ["currency"] = a.LoanCurrency,
                    ["paymentDate"] = SerialDate.ToIso(r.PayDate),
                    ["principal"] = Math.Round((decimal)r.Principal, 2),
                    ["interest"] = Math.Round((decimal)r.CashInterest, 2),
                    ["capitalized"] = Math.Round((decimal)r.CapInterest, 2),
                    ["total"] = Math.Round((decimal)(r.Principal + r.CashInterest + r.CapInterest), 2),
                });
            }
        }
        rows = rows.OrderBy(r => r["borrower"]).ThenBy(r => r["paymentDate"]).ToList();

        return Ok(new ReportResult("Principal & Interest Due Report", new[]
        {
            Col("borrower", "Borrower", "text"), Col("project", "Project", "text"),
            Col("currency", "Ccy", "text"), Col("paymentDate", "Payment Date", "date"),
            Col("principal", "Principal", "money"), Col("interest", "Interest", "money"),
            Col("capitalized", "Capitalized", "money"), Col("total", "Total", "money"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["principal"] = rows.Sum(r => (decimal)(r["principal"] ?? 0m)),
            ["interest"] = rows.Sum(r => (decimal)(r["interest"] ?? 0m)),
            ["total"] = rows.Sum(r => (decimal)(r["total"] ?? 0m)),
        }));
    }

    [HttpGet("collection")]
    public async Task<IActionResult> Collection([FromQuery] string? from, [FromQuery] string? to, CancellationToken ct)
    {
        var (fromD, toD) = ParseRange(from, to);
        var list = await _db.Set<Receipt>().AsNoTracking().ToListAsync(ct);

        var rows = list
            .Where(r => r.ReceiveDate >= fromD && r.ReceiveDate <= toD)
            .OrderBy(r => r.CustomerNo).ThenBy(r => r.ReceiveDate)
            .Select(r => new Dictionary<string, object?>
            {
                ["borrower"] = r.CustomerNo,
                ["project"] = r.ProjectName,
                ["currency"] = r.Currency,
                ["receiveDate"] = r.ReceiveDate.ToString("yyyy-MM-dd"),
                ["mode"] = r.PaymentMode,
                ["principal"] = r.PrincipalAmount,
                ["interest"] = r.InterestAmount,
                ["lpc"] = r.LpcAmount,
                ["amount"] = r.InstrumentAmount,
                ["status"] = r.Status,
            }).ToList();

        return Ok(new ReportResult("Borrower Payment Information", new[]
        {
            Col("borrower", "Borrower", "text"), Col("project", "Project", "text"),
            Col("currency", "Ccy", "text"), Col("receiveDate", "Receive Date", "date"),
            Col("mode", "Mode", "text"), Col("principal", "Principal", "money"),
            Col("interest", "Interest", "money"), Col("lpc", "LPC", "money"),
            Col("amount", "Amount", "money"), Col("status", "Status", "text"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["principal"] = rows.Sum(r => (decimal)(r["principal"] ?? 0m)),
            ["interest"] = rows.Sum(r => (decimal)(r["interest"] ?? 0m)),
            ["lpc"] = rows.Sum(r => (decimal)(r["lpc"] ?? 0m)),
            ["amount"] = rows.Sum(r => (decimal)(r["amount"] ?? 0m)),
        }));
    }

    [HttpGet("classification")]
    public async Task<IActionResult> Classification(CancellationToken ct)
    {
        var all = await _db.Set<LoanClassification>().AsNoTracking().ToListAsync(ct);
        var latestRun = all.OrderByDescending(c => c.AsOfDate).ThenByDescending(c => c.CreatedAtUtc)
            .Select(c => c.RunId).FirstOrDefault();
        var rows = all.Where(c => c.RunId == latestRun)
            .OrderBy(c => c.CustomerNo)
            .Select(c => new Dictionary<string, object?>
            {
                ["borrower"] = c.CustomerNo,
                ["project"] = c.ProjectName,
                ["currency"] = c.Currency,
                ["financeType"] = c.FinanceType,
                ["tenorBucket"] = c.TenorBucket,
                ["overdueMonths"] = c.OverdueMonths,
                ["status"] = c.Status,
                ["outstanding"] = c.OutstandingAmount,
                ["suspense"] = c.InterestSuspense,
                ["provisionRate"] = c.ProvisionRatePercent,
                ["provisionRequired"] = c.ProvisionRequired,
            }).ToList();

        return Ok(new ReportResult("Classification & Provisioning Report", new[]
        {
            Col("borrower", "Borrower", "text"), Col("project", "Project", "text"),
            Col("currency", "Ccy", "text"), Col("financeType", "Finance Type", "text"),
            Col("tenorBucket", "Tenor", "text"), Col("overdueMonths", "Overdue (mo)", "int"),
            Col("status", "Classification", "status"), Col("outstanding", "Outstanding", "money"),
            Col("suspense", "Int. Suspense", "money"), Col("provisionRate", "Prov. %", "rate"),
            Col("provisionRequired", "Provision", "money"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["outstanding"] = rows.Sum(r => (decimal)(r["outstanding"] ?? 0m)),
            ["provisionRequired"] = rows.Sum(r => (decimal)(r["provisionRequired"] ?? 0m)),
        }));
    }

    [HttpGet("principal-movement")]
    public async Task<IActionResult> PrincipalMovement(CancellationToken ct)
    {
        var disb = await _db.Set<DisbursementRequest>().AsNoTracking().Where(d => d.Status == "Processed").ToListAsync(ct);
        var receipts = await _db.Set<Receipt>().AsNoTracking().Where(r => r.Status == "Verified").ToListAsync(ct);

        var disbursedBy = disb.GroupBy(d => d.SanctionRef).ToDictionary(g => g.Key, g => new { g.First().CustomerNo, g.First().ProjectName, Sum = g.Sum(x => x.EffectiveLoanAmount) });
        var repaidBy = receipts.GroupBy(r => r.SanctionRef).ToDictionary(g => g.Key, g => g.Sum(x => x.PrincipalAmount));

        var rows = disbursedBy.OrderBy(kv => kv.Key).Select(kv =>
        {
            var repaid = repaidBy.TryGetValue(kv.Key, out var rp) ? rp : 0m;
            return new Dictionary<string, object?>
            {
                ["borrower"] = kv.Value.CustomerNo,
                ["project"] = kv.Value.ProjectName,
                ["creditNo"] = kv.Key,
                ["disbursed"] = kv.Value.Sum,
                ["repaid"] = repaid,
                ["outstanding"] = kv.Value.Sum - repaid,
            };
        }).ToList();

        return Ok(new ReportResult("Principal Movement Report", new[]
        {
            Col("borrower", "Borrower", "text"), Col("project", "Project", "text"),
            Col("creditNo", "Credit No", "text"), Col("disbursed", "Disbursed", "money"),
            Col("repaid", "Principal Repaid", "money"), Col("outstanding", "Outstanding", "money"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["disbursed"] = rows.Sum(r => (decimal)(r["disbursed"] ?? 0m)),
            ["repaid"] = rows.Sum(r => (decimal)(r["repaid"] ?? 0m)),
            ["outstanding"] = rows.Sum(r => (decimal)(r["outstanding"] ?? 0m)),
        }));
    }

    [HttpGet("reschedule")]
    public async Task<IActionResult> Reschedule(CancellationToken ct)
    {
        var agreements = await _db.Set<LoanAgreement>().AsNoTracking().ToListAsync(ct);
        var byId = agreements.ToDictionary(a => a.Id, a => a);
        var facilities = await _db.Set<Facility>().Include(f => f.Versions).AsNoTracking().ToListAsync(ct);

        var rows = new List<Dictionary<string, object?>>();
        foreach (var f in facilities)
        {
            byId.TryGetValue(f.SanctionId, out var a);
            foreach (var v in f.Versions.Where(v => v.EventType != FacilityVersionEventType.Original).OrderBy(v => v.EffectiveDate))
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["borrower"] = a?.CustomerNo ?? f.LenderCode,
                    ["lender"] = f.LenderCode,
                    ["eventType"] = v.EventType.ToString(),
                    ["label"] = v.Label,
                    ["effectiveDate"] = v.EffectiveDate.ToString("yyyy-MM-dd"),
                    ["rateBefore"] = v.RateBeforePercent,
                    ["rateAfter"] = v.RateAfterPercent,
                    ["capitalized"] = v.CapitalizedAmount,
                    ["overdueRolledIn"] = v.OverdueAmountRolledIn,
                    ["regulatoryRef"] = v.RegulatoryReference,
                });
            }
        }
        rows = rows.OrderBy(r => r["borrower"]).ThenBy(r => r["effectiveDate"]).ToList();

        return Ok(new ReportResult("Reschedule & Restructure Return", new[]
        {
            Col("borrower", "Borrower", "text"), Col("lender", "Lender", "text"),
            Col("eventType", "Event", "text"), Col("label", "Label", "text"),
            Col("effectiveDate", "Effective", "date"), Col("rateBefore", "Rate Before %", "rate"),
            Col("rateAfter", "Rate After %", "rate"), Col("capitalized", "Capitalized", "money"),
            Col("overdueRolledIn", "Overdue Rolled-In", "money"), Col("regulatoryRef", "Reg. Ref", "text"),
        }, rows, new Dictionary<string, object?>
        {
            ["count"] = rows.Count,
            ["capitalized"] = rows.Sum(r => (decimal)(r["capitalized"] ?? 0m)),
        }));
    }

    [HttpGet("provisioning")]
    public async Task<IActionResult> Provisioning(CancellationToken ct)
    {
        var all = await _db.Set<LoanClassification>().AsNoTracking().ToListAsync(ct);
        var latestRun = all.OrderByDescending(c => c.AsOfDate).ThenByDescending(c => c.CreatedAtUtc)
            .Select(c => c.RunId).FirstOrDefault();
        var run = all.Where(c => c.RunId == latestRun).ToList();

        var order = new[] { "Standard", "SMA", "Sub-Standard", "Doubtful", "Bad/Loss" };
        var rows = run.GroupBy(c => c.Status)
            .OrderBy(g => Array.IndexOf(order, g.Key))
            .Select(g => new Dictionary<string, object?>
            {
                ["status"] = g.Key,
                ["accounts"] = g.Count(),
                ["outstanding"] = g.Sum(c => c.OutstandingAmount),
                ["suspense"] = g.Sum(c => c.InterestSuspense),
                ["provisionRequired"] = g.Sum(c => c.ProvisionRequired),
            }).ToList();

        return Ok(new ReportResult("Provisioning Summary (F&A)", new[]
        {
            Col("status", "Classification", "status"), Col("accounts", "Accounts", "int"),
            Col("outstanding", "Outstanding", "money"), Col("suspense", "Int. Suspense", "money"),
            Col("provisionRequired", "Provision Required", "money"),
        }, rows, new Dictionary<string, object?>
        {
            ["accounts"] = rows.Sum(r => (int)(r["accounts"] ?? 0)),
            ["outstanding"] = rows.Sum(r => (decimal)(r["outstanding"] ?? 0m)),
            ["provisionRequired"] = rows.Sum(r => (decimal)(r["provisionRequired"] ?? 0m)),
        }));
    }

    private static ReportColumn Col(string key, string label, string kind) => new(key, label, kind);

    private static (DateOnly from, DateOnly to) ParseRange(
        string? from, string? to, DateOnly? defaultFrom = null, DateOnly? defaultTo = null)
    {
        var f = DateOnly.TryParse(from, out var fp) ? fp : (defaultFrom ?? new DateOnly(2000, 1, 1));
        var t = DateOnly.TryParse(to, out var tp) ? tp : (defaultTo ?? new DateOnly(2100, 12, 31));
        return (f, t);
    }
}

public sealed record ReportColumn(string Key, string Label, string Kind);

public sealed record ReportResult(
    string Title,
    IReadOnlyList<ReportColumn> Columns,
    IReadOnlyList<Dictionary<string, object?>> Rows,
    Dictionary<string, object?> Totals)
{
    public string GeneratedAtUtc { get; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
}
