# Data Dictionary — Initiate Disbursement

> **Department:** BU/CRM  ·  **App route:** `/disbursements`  ·  **Backend module:** Disbursement (Stage 1 – BU)

## Purpose

The "Initiate Disbursement" BU/CRM entry page (InitiateDisbursementEntry.tsx) is stage 1 of IDCOL's 3-stage disbursement maker-checker (Suggested -> Proposed/In Review -> ...). Workflow: BU user selects a Customer A/C from a dropdown; selecting it auto-patches (read-only) six collapsible display panels from the signed sanction/account: (1) Account General Information, (2) Customer Information, (3) Borrower Agreement Details (Core Info + Repayment/Interest Info + Other Details), (4) Customer Account Details, (5) Agreement Related Documents table, (6) Customer Account Related Documents table. Below those is a computed "Disbursement Summary" panel (Already Disbursed Loan, Available Loan, Already Disbursed Grant, Available Grant). The only user-editable capture is the "Call For Disbursement" section (BU Suggested Loan Disbursement Amount, BU Suggested Grant Disbursement Amount, Remarks) plus a repeating "BU provided Document" upload table (Document Name + File, add/remove rows). Actions: Clear and Submit (Submit navigates to the list, transitioning the record to status "Suggested"). The List page (InitiateDisbursementList.tsx) shows Reference ID, Entry Date, Borrower Name, A/C No, Project, Total Amount, Status (Suggested | In Review), item count, and a view/eye action; supports search + filter + pagination. NOTE: the CAD-authored help/Disbursement Entry.html is a DIFFERENT, richer "Disbursement Node" screen (Node Information, Borrower & Facility Information, Generated References, Schedule Handling, Disbursement Instruction with Cheque/EFT/RTGS/GL Transfer/Pay Order tabs + client bank instruction table, Disbursement Amount) — that HTML models the later Accounts/posting node, not the BU initiate stage, so its instruction/GL/schedule fields are out of scope for THIS stage except where noted (branch, value date, disbursement number, reference no/date).

## Fields (51)

| Field | Type | Options / Values | Section | Source | Notes |
|---|---|---|---|---|---|
| `Select Customer A/C (drives auto-patch of all panels)` | select | 1002345678 (ABC Corporation Ltd.), 1002345679 (John Doe) | Top selector | user input | Chooses the loan account / signed sanction to disburse against. In the real app this is the sanction/loan-account selector. |
| `A/C No` | computed |  | Account General Information | computed | Read-only, patched from account. |
| `A/C Open Date` | computed |  | Account General Information | computed |  |
| `A/C Expiry Date` | computed |  | Account General Information | computed |  |
| `System Date` | computed |  | Account General Information | computed |  |
| `Customer ID` | computed |  | Customer Information | computed |  |
| `Customer Name` | computed |  | Customer Information | computed |  |
| `Mobile No` | computed |  | Customer Information | computed |  |
| `Email Address` | computed |  | Customer Information | computed |  |
| `Sector Code` | computed |  | Customer Information | computed |  |
| `Address` | computed |  | Customer Information | computed | Not present on current Customer model. |
| `Sanction ID` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Business Unit` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Product` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Project` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Industry Type` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Loan Amount (with currency / Equivalent BDT handling)` | computed |  | Borrower Agreement Details - Core | computed | Supports loanCurrency='Equivalent BDT' showing foreign amount + BDT equivalent. |
| `Grant Amount (with currency / Equivalent BDT handling)` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Agreement Date` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Availability Period (Date)` | computed |  | Borrower Agreement Details - Core | computed | = sanction expiryDate. |
| `Availability Period (Months)` | computed |  | Borrower Agreement Details - Core | computed |  |
| `Interest Rate Type` | computed | Fixed, Float | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Initial Interest Rate` | computed |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Loan Tenor (Months)` | computed |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `No of Principal Repayments` | computed |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Interest Grace Period (+ unit)` | computed |  | Borrower Agreement Details - Repayment/Interest | computed | Unit e.g. Month. |
| `Principal Moratorium Period (+ unit)` | computed |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Repayment Method` | computed | Level Principal, EMI | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Principal Frequency` | computed | Monthly, Quarterly | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Interest Frequency` | computed | Monthly | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Day Count Basis` | computed | 360, 365 | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Initiated by (BU)` | computed |  | Borrower Agreement Details - Other | computed |  |
| `Approved by (BU)` | computed |  | Borrower Agreement Details - Other | computed |  |
| `Initiated by (CRM)` | computed |  | Borrower Agreement Details - Other | computed |  |
| `Approved by (CRM)` | computed |  | Borrower Agreement Details - Other | computed |  |
| `Credit Rating` | computed |  | Borrower Agreement Details - Other | computed |  |
| `Remarks (agreement)` | computed |  | Borrower Agreement Details - Other | computed | Sanction-level remark, read-only here. |
| `Loan Currency` | computed |  | Customer Account Details | computed |  |
| `Loan Amount` | computed |  | Customer Account Details | computed |  |
| `Loan Type` | computed | New | Customer Account Details | computed | e.g. New; enum not fully enumerated in prototype data. |
| `Grant Currency` | computed |  | Customer Account Details | computed |  |
| `Grant Amount` | computed |  | Customer Account Details | computed |  |
| `Already Disbursed Loan` | computed |  | Disbursement Summary | computed | Sum of prior disbursements for this sanction (loan). |
| `Available Loan Amount` | computed |  | Disbursement Summary | computed | = sanction loan amount - already disbursed loan. Validation ceiling for suggested loan amount. |
| `Already Disbursed Grant` | computed |  | Disbursement Summary | computed |  |
| `Available Grant Amount` | computed |  | Disbursement Summary | computed | Validation ceiling for suggested grant amount. |
| `BU Suggested Loan Disbursement Amount` | number |  | Call For Disbursement | user input | Editable. Currency prefix derived from loanCurrency (BDT if Equivalent BDT). Maps to SuggestedLoanAmount. |
| `BU Suggested Grant Disbursement Amount` | number |  | Call For Disbursement | user input | Editable. Maps to SuggestedGrantAmount. |
| `Remarks (call for disbursement)` | textarea |  | Call For Disbursement | user input | Maps to BuRemarks. |
| `BU provided Document - Document Name` | text |  | BU provided Document (repeating table) | user input | Per-row free text; add/remove rows. |
| `BU provided Document - File` | file |  | BU provided Document (repeating table) | user input | Per-row file upload; NOT captured anywhere in current app. |

## Repeating / line-item tables

- BU provided Document (EDITABLE, this stage's real capture): columns = Document Name (text), File (upload), Action (remove); rows added via 'Add Document'. No persistence of uploaded files exists in the current app.
- Agreement Related Documents (read-only, patched from sanction): columns = Document Name, File (filename link).
- Customer Account Related Documents (read-only, patched from account/KYC): columns = Document Name, File (filename link).
- List page grid: columns = Reference ID, Entry Date, Borrower Name, A/C No, Project, Total Amount, Status, (item count), Action (view).
- OUT-OF-SCOPE for BU initiate (from CAD help HTML 'Disbursement Node', belongs to Accounts/posting node): Disbursement Instruction table (Type, Currency, Amount, Bank, A/C No, Actions) with Cheque/EFT/RTGS/GL Transfer/Pay Order tabs + client instruction (Bank, Branch, Routing No, A/C No).

## Current application coverage

**Backend:** DisbursementRequest (aggregate) + DisbursementGlLine, at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/Disbursement/IDCOL.CBS.Disbursement.Domain/DisbursementRequest.cs and DisbursementGlLine.cs. Initiate() factory (stage 1) params: id, referenceNo, disbursementNo, sanctionId, sanctionRef, customerNo, projectName, loanCurrency, suggestedLoanAmount, suggestedGrantAmount, buRemarks, initiatingUserId. Command: InitiateDisbursementCommand(SanctionId, SanctionRef, CustomerNo, ProjectName, LoanCurrency, SuggestedLoanAmount, SuggestedGrantAmount, BuRemarks) at IDCOL.CBS.Disbursement.Application/Commands/InitiateDisbursement.cs. Controller: DisbursementsController POST api/disbursements/initiate (BU-role gated in handler).

Existing persisted fields: `ReferenceNo`, `DisbursementNo`, `SanctionId`, `SanctionRef`, `CustomerNo`, `ProjectName`, `LoanCurrency`, `Status`, `SuggestedLoanAmount`, `SuggestedGrantAmount`, `BuRemarks`, `InitiatedBy, InitiatedAtUtc, CreatedBy, CreatedAtUtc`

**Frontend:** InitiateDisbursementRequest interface at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/core/disbursements/disbursement.models.ts; entry/list UI at features/disbursements/disbursements.component.ts + .html; service at core/disbursements/disbursement.service.ts.

## Gaps vs. prototype (13)

| Field / concept | Severity | Layers | Recommendation |
|---|---|---|---|
| BU provided Document upload table (Document Name + File, repeating rows) | 🔴 Missing | domain, application, api, angular | Add a DisbursementDocument child entity (Id, DisbursementRequestId, DocumentName, StoredFileRef/Path, ContentType, UploadedBy, UploadedAtUtc) collection on DisbursementRequest, add them via the Initiate command as a list, expose multipart upload (or pre-uploaded file refs) on POST api/disbursements/initiate, and build a repeating Document Name + File add/remove grid in the Angular initiate form. This is the primary genuinely-missing data capture of the BU stage. |
| Disbursement Summary computed values: Already Disbursed Loan, Available Loan Amount, Already Disbursed Grant, Available Grant Amount | 🔴 Missing | application, angular | Add a query (e.g. GetDisbursementContextForSanction) returning sanction loan/grant amount, sum of prior Effective (or Processed) loan/grant disbursements = Already Disbursed, and Available = sanction amount - already disbursed. Render as read-only summary panel on the initiate screen. Purely derived — no new stored column needed, but must exist for the BU to see headroom. |
| Suggested amount must not exceed Available (loan/grant) headroom | 🔴 Missing | domain, application, angular | Add validation that SuggestedLoanAmount <= availableLoan and SuggestedGrantAmount <= availableGrant (using the summary computation). Currently Initiate only checks amounts are non-negative and at least one positive; there is no over-disbursement guard. Enforce in the Initiate handler/domain and surface inline in Angular. |
| Customer Address | 🔴 Missing | angular | Prototype's Customer Information panel shows Address; the current Customer model (lifecycle.models.ts) has no address field. For this read-only patch panel, source address from the customer record — add address to the customer projection/read model or the disbursement context query. Low priority display-only. |
| Account General Information panel (A/C No, A/C Open Date, A/C Expiry Date, System Date) | 🟠 Partial | application, angular | Prototype patches a loan-account object (account no, open/expiry/system date). Current app has no loan-account concept surfaced to disbursement; sanction has no open/expiry-of-account dates. If a loan A/C entity exists elsewhere, join it into the disbursement context query and render the panel; otherwise treat A/C No as the sanction/loan reference and add account open/expiry dates to the read model. Display-only for BU stage. |
| Industry Type (agreement detail) | 🟠 Partial | application, angular | Prototype displays Industry Type in Borrower Agreement Details. CreateSanctionRequest captures industryType but the read Sanction interface/DTO does not expose it. Add industryType to the Sanction read model so the initiate patch panel can show it. Display-only. |
| Interest Rate Type / Grace unit / Moratorium unit / Interest Frequency (agreement details display) | 🟠 Partial | application, angular | The read Sanction interface omits interestRateType, interestGracePeriodUnit, principalMoratoriumUnit, interestFrequency, availabilityPeriodMonths, and credit rating even though CreateSanctionRequest captures most. To fully render the prototype's Borrower Agreement Details patch panel, expand the Sanction read DTO/interface to surface these. Read-only projection; no new capture in the disbursement aggregate. |
| Grant Currency (separate from Loan Currency) | 🟠 Partial | domain, application, angular | Prototype's Customer Account Details shows Grant Currency independent of Loan Currency (sample data has BDT loan / USD grant). DisbursementRequest stores only LoanCurrency; the initiate command has no grant currency. If grant can be a different currency, add GrantCurrency to the aggregate + command; otherwise confirm grant always follows loan currency and document the simplification. |
| Equivalent BDT dual-amount display (foreign amount + BDT equivalent) for loan/grant | 🟠 Partial | domain, application, angular | Prototype handles loanCurrency='Equivalent BDT' rendering foreign currency amount + BDT-equivalent and picks the currency prefix accordingly. Current model has a single LoanCurrency string and single amount, no equivalent/foreign split. If IDCOL disburses FX loans booked in BDT-equivalent, add foreignCurrency + equivalentBdtAmount (and exchange rate) to the disbursement/sanction; else confirm out of scope. |
| Transaction Entry Date / Value Date / A-C Branch at initiation (from CAD help 'Node Information') | 🟠 Partial | domain, application, angular | The CAD-authored help screen captures Transaction Entry Date, Value Date and A/C Branch on the node. In the current app ValueDate is only set at the Post (Accounts) stage and there is no branch/entry-date at all. Decide with CAD whether BU initiate should capture entry date/branch; if yes add EntryDate + BranchCode to the initiate command/aggregate. (Value date at BU stage likely remains a posting concern.) |
| List page columns: Entry Date, Borrower Name, Total Amount, item/tranche count | 🟠 Partial | application, angular | Prototype list shows Entry Date, Borrower Name (not just CustomerNo), a combined Total Amount, and an items count. The DisbursementDto exposes CustomerNo (no borrower name), no created/entry date, and separate loan/grant amounts (no combined total or item count). Add CreatedAtUtc (entry date), a resolved borrower/customer name, and a total/effective amount to the list DTO; render those columns in the Angular list. Also add search + status filter + pagination to match the prototype list. |
| Status vocabulary alignment (Suggested vs 'In Review') | 🟡 Naming | angular | Prototype list uses status labels 'Suggested' and 'In Review'; the backend uses Suggested \| Proposed \| Processed. Map 'Proposed' -> display 'In Review' (or align labels) so the BU list reads consistently with the approved prototype. Naming/label only — the underlying state machine already exists. |
| 'Clear' and 'Save Draft' actions / Draft status | 🟠 Partial | domain, application, angular | Prototype/help expose Clear and (in help) Save Draft with a 'Draft' status before Submit; current app only has Submit (creates directly as 'Suggested'). If BU needs to persist an incomplete draft, add a 'Draft' status and a save-draft path; otherwise implement Clear as a pure client-side reset and drop Draft. Confirm with CAD which is intended. |

