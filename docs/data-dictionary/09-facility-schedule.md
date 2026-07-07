# Data Dictionary — Facility / Repayment Schedule (Reschedule & Restructure)

> **Department:** CAD  ·  **App route:** `/sanctions/:id/facility`  ·  **Backend module:** RepaymentEngine

## Purpose

The versioned repayment-schedule engine ("the heart"). A **Facility** is one lender-tranche of a
sanction; a **FacilityVersion** is one immutable "generation" of its schedule — the original as
sanctioned, or a later reschedule / restructure / rate-change / prepayment / moratorium-extension.
Schedule rows are never persisted; they are recomputed deterministically from the version's stored
`ParametersJson`, which is what makes the engine auditable.

## `Facility`

| Field | Type | Notes |
|---|---|---|
| `SanctionId` | guid | The owning loan agreement |
| `LenderCode` | string | `IDCOL`, `TRUST_BANK`, `BIFFL`, `IDCOL_ADDITIONAL` … (co-financed loans have several facilities) |
| `Currency` | string | BDT / USD |
| `Versions` | collection | Ordered version chain; exactly one is Active (highest sequence) |
| `CurrentVersion` | computed | Latest active version |

## `FacilityVersion`

| Field | Type | Notes |
|---|---|---|
| `VersionSequence` | int | 0 = Original, contiguous |
| `EventType` | enum | Original, Reschedule, Restructure, RateChange, Prepayment, MoratoriumExtension |
| `Status` | enum | Active / Superseded |
| `EffectiveDate` | date | Non-decreasing across the chain |
| `Label` / `SourceFile` | string | e.g. "1st Rescheduled", source workbook |
| `RateBeforePercent` / `RateAfterPercent` | decimal? | Rate delta provenance |
| `TenorMonthsBefore` / `TenorMonthsAfter` | int? | Tenor delta provenance |
| `CapitalizedAmount` | decimal | Interest capitalised into the new opening balance |
| `WaivedAmount` | decimal | Interest forgiven (distinct from capitalised) |
| `OverdueAmountRolledIn` | decimal | Overdue principal rolled into the new balance |
| `RegulatoryReference` | string? | e.g. DFIM circular / BB approval |
| `ParametersJson` | clob | Serialized `ScheduleParameters` — the sole source of truth for the schedule |

## Computed schedule rows (`ScheduleRow`, recomputed on read)

| Column | Notes |
|---|---|
| `Idx` / `PayDate` | Installment index / Excel-serial pay date |
| `OpeningBal` | Opening balance |
| `Interest` / `CashInterest` / `CapInterest` | Total = cash + capitalised interest |
| `Principal` | Principal component |
| `Tds` | Total debt service (per configured mode) |
| `ClosingBal` | Opening + capitalised − principal (reconciliation invariant) |
| `Days` | Day count in the period |

## In-table schedule modification

`ApplyInstallmentOverride(index, interest?, opening?, closing?)` pins a specific installment to an
explicit value, reusing the engine's override arrays. On first use the arrays are backfilled with
the naturally-computed value per row, so editing one row never zeroes the others.

## Seeded data

All 19 real IDCOL borrowers are seeded with their original schedule **plus every historical
reschedule/restructure/prepayment event** (63 versions total), including co-financed tranches.
