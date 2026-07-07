using IDCOL.CBS.Classification.Domain;
using IDCOL.CBS.Collection.Domain;
using IDCOL.CBS.CreditSanction.Domain;
using IDCOL.CBS.Disbursement.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.RepaymentEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IDCOL.CBS.Api.Infrastructure;

/// <summary>
/// Dev-only seed of downstream lifecycle ACTIVITY on top of the borrower history (customers,
/// sanctions, facilities) that <see cref="BorrowerHistorySeed"/> creates. Without this the
/// Disbursement, Collection and Classification screens are empty even though the loans exist -
/// which is exactly the "pages are missing data" gap. For each seeded borrower it derives, from
/// that borrower's own active repayment schedule, a coherent operational history:
///   * Disbursement: the loan's original drawdown posted through the full 3-stage maker-checker
///     (BU Suggested -> CAD Proposed -> Accounts Processed with a balanced GL), plus a few
///     in-flight tranches left at earlier stages so the BU/CAD work-queues aren't empty either.
///   * Collection: 2-stage receipts (CAD enters -> Accounts verifies -> GL) for the installments
///     that fell due on/before the as-of date, with the most recent one or two deliberately left
///     unpaid to create realistic arrears.
///   * Classification: a single DFIM 04/2021 run as of the last quarter-end over every account
///     whose loan is still outstanding, using the arrears implied by those unpaid installments -
///     producing a real Standard/SMA/Sub-Standard/Doubtful/Bad-Loss spread with provisioning.
/// Guarded exactly like the rest of the dev bootstrap (Development + UseSqliteForLocalDevelopment)
/// and idempotent: it no-ops if any disbursement already exists.
/// </summary>
public static class LifecycleActivitySeed
{
    // As-of / reporting date for the whole synthetic operational history. A quarter-end so the
    // classification run lines up with DFIM's quarterly base-date cadence and the CAD report
    // samples ("Mar'26").
    private static readonly DateOnly AsOf = new(2026, 3, 31);
    private static readonly double AsOfSerial = SerialDate.FromIso("2026-03-31")!.Value;

    // Maker-checker actors. Distinct users per stage so the domain's structural same-user
    // rejection is satisfied (BU initiates, CAD proposes/enters, Accounts posts/verifies).
    private const string Bu = "bu1";
    private const string Cad = "cad1";
    private const string Accounts = "acct1";

    // Standard GL codes used for the seeded journal lines (a minimal, self-consistent chart).
    private const string GlLoanAsset = "120100";
    private const string GlLoanAssetName = "Loans & Advances to Customers";
    private const string GlBankDisb = "110105";
    private const string GlBankDisbName = "Bank A/C - Disbursement";
    private const string GlBankColl = "110106";
    private const string GlBankCollName = "Bank A/C - Collection";
    private const string GlInterestIncome = "410100";
    private const string GlInterestIncomeName = "Interest Income - Loans";
    private const string GlLpcIncome = "410300";
    private const string GlLpcIncomeName = "Late Payment Charge Income";

    public static async Task SeedAsync(LoanLifecycleDbContext db)
    {
        if (await db.DisbursementRequests.AnyAsync()) return;

        var agreements = await db.LoanAgreements.AsNoTracking().ToListAsync();
        var facilities = await db.Facilities.Include(f => f.Versions).AsNoTracking().ToListAsync();
        var thresholds = await db.ClassificationThresholds.AsNoTracking().ToListAsync();
        var rates = await db.ProvisioningRates.AsNoTracking().ToListAsync();

        // Facilities keyed by their sanction (loan agreement) id, preferring the primary IDCOL
        // tranche when a loan is co-financed across several lenders.
        var facilityBySanction = facilities
            .GroupBy(f => f.SanctionId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(f => f.LenderCode == "IDCOL").ThenBy(f => f.LenderCode).First());

        var disbursements = new List<DisbursementRequest>();
        var receipts = new List<Receipt>();
        var classifications = new List<LoanClassification>();
        var runId = Guid.NewGuid();

        // Arrears pattern (installments left unpaid at as-of) rotated across borrowers so the
        // classification run yields a realistic mix rather than every account Standard.
        var arrearsPattern = new[] { 0, 0, 1, 0, 2, 0, 3, 1, 0, 4, 0, 1, 2, 0, 5, 0, 1, 0, 3 };

        var index = 0;
        foreach (var a in agreements.OrderBy(x => x.SanctionId))
        {
            var i = index++;
            if (!facilityBySanction.TryGetValue(a.Id, out var facility)) continue;

            List<ScheduleRow> schedule;
            try { schedule = facility.CurrentVersion.ComputeSchedule(); }
            catch { continue; }
            if (schedule.Count == 0) continue;

            var frequency = a.PrincipalFrequency <= 0 ? 4 : a.PrincipalFrequency;
            var monthsPerInstallment = Math.Max(1, 12 / frequency);
            var valueDate = a.AgreementDate;

            // ---- Disbursement: original drawdown, fully processed to GL ----
            var loanAmt = a.LoanAmount;
            var disb = DisbursementRequest.Initiate(
                Guid.NewGuid(), $"DISB-{a.SanctionId}-01", 1, a.Id, a.SanctionId, a.CustomerNo,
                a.ProjectName, a.LoanCurrency, loanAmt, a.GrantAmount, "Initial drawdown as per sanction.", Bu);
            disb.Propose(Cad, loanAmt, a.GrantAmount, "Reviewed against sanction limit; amounts justified.");
            disb.Post(Accounts, "RTGS", valueDate, new List<(string, string, decimal, decimal)>
            {
                (GlLoanAsset, $"{GlLoanAssetName} - {a.CustomerNo}", loanAmt, 0m),
                (GlBankDisb, GlBankDisbName, 0m, loanAmt),
            });
            disbursements.Add(disb);

            // A few borrowers also carry an in-flight tranche so the upstream queues show work:
            // every 3rd is left Suggested (awaiting CAD), every 3rd+1 is Proposed (awaiting Accounts).
            if (loanAmt > 0 && i % 3 == 0)
            {
                var tranche = DisbursementRequest.Initiate(
                    Guid.NewGuid(), $"DISB-{a.SanctionId}-02", 2, a.Id, a.SanctionId, a.CustomerNo,
                    a.ProjectName, a.LoanCurrency, Math.Round(loanAmt * 0.10m, 2), 0m,
                    "Additional tranche request pending CAD review.", Bu);
                disbursements.Add(tranche);
            }
            else if (loanAmt > 0 && i % 3 == 1)
            {
                var tranche = DisbursementRequest.Initiate(
                    Guid.NewGuid(), $"DISB-{a.SanctionId}-02", 2, a.Id, a.SanctionId, a.CustomerNo,
                    a.ProjectName, a.LoanCurrency, Math.Round(loanAmt * 0.10m, 2), 0m,
                    "Additional tranche request.", Bu);
                tranche.Propose(Cad, Math.Round(loanAmt * 0.10m, 2), 0m, "Reviewed; awaiting Accounts posting.");
                disbursements.Add(tranche);
            }

            // ---- Collections: receipts for installments due on/before as-of, with arrears ----
            var dueRows = schedule
                .Where(r => r.Principal > 0.005 && r.PayDate <= AsOfSerial)
                .OrderBy(r => r.PayDate)
                .ToList();

            // Only the most recent window is materialised, so long schedules don't explode row counts.
            var window = dueRows.Count > 6 ? dueRows.Skip(dueRows.Count - 6).ToList() : dueRows;
            var unpaidCount = Math.Min(arrearsPattern[i % arrearsPattern.Length], window.Count);
            var paidRows = window.Take(window.Count - unpaidCount).ToList();

            var receiptNo = 0;
            foreach (var row in paidRows)
            {
                receiptNo++;
                var principal = Math.Round((decimal)row.Principal, 2);
                var interest = Math.Round((decimal)(row.CashInterest > 0 ? row.CashInterest : row.Interest), 2);
                var amount = principal + interest;
                if (amount <= 0) continue;

                var payDate = DateOnly.FromDateTime(SerialDate.ToDate(row.PayDate));
                var receipt = Receipt.Enter(
                    Guid.NewGuid(), $"RCPT-{a.SanctionId}-{receiptNo:D2}", a.Id, a.SanctionId, a.CustomerNo,
                    a.ProjectName, a.LoanCurrency, "RTGS", $"TXN{a.SanctionId}{receiptNo:D2}",
                    "Sonali Bank PLC", amount, payDate, payDate, null, principal, interest, 0m, Cad);

                // Most receipts are reconciled to GL; for a couple of borrowers the latest one is
                // deliberately left Pending so the Accounts verification queue also has data.
                var leaveLatestPending = i % 5 == 0 && receiptNo == paidRows.Count;
                if (!leaveLatestPending)
                {
                    receipt.Verify(Accounts, "Reconciled with bank statement.", new List<(string, string, decimal, decimal)>
                    {
                        (GlBankColl, GlBankCollName, amount, 0m),
                        (GlLoanAsset, $"{GlLoanAssetName} - {a.CustomerNo}", 0m, principal),
                        (GlInterestIncome, GlInterestIncomeName, 0m, interest),
                    });
                }
                receipts.Add(receipt);
            }

            // ---- Classification: outstanding + arrears as of quarter-end ----
            var lastPaid = paidRows.LastOrDefault();
            var outstanding = lastPaid is not null
                ? Math.Round((decimal)lastPaid.ClosingBal, 2)
                : Math.Round((decimal)(schedule.FirstOrDefault(r => r.PayDate <= AsOfSerial)?.ClosingBal ?? schedule[0].OpeningBal), 2);

            if (outstanding <= 0) continue; // loan fully repaid at as-of - nothing to classify

            var overdueMonths = unpaidCount * monthsPerInstallment;
            var unpaidRows = window.Skip(window.Count - unpaidCount).ToList();
            var interestSuspense = ClassificationStatus.IsClassified(
                    ClassificationEngine.Classify(FinanceType.Term, a.LoanTenorMonths, overdueMonths, thresholds))
                ? Math.Round(unpaidRows.Sum(r => (decimal)(r.CashInterest > 0 ? r.CashInterest : r.Interest)), 2)
                : 0m;
            var eligibleCollateral = Math.Round(outstanding * 0.25m, 2); // indicative security cover
            var isCmsme = i % 4 == 0;

            var status = ClassificationEngine.Classify(FinanceType.Term, a.LoanTenorMonths, overdueMonths, thresholds);
            var provision = ClassificationEngine.ComputeProvision(
                status, isCmsme, outstanding, interestSuspense, eligibleCollateral, rates);

            classifications.Add(LoanClassification.Create(
                Guid.NewGuid(), runId, AsOf, a.Id, a.SanctionId, a.CustomerNo, a.ProjectName, a.LoanCurrency,
                FinanceType.Term, a.LoanTenorMonths, TenorBucket.For(FinanceType.Term, a.LoanTenorMonths),
                isCmsme, outstanding, overdueMonths, interestSuspense, eligibleCollateral, status,
                false, null, provision, Cad));
        }

        db.DisbursementRequests.AddRange(disbursements);
        db.Receipts.AddRange(receipts);
        db.LoanClassifications.AddRange(classifications);
        await db.SaveChangesAsync();

        Log.Information(
            "Lifecycle activity seed: {Disb} disbursements, {Rcpt} receipts, {Cls} classifications (run {RunId}) across {N} borrowers as of {AsOf}.",
            disbursements.Count, receipts.Count, classifications.Count, runId, agreements.Count, AsOf);
    }
}
