# Data Dictionary — Classification & Provisioning

> **Department:** CAD  ·  **App route:** `/classification`  ·  **Backend module:** Classification (DFIM Circular 04/2021 engine)

## Purpose

Runs the Bangladesh Bank DFIM Circular 04/2021 classification & provisioning engine over the loan
accounts as of a base date, producing one classification row per account plus the configurable
threshold/rate matrix that drives it. All regulatory numbers are config-driven (`ClassificationThreshold`,
`ProvisioningRate`), never hardcoded. Outstanding and arrears are derived from each facility's active
repayment schedule.

## Configuration tables

### `ClassificationThreshold` (DFIM overdue bands)

| Field | Type | Values | Notes |
|---|---|---|---|
| `FinanceType` | enum | ShortTerm, Term, Lease, Housing | Each finance type has its own overdue thresholds |
| `TenorBucket` | enum? | `1-5YR`, `>5YR`, null | ShortTerm is not tenor-bucketed; others split at 60 months |
| `Status` | enum | Standard, SMA, Sub-Standard, Doubtful, Bad/Loss | Worst-last severity order |
| `MinOverdueMonths` | decimal | — | Lower bound of the band (inclusive) |
| `MaxOverdueMonths` | decimal? | — | Upper bound; null = open-ended (worst band) |
| `CircularRef` | string | e.g. `DFIM-04/2021` | Provenance |
| `EffectiveDate` | date | — | Versioned by circular/effective date |

### `ProvisioningRate`

| Field | Type | Values | Notes |
|---|---|---|---|
| `Status` | enum | Standard … Bad/Loss | — |
| `IsCmsme` | bool | — | Standard splits CMSME (0.25%) vs other (1%) |
| `ProvisionType` | enum | General, Specific | General for Standard/SMA; Specific for classified |
| `RatePercent` | decimal | e.g. 0.25, 1, 5, 20, 50, 100 | Applied to the provision base |
| `CircularRef` / `EffectiveDate` | — | — | Provenance |

## Result rows — `LoanClassification`

| Field | Type | Source | Notes |
|---|---|---|---|
| `RunId` | guid | run grouping | One classification run = one base date over N accounts |
| `AsOfDate` | date | run input | Quarter-end base date (matches DFIM cadence) |
| `AccountId` / `AccountRef` | guid / string | loan account | Currently the sanction id/ref proxy |
| `CustomerNo` / `ProjectName` / `Currency` | — | account | Denormalised for reporting |
| `FinanceType` | enum | product | Drives threshold lookup |
| `TenorMonths` | int | sanction | Drives tenor bucket |
| `TenorBucket` | enum? | computed | `TenorBucket.For(financeType, tenorMonths)` |
| `IsCmsme` | bool | account | Affects Standard provisioning rate |
| `OutstandingAmount` | decimal | **computed** from active schedule closing balance at as-of | — |
| `OverdueMonths` | decimal | **computed** from unpaid due installments | Drives objective status |
| `InterestSuspense` | decimal | computed | Accrued interest on arrears for classified accounts |
| `EligibleCollateral` | decimal | security module | Reduces the specific-provision base |
| `Status` | enum | **engine** `Classify()` | Objective band, raised by any qualitative override |
| `IsQualitativeOverride` / `QualitativeReason` | bool / string | CAD input | Override never improves status, only worsens |
| `ProvisionType` | enum | engine | General / Specific |
| `ProvisionRatePercent` | decimal | engine | Resolved rate |
| `ProvisionBase` | decimal | engine | Specific: max(outstanding − suspense − collateral, 15% × outstanding); SMA: outstanding − suspense; Standard: outstanding |
| `ProvisionRequired` | decimal | engine | `round(base × rate/100, 2)` |

## Seeded data

The dev seed runs one DFIM classification as of **31-Mar-2026** over every outstanding borrower,
deriving arrears from deliberately-unpaid recent installments — yielding a realistic
Standard / SMA / Sub-Standard / Doubtful / Bad-Loss spread with provisioning.
