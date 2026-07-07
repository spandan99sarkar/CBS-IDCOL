# Data Dictionary — Product Configuration

> **Department:** Admin  ·  **App route:** `/products`  ·  **Backend module:** ProductConfig

## Purpose

A single-page master-data entry form ("Product Configuration Entry") plus a searchable "Product List" grid, both persisting to localStorage under key 'efs_disbursement_products'. The Entry page captures the product-level defaults that seed a Loan Agreement: identity (code, name), classification (product type, status), interest behaviour (interest type, interest calculation method, EMI/amortization method), grace treatment, three policy toggles (prepayment, penalty, rescheduling), and a suggested/lower/upper interest-rate band. There are no repeating/tabular structures, no computed/derived values, and no charge/GL/schedule lines on this stage — every field is a single scalar user input. Workflow is minimal: two actions on the Entry page (Clear = reset form, Save = validate that Product Code is non-empty then upsert by code and navigate to the list). The List page has one action (Add New) plus per-row Edit (navigates to Entry with ?code=). Status is a two-state Active/Inactive toggle rendered as a badge in the list; there is NO maker-checker / Suggest-Propose-Process workflow on this stage (unlike other stages of the CBS). Editing an existing product is supported via the ?code query param which pre-loads the matching record.

## Fields (14)

| Field | Type | Options / Values | Section | Source | Notes |
|---|---|---|---|---|---|
| `productCode` | text |  | Product Configuration | user input | Required (only validated field). On save it is trimmed; used as the natural key for upsert/edit lookup. |
| `productName` | text |  | Product Configuration | user input | Free text. |
| `productType` | select | Term, Revolving | Product Configuration | user input | TS type ProductType = '' \| 'Term' \| 'Revolving'. Only two real values. |
| `interestType` | select | Fixed, Floating, Both | Product Configuration | user input | TS type InterestType. Includes a third value 'Both' beyond Fixed/Floating. |
| `interestCalculationMethod` | select | Simple, Compound, Both | Product Configuration | user input | TS type InterestCalculationMethod. No equivalent anywhere in the current app. |
| `emiMethod` | select | Annuity, EMI, Level Principal, Bullet | Product Configuration | user input | TS type EMIMethod. This is the amortization/installment method. Values differ from the app's RepaymentMethod set. |
| `gracePeriod` | select | Interest, Principal, Both | Product Configuration | user input | TS type GracePeriod. Describes WHAT is graced (interest vs principal vs both) — categorical, not a month count. |
| `prepaymentRules` | select | Yes, No | Product Configuration | user input | TS type YesNo — 3-state including empty/unset. |
| `penaltyRules` | select | Yes, No | Product Configuration | user input | TS type YesNo — 3-state including empty/unset. |
| `reschedulingPolicy` | select | Yes, No | Product Configuration | user input | TS type YesNo. No equivalent in the current app. |
| `suggestedInterestRate` | number |  | Product Configuration | user input | step 0.01, stored as string; rendered with % in list. |
| `lowerLimitInterestRate` | number |  | Product Configuration | user input | step 0.01; forms lower bound of the rate band. |
| `upperLimitInterestRate` | number |  | Product Configuration | user input | step 0.01; forms upper bound of the rate band. |
| `status` | select | Active, Inactive | Product Configuration | user input | TS type Status = 'Active' \| 'Inactive'. Defaults to 'Active'. Directly editable on the form (two-state). |

## Current application coverage

**Backend:** LoanProduct (aggregate root) at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/ProductConfig/IDCOL.CBS.ProductConfig.Domain/LoanProduct.cs; Create() factory + Deactivate(); Application command CreateProductCommand + ProductDto at .../IDCOL.CBS.ProductConfig.Application/Products/CreateProduct.cs and ListProducts.cs; EF config LoanProductConfiguration.cs (table PRODCFG_LOAN_PRODUCT) at .../LoanLifecycle.Infrastructure/Persistence/Configurations/; API ProductsController.cs (GET /api/products, POST /api/products) at host/IDCOL.CBS.Api/Controllers/

Existing persisted fields: `ProductCode`, `ProductName`, `ProductType`, `InterestType`, `RepaymentMethod`, `DayCountBasis`, `GracePeriodMonths`, `PrepaymentAllowed`, `PenaltyAllowed`, `SuggestedRatePercent`, `LowerRatePercent`, `UpperRatePercent`, `IsActive`, `CreatedBy/CreatedAtUtc/LastModifiedBy/LastModifiedAtUtc`

**Frontend:** Product & CreateProductRequest interfaces at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/core/lifecycle/lifecycle.models.ts; ProductsComponent at .../features/products/products.component.ts + products.component.html

## Gaps vs. prototype (10)

| Field / concept | Severity | Layers | Recommendation |
|---|---|---|---|
| interestCalculationMethod | 🔴 Missing | domain, application, api, angular | Add InterestCalculationMethod (Simple\|Compound\|Both) as a new property on LoanProduct with a Create() param, add to CreateProductCommand + validator (Must be Simple/Compound/Both), map a new column (e.g. INTEREST_CALC_METHOD nvarchar(20)) in LoanProductConfiguration, surface in ProductDto/List, and add a dropdown to the Angular form + Product/CreateProductRequest models. No current equivalent exists anywhere. |
| reschedulingPolicy | 🔴 Missing | domain, application, api, angular | Add ReschedulingAllowed (bool, from Yes/No) to LoanProduct + Create() + CreateProductCommand + EF column (RESCHEDULING_ALLOWED) + Angular model & form control. Prototype has a third policy toggle the app lacks (app only has prepayment + penalty). |
| emiMethod | 🟠 Semantics | domain, application, api, angular | Prototype EMIMethod values are Annuity\|EMI\|Level Principal\|Bullet; the app's RepaymentMethod values are Level Principal\|Annuity\|PPMT Principal\|Scheduled Principal. Overlap is partial and 'EMI' and 'Bullet' are absent from the app while 'PPMT Principal'/'Scheduled Principal' are absent from the prototype. Reconcile the allowed set with the CAD head: either extend RepaymentMethod's permitted values to include EMI and Bullet (and rename UI label to 'EMI Method' to match prototype) or keep both concepts. Update the repaymentMethods array in products.component.ts, the domain comment, and any downstream schedule-engine switch that keys off RepaymentMethod. |
| gracePeriod | 🟠 Semantics | domain, application, api, angular | Prototype gracePeriod is a categorical enum (Interest\|Principal\|Both) describing WHICH component is graced; the app's GracePeriodMonths is an integer duration. These are different concepts, not a naming variant. Add a new GraceType (Interest\|Principal\|Both) property/column alongside the existing month count (the Loan Agreement will still need a duration), add the CreateProductCommand param + validator, EF column (GRACE_TYPE), and an Angular dropdown. Keep GracePeriodMonths but treat GraceType as the prototype-required field. |
| productType | 🟠 Partial | application, api, angular | Prototype constrains ProductType to exactly Term\|Revolving; the app stores free text (default 'Term Loan') with no validation and the Angular form uses a plain text input. Add a validator restricting to the agreed value set (confirm with CAD head whether it is strictly Term/Revolving or the broader 'Term Loan/Working Capital/Bridge/Lease' list the domain comment implies — this is a spec conflict to resolve) and convert the Angular text input to a <select>. Domain type can stay string. |
| interestType | 🟠 Partial | domain, application, angular | Prototype allows Fixed\|Floating\|Both; the app's CreateProductCommandValidator hard-rejects anything except Fixed\|Floating (Must(t => t is 'Fixed' or 'Floating')) and the Angular select only offers Fixed/Floating. Add 'Both' to the validator's allowed set and to the Angular <select>. Verify downstream rate-behaviour logic can handle a 'Both' product. |
| prepaymentRules | 🟠 Partial | application, angular | Prototype is a 3-state select (unset/Yes/No); the app models it as a plain bool AND the Angular form does not render any input for prepaymentAllowed (it is silently defaulted to true in blank()). Add the missing checkbox/select control to the Angular form so the user can actually set it; optionally treat unset as a validation-required choice. Bool is acceptable if empty is disallowed on save. |
| penaltyRules | 🟠 Partial | application, angular | Same as prepaymentRules: penaltyAllowed exists in the model/command but has NO control in the Angular form (defaulted to true). Add the form control; keep bool but ensure the user selects it. |
| status | 🟠 Partial | application, api, angular | Prototype lets the user set Active/Inactive directly on the entry form (two-way). The app only has a one-way domain Deactivate() with no reactivate path, no update/status endpoint, and the Angular UI shows status as a read-only badge. Add an update/status-toggle command + endpoint and an editable control so Inactive products can be reactivated and status set on the form. |
| __edit_flow__ (product edit/update) | 🔴 Missing | application, api, angular | Prototype supports editing an existing product (List row -> Entry ?code= pre-loads, Save upserts). The app has Create + List + domain Deactivate only — no UpdateProductCommand, no PUT /api/products/{id}, and no edit mode in ProductsComponent. Add an update command/handler (guarding the immutable ProductCode or allowing rename per policy), a PUT endpoint, a GetById query, and edit-mode wiring in the Angular form. Without this the prototype's core edit workflow cannot be reproduced. |

