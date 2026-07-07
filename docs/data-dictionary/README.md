# IDCOL CBS — Data Dictionary

Field-level specification for every screen in the loan/credit lifecycle, derived from the
approved CBS flow prototype (built with the CAD head) and mapped against the current
.NET + Angular implementation. Each page lists its fields, repeating tables, current backend/
frontend coverage, and the concrete gaps still to close.

**Legend:** 🔴 Missing · 🟠 Partial / Semantics · 🟡 Naming

## Lifecycle flow

```
Admin            BU / CRM                              CAD                 Accounts
Product   ->  Customer Account -> Loan Agreement -> Initiate Disb. -> CAD Review -> Accounts Post -> GL
                                                     (Suggested)      (Proposed)     (Processed)
                                  Collection (CAD enter -> Accounts verify)  ->  Classification (DFIM)
```

## Pages

| # | Page | Dept | Route | Prototype fields | Gaps |
|---|---|---|---|---:|---:|
| 1 | [Product Configuration](01-product-configuration.md) | Admin | `/products` | 14 | 10 |
| 2 | [Customer Account](02-customer-account.md) | BU/CRM | `/customers` | 33 | 30 |
| 3 | [Loan Agreement](03-loan-agreement.md) | BU/CRM | `/sanctions` | 0 | 20 |
| 4 | [Initiate Disbursement](04-initiate-disbursement.md) | BU/CRM | `/disbursements` | 51 | 13 |
| 5 | [CAD Disbursement Review](05-cad-disbursement-review.md) | CAD | `/disbursements` | 63 | 14 |
| 6 | [Accounts Disbursement Posting](06-accounts-disbursement-posting.md) | Accounts | `/disbursements` | 77 | 21 |
| 7 | [Collection](07-collection.md) | CAD / Accounts | `/collections` | 22 | 12 |
| 8 | [Classification & Provisioning](08-classification.md) | CAD | `/classification` | — | — |
| 9 | [Facility / Repayment Schedule](09-facility-schedule.md) | CAD | `/sanctions/:id/facility` | — | — |

## Reports

- [CAD & F&A Reports Inventory](reports-inventory.md) — every CAD report (18.1–18.13 plus CIB
  Matrix, Invoice, LPC, Reschedule return, Write-off, Top Borrower …) and the in-scope F&A
  reports, each documented with its data source, filters, and key columns.
