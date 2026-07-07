# IDCOL CBS — Feature Gap Analysis vs. a Standard NBFI Core Banking Solution

What a full Bangladesh-Bank-compliant NBFI loan/credit CBS is expected to do, mapped against what
this build currently delivers. Scope note: Treasury, FDR/investment, Donor-Fund, Budgeting, Vendor/
Bill Payment and the full General Ledger are **intentionally out of scope** here — IDCOL runs those
in eFS; this system owns the custom loan/credit lifecycle and exposes integration points.

**Legend:** ✅ Built · 🟡 Partial · 🔴 Missing · ⚪ Out of scope (eFS/other system)

---

## 1. Customer / KYC / AML

| Capability | Status | Notes |
|---|---|---|
| Customer master (local read model) | 🟡 | Basic Customer entity + seed; no beneficial-owner / related-party model |
| AML System inbound integration (sync job + SYNC_LOG) | 🔴 | The one required real-time integration; customers are seeded, not synced from the AML system of record |
| KYC profile, risk grading, PEP / sanctions screening | 🔴 | Risk level is a static field; no screening result model or re-KYC scheduling |
| CIB (Credit Information Bureau) inquiry / reporting | 🔴 | No CIB inquiry capture or CIB Matrix return |

## 2. Product Configuration

| Capability | Status | Notes |
|---|---|---|
| Loan product master | 🟡 | Core fields present; missing `interestCalculationMethod` (Simple/Compound), `reschedulingPolicy`, grace-type enum, Bullet EMI, Active/Inactive edit (see data-dictionary gaps) |
| Rate index / rate card (floating benchmarks) | 🔴 | No LIBOR/SOFR/benchmark index master; floating rate is a scalar |
| Fee / charge rule engine | 🔴 | No configurable fees (processing, commitment, monitoring, front-end) |
| Holiday calendar master | 🟡 | Engine supports holidays; no CRUD/master screen |
| Business unit master | 🔴 | BU codes referenced but not a managed master |
| Payment-allocation (waterfall) rule config | 🔴 | Allocation is hard-coded, not product-configurable |

## 3. Credit / Sanction & Appraisal

| Capability | Status | Notes |
|---|---|---|
| Loan Agreement (sanction) | ✅ | With sign step |
| 5-stage approval chain (BU→RM→BU Head→Dept Head→CAD→Board) | 🔴 | Only a single sign; no sequential approval workflow |
| Credit Limit Management (borrower/group exposure) | 🔴 | No limit tree or single-borrower/group exposure engine |
| CRG / internal credit rating engine | 🔴 | Credit rating is a free-text field |
| Credit appraisal / proposal document | 🔴 | Not modelled |
| Multi-currency sanction (foreign + BDT-equivalent) | 🔴 | Single currency + amount (see LoanAgreement gaps) |

## 4. Loan Account Opening

| Capability | Status | Notes |
|---|---|---|
| Distinct Loan Account entity | 🔴 | Sanction id is used as the account proxy; no dedicated account |
| BB regulatory codes (Economic Purpose / Security / Sector / SME) | 🔴 | Required for BB returns; not captured |

## 5. Disbursement

| Capability | Status | Notes |
|---|---|---|
| 3-stage maker-checker (BU→CAD→Accounts) with structural control | ✅ | Enforced in domain, not just UI |
| GL posting on Accounts stage | ✅ | Balanced double-entry |
| Multi-tranche tracking (granted / already-disbursed / available) | 🟡 | Multiple requests supported; no running availability reconciliation block |
| Multi-currency + exchange rate + local-currency amount | 🔴 | Prototype captures FX; app is single-currency |
| Client-instruction split (multi-bank payment table) | 🔴 | Prototype has a client-instruction line-item table |
| Source of fund / FI instruction / generated reference IDs | 🔴 | Prototype "Disbursement Node" fields (see data dictionary) |
| Amount-in-words, value date, branch/BU on the node | 🟡 | Value date only |

## 6. Repayment Schedule Engine ("the heart")

| Capability | Status | Notes |
|---|---|---|
| Schedule generation (level/annuity/PPMT/scheduled) | ✅ | Validated against 19 real borrowers |
| Day-count conventions, grace, capitalization, rate-change | ✅ | |
| Versioning: reschedule / restructure / prepayment / moratorium | ✅ | Facility/FacilityVersion, seeded with real history |
| In-table schedule override | ✅ | |
| Prepayment calculation UI + down-payment calc | 🟡 | Domain supports it; no dedicated prepayment/down-payment screen |
| All 6 capitalization methods as first-class config | 🟡 | Core methods covered; some edge methods not surfaced in UI |

## 7. Collection & Payment Application

| Capability | Status | Notes |
|---|---|---|
| 2-stage receipt (CAD enter → Accounts verify) + GL | ✅ | |
| Principal / Interest / LPC allocation | 🟡 | Manual allocation; no automatic waterfall against due schedule |
| PDC (post-dated cheque) lifecycle | 🔴 | PDC is only a payment mode; no PDC register / presentation / bounce workflow |
| LPC (late payment charge) auto-calc engine | 🔴 | LPC is entered, not computed from overdue days × rate |
| Invoicing (periodic due invoice + samples) | 🔴 | No invoice generation |
| Bounce / dishonour handling, reversal | 🔴 | No reversal-as-new-entry flow for receipts |
| Auto payment assignment to installments | 🔴 | No receipt→installment allocation ledger |

## 8. Classification & Provisioning

| Capability | Status | Notes |
|---|---|---|
| DFIM 04/2021 classification engine (config-driven) | ✅ | Thresholds/rates in DB; run seeded |
| Provisioning (general/specific, eligibility haircut) | ✅ | |
| Interest suspense **ledger** (movement over time) | 🟡 | Suspense is computed per run; no running ledger |
| Write-off workflow + statement | 🔴 | No write-off state machine or write-off register |
| CL-1..7 regulatory report forms | 🔴 | The classification data exists; the CL forms aren't generated |
| Automated quarterly batch (Worker) | 🔴 | Runs on demand; Worker host skeleton exists but no scheduled job |

## 9. Security / Collateral & Covenant

| Capability | Status | Notes |
|---|---|---|
| Collateral register (10 instrument families) | ✅ | **Built (Phase 9)** — BG/FDR/MTDR/DSRA/land/insurance/CR/FS/PDC/monitoring |
| Expiry + recommended-action engine + dashboard | ✅ | Days-left buckets, urgency highlighting |
| Templated letter generation (~30 letters) | ✅ | Catalogue + merge renderer |
| BB eligibility haircut (IDCOL portion %, eligible %) | ✅ | Computed columns |
| Financial-ratio compliance engine (DSCR, D/E, current ratio…) | 🔴 | FS detail line-items + ratio-vs-standard not yet built |
| Covenant compliance calendar + reminders (scheduled) | 🟡 | Compliance status stored; no scheduled reminder job |
| Bank / Branch / Covenant-Type master data screens | 🔴 | Referenced as free text; no masters |
| Letter → lifecycle trigger automation | 🟡 | Letters render; generating one doesn't yet auto-transition the instrument |

## 10. Accounting / GL

| Capability | Status | Notes |
|---|---|---|
| Journal lines on disbursement / collection | ✅ | Balanced double-entry captured |
| Full GL (chart of accounts, trial balance, period close, vouchers) | ⚪ | eFS owns the GL |
| Integration outbox / GL export to eFS | 🔴 | Journal lines exist; no outbox/webhook export mechanism |

## 11. Reporting

| Capability | Status | Notes |
|---|---|---|
| Core CAD reports (sanction, disbursement, due, collection, classification, principal movement, reschedule, provisioning) | ✅ | Built with shared filter/grid shell |
| Remaining CAD reports (account statement, invoice, LPC, CIB matrix, top borrower, write-off, accrual, interest-rate) | 🔴 | Catalogued in reports-inventory; not yet built |
| BB regulatory returns (CIB, SBS 1/2/3, CTR/STR, reschedule return, large-loan) | 🔴 | Data largely exists; formatted returns not generated |
| Full F&A report suite (Statement of Affairs, ISS, liquidity, recon) | 🔴 | In-scope subset only; most not built |
| Scheduled / automated report generation (no manual step) | 🔴 | Reports are on-demand |

## 12. System Administration & Cross-cutting

| Capability | Status | Notes |
|---|---|---|
| RBAC + roles + structural maker-checker + audit trail | ✅ | 3-layer maker-checker enforcement |
| EOD / BOD batch scheduler | 🔴 | Worker host skeleton only; no accrual/EOD/reclassification jobs |
| Parameter / master-data admin UI | 🔴 | Config is DB-seeded; no admin screens |
| User & role management UI | 🟡 | Users seeded; no management screen |
| Notifications / alerts (email / SMS / in-app) | 🔴 | None (needed for covenant/expiry reminders) |
| Document management (upload/store instrument scans, agreements) | 🔴 | File-link fields exist; no storage/upload |
| Dashboards / MIS / analytics | 🟡 | Security dashboard only; no portfolio MIS dashboard |
| Workflow / BPM engine (reusable approval chains) | 🟡 | Maker-checker per module; no generic SequentialApprovalWorkflow |
| BB ICT security controls (audit, encryption, DR posture) | 🔴 | Not yet hardened (Phase 9 of the plan) |

---

## Highest-priority missing items (recommended order)

1. **Credit approval chain + Credit Limit Management** — core NBFI credit governance, currently a single sign step.
2. **Loan Account entity + BB regulatory codes** — prerequisite for every BB regulatory return.
3. **Collection depth** — LPC auto-calc, PDC register, invoicing, auto-allocation, bounce/reversal.
4. **AML System integration** — the one mandated real-time dependency; unblocks true KYC/CIB.
5. **BB regulatory returns** — CIB Matrix, reschedule return, CL-1..7, SBS — the data exists; they need formatting + scheduling.
6. **EOD/BOD batch + notifications** — automated accrual, reclassification, and covenant/expiry reminders (the "no manual intervention" BB requirement).
7. **Fee/charge + rate-index configuration** — product economics beyond a flat rate.
8. **Financial-ratio covenant engine + master data** — completes the Security & Covenant module.
9. **Disbursement/Loan-Agreement field parity** with the prototype (multi-currency, client instructions, source of fund) — see the per-page data dictionary.
10. **Integration outbox** — GL export to eFS and event publishing.

_See also: [data-dictionary/](data-dictionary/) for field-level prototype gaps, and
[data-dictionary/reports-inventory.md](data-dictionary/reports-inventory.md) for the full report catalogue._
