# Data Dictionary — CAD & F&A Reports Inventory

Report catalogue for the two reporting suites IDCOL runs off the loan portfolio, sourced from the
`Resources by IDCOL/Reports` samples (CAD report designs 18.1–18.13 + F&A report templates). Each
report lists the **data source** (which module/entity feeds it), the **filters**, and the **key
columns**, so it can be built directly against the existing schema.

All CAD reports share a common shape: a **filter panel** (Borrower, Project, Donor, Credit No,
Department, Sector, Project Type, Balance-sheet On/Off, date From/To, Loan/Grant, Loan/Investment)
and a **tabbed grid** — `Detail Local Currency`, `Detail Foreign Currency`, `Summary Local Currency`,
`Summary Foreign Currency` — with a totals row.

## CAD Reports

| # | Report | Source data | Key columns |
|---|---|---|---|
| 18.1 | **Accrual Report** | Facility schedule (accrued interest) per period | Borrower, Project, Statement, accrued interest, days |
| 18.2 | **Average Installment Size** | Facility schedule installments | Borrower, Project, avg installment, count |
| 18.3 | **Borrower Payment Info** | Collection receipts | Borrower, receipt date, principal, interest, LPC, mode |
| 18.4 | **Disbursement Statement** | Disbursement (Processed) | Borrower, Project, Statement, Credit No, Disb. date, Disb. amount + total |
| 18.5 | **Principal & Interest Due** | Facility schedule (due rows) | Borrower, Project, Payment date, Principal, Capitalized(Pr), Interest, Capitalized(In), Total |
| 18.6a | **Interest Rate Report** | Sanction + rate-change events | Borrower, effective rate, from/to |
| 18.6b | **Interest Suspense Report** | Classification (`InterestSuspense`) | Borrower, outstanding, suspense, status |
| 18.7 | **Loan Statement (Account Statement)** | Facility schedule + disbursements + receipts | Date, particulars, debit, credit, running balance |
| 18.8 / 18.12 | **Principal Movement** | Disbursements (+) and receipts (−) principal | Borrower, opening, disbursed, repaid, closing |
| 18.9 | **Transaction Statement** | Disbursement + Receipt GL lines | Date, voucher, GL code, debit, credit |
| 18.10 | **Sanction Report** | Loan agreements | Borrower, Project, sanction amount, tenor, rate, status |
| 18.11 | **Classification Report** | Classification run | Borrower, finance type, overdue months, status, outstanding, provision |
| 18.13 | **LPC (Late Payment Charge) Report** | Collection (`LpcAmount`, `LpcDate`) + overdue | Borrower, overdue days, LPC rate, LPC amount |
| — | **Payment Assign / Payment Receipt** | Collection allocation | Receipt → installment allocation (waterfall) |
| — | **CIB Matrix** | Sanction + classification + outstanding | BB CIB reporting matrix per borrower |
| — | **Invoice** | Facility schedule due + LPC | Borrower invoice for the period's due amount |
| — | **Overdue Status** | Facility schedule vs receipts | Borrower, overdue principal/interest, days |
| — | **Quarterly Payment Status** | Collection receipts by quarter | Borrower, quarter, due vs paid |
| — | **Reschedule & Restructure Return** | FacilityVersion events | Borrower, event type, date, capitalized, rate/tenor delta, reg. ref |
| — | **Statement of Write-off** | Classification (Bad/Loss written off) | Borrower, write-off date, amount |
| — | **Top Borrower Report** | Outstanding by borrower | Rank, borrower, outstanding, % of portfolio |

## F&A Reports (lending-related, in scope)

| Report | Source data | Notes |
|---|---|---|
| **FI Accrual Status** | Facility schedule accrued interest | Monthly accrued interest income on the loan book |
| **Interest Income** | Collection receipts (interest) + accrual | Realised + accrued interest income |
| **ISS (Interest Suspense) Report** | Classification `InterestSuspense` | Suspense ledger movement |
| **Note on Provision & OL** | Classification `ProvisionRequired` | Provision charge / release for the period |
| **Statement of Affairs (BB template)** | Loan book outstanding + provisions | Loan-portfolio lines of the BB statement of affairs |
| **Monthly Liquidity Statement (BB)** | Schedule due inflows | Contractual loan inflows by maturity bucket |
| **Loan & Borrowing Reconciliation** | Disbursement + receipts + GL | Reconcile loan control accounts to sub-ledger |
| **Exchange Gain/Loss** | USD facilities revalued | For foreign-currency loans (DHRL, KPCL) |

## F&A Reports (out of the lending scope — flagged)

These appear in the shared F&A folder but belong to Treasury / Donor-Fund / Tax modules that the
approved architecture marks **out of scope** for this build (done in eFS / elsewhere):

- Bank Reconciliation, FDR schedules/interest, Investment in Capital Market / Commercial Paper
- Tax/VAT: Withholding Tax Return, Mushak 6.3/6.6/9.1, Tax/VAT certificates
- Grants: Quarterly Grant Report, QGDP, project FMR/IUFR/FAPAD FS (donor project statements)
- Depreciation, Gratuity/Provision (HR/fixed-asset), CSR, Dividend

> **Build status:** report data sources all exist in the seeded schema. The CAD report suite is the
> priority build (loan-portfolio driven); the in-scope F&A subset follows. See the project plan for
> the reporting module (CQRS query side + Angular report screens with the shared filter/grid shell).
