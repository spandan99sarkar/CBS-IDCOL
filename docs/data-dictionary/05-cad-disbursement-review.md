# Data Dictionary — CAD Disbursement Review

> **Department:** CAD  ·  **App route:** `/disbursements`  ·  **Backend module:** Disbursement (Stage 2 – CAD)

## Purpose

The CAD Disbursement Entry page (route /cad/entry, header "Propose Disbursement") is the CAD reviewer's screen for the middle stage of IDCOL's disbursement maker-checker. Flow: BU "Suggests" (Call For Disbursement) -> CAD "Proposes" (this page: justify & review) -> Accounts "Processes" (posts GL). The page is driven by a single top selector "Select Customer A/C" which auto-patches a large body of READ-ONLY reference data pulled from the customer account + signed sanction agreement, grouped into collapsible sections: (1) Account General Information [A/C No, A/C Open Date, A/C Expiry Date, System Date]; (2) Customer Information [Customer ID, Name, Mobile, Email, Sector Code, Address]; (3) Borrower Agreement Details — Core Info [Sanction ID, Business Unit, Product, Project, Industry Type, Loan Amount (with foreign/equivalent-BDT dual display), Grant Amount (dual display), Agreement Date, Availability Period (Date), Availability Period (Months)], Repayment & Interest Info [Interest Rate Type, Initial Interest Rate, Loan Tenor, No of Principal Repayments, Interest Grace Period + unit, Principal Moratorium Period + unit, Repayment Method, Principal Frequency, Interest Frequency, Day Count Basis], Other Details [Initiated by (BU), Approved by (BU), Initiated by (CRM), Approved by (CRM), Credit Rating, Remarks]; (4) Customer Account Details [Loan Currency, Loan Amount, Loan Type, Grant Amount]; (5) Agreement Related Documents table [Document Name, File]; (6) Customer Account Related Documents table [Document Name, File]; (7) Call For Disbursement (BU) — the BU-provided suggestion this CAD stage reviews [BU Suggested Loan Disbursement Amount, BU Suggested Grant Disbursement Amount, Remarks]; (8) Disbursement Summary — computed utilization [Already Disbursed Loan, Available Loan Amount, Already Disbursed Grant, Available Grant Amount] plus a read-only Previous Disbursements table [No, Sub No, Type, Source of Fund, Credit Line, Source of Disbursement, Amount, Remarks]. The two EDITABLE data-entry structures CAD fills in are: (A) the "Justify & Review Disbursement" repeating table (add/remove rows) — the heart of the CAD stage — and (B) the "CAD Provided Documents" upload table (add/remove rows). Workflow actions: "Clear" and "Submit" (Submit navigates to the CAD list, i.e. moves the request to Proposed / In Review). The list page (route /cad/list, "Proposed Disbursements") shows columns [Reference ID, Entry Date, Borrower Name, A/C No, Project, Total Amount, Status, Action(view)] with a search box + Filter button + pagination, and status values "Suggested" and "In Review".

## Fields (63)

| Field | Type | Options / Values | Section | Source | Notes |
|---|---|---|---|---|---|
| `Select Customer A/C` | select | 1002345678 (ABC Corporation Ltd.), 1002345679 (John Doe) | Top Selector | user input | Drives auto-patch of all reference sections. In real system this is the loan/customer account whose signed sanction is being disbursed. |
| `A/C No` | computed |  | Account General Information | computed | Read-only, patched from account. |
| `A/C Open Date` | computed |  | Account General Information | computed |  |
| `A/C Expiry Date` | computed |  | Account General Information | computed |  |
| `System Date` | computed |  | Account General Information | computed |  |
| `Customer ID` | computed |  | Customer Information | computed |  |
| `Customer Name` | computed |  | Customer Information | computed |  |
| `Mobile No` | computed |  | Customer Information | computed |  |
| `Email Address` | computed |  | Customer Information | computed |  |
| `Sector Code` | computed |  | Customer Information | computed |  |
| `Address` | computed |  | Customer Information | computed |  |
| `Sanction ID` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Business Unit` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Product` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Project` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Industry Type` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Loan Amount (dual: foreign / equivalent BDT)` | computed |  | Borrower Agreement Details / Core | computed | When loanCurrency=='Equivalent BDT' shows foreign amount + BDT equivalent; needs loanCurrency, loanCurrencyForeign, loanAmount, loanAmountEquivalent. |
| `Grant Amount (dual: foreign / equivalent BDT)` | computed |  | Borrower Agreement Details / Core | computed | Same dual-currency treatment as loan; needs grantCurrency, grantCurrencyForeign, grantAmount, grantAmountEquivalent. |
| `Agreement Date` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Availability Period (Date)` | computed |  | Borrower Agreement Details / Core | computed | Displayed from agreement.expiryDate. |
| `Availability Period (Months)` | computed |  | Borrower Agreement Details / Core | computed |  |
| `Interest Rate Type` | computed | Fixed, Float | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Initial Interest Rate` | computed |  | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Loan Tenor` | computed |  | Borrower Agreement Details / Repayment & Interest | computed |  |
| `No of Principal Repayments` | computed |  | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Interest Grace Period (+ unit)` | computed |  | Borrower Agreement Details / Repayment & Interest | computed | Value + unit (e.g. Month). |
| `Principal Moratorium Period (+ unit)` | computed |  | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Repayment Method` | computed | Level Principal, EMI | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Principal Frequency` | computed | Quarterly, Monthly | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Interest Frequency` | computed | Monthly | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Day Count Basis` | computed | 360, 365 | Borrower Agreement Details / Repayment & Interest | computed |  |
| `Initiated by (BU)` | computed |  | Borrower Agreement Details / Other | computed |  |
| `Approved by (BU)` | computed |  | Borrower Agreement Details / Other | computed |  |
| `Initiated by (CRM)` | computed |  | Borrower Agreement Details / Other | computed |  |
| `Approved by (CRM)` | computed |  | Borrower Agreement Details / Other | computed |  |
| `Credit Rating` | computed |  | Borrower Agreement Details / Other | computed |  |
| `Agreement Remarks` | computed |  | Borrower Agreement Details / Other | computed |  |
| `Loan Currency` | computed |  | Customer Account Details | computed |  |
| `Loan Amount` | computed |  | Customer Account Details | computed | Dual-currency display as above. |
| `Loan Type` | computed | New | Customer Account Details | computed | Prototype data only shows 'New'; likely New/Renewal/Enhancement in full system. |
| `Grant Amount` | computed |  | Customer Account Details | computed |  |
| `Agreement Documents (table)` | table-line-item |  | Agreement Related Documents | computed | Read-only patched list; columns Document Name, File(name). |
| `Customer Account Documents (table)` | table-line-item |  | Customer Account Related Documents | computed | Read-only patched list; columns Document Name, File(name). |
| `BU Suggested Loan Disbursement Amount` | computed |  | Call For Disbursement (BU) | computed | The BU 'Suggested' amount this CAD stage justifies/proposes against. Maps to domain SuggestedLoanAmount. |
| `BU Suggested Grant Disbursement Amount` | computed |  | Call For Disbursement (BU) | computed | Maps to domain SuggestedGrantAmount. |
| `BU Call Remarks` | computed |  | Call For Disbursement (BU) | computed | Maps to domain BuRemarks. |
| `Already Disbursed Loan` | computed |  | Disbursement Summary | computed | Cumulative loan already disbursed on this sanction. Not tracked in current app. |
| `Available Loan Amount` | computed |  | Disbursement Summary | computed | Sanction loan amount minus already-disbursed; the ceiling CAD must not exceed. |
| `Already Disbursed Grant` | computed |  | Disbursement Summary | computed |  |
| `Available Grant Amount` | computed |  | Disbursement Summary | computed |  |
| `Previous Disbursements (table)` | table-line-item |  | Disbursement Summary | computed | Read-only history; columns No, Sub No, Type, Source of Fund, Credit Line, Source of Disbursement, Amount, Remarks. |
| `Justify Row - No.` | text |  | Justify & Review Disbursement (EDITABLE tranche table) | user input | Disbursement number; auto-increments per group. Domain has scalar DisbursementNo but not per-line. |
| `Justify Row - Sub No.` | text |  | Justify & Review Disbursement | user input | Roman-numeral sub-tranche (i, ii, iii) allowing multiple lines under one disbursement no. No equivalent in current app. |
| `Justify Row - Type` | select | Loan, Grant | Justify & Review Disbursement | user input | Per-line Loan vs Grant classification. Current app splits by two fixed scalar amount fields, not per-line. |
| `Justify Row - Source of Fund` | select | Internal, Donor Fund | Justify & Review Disbursement | user input | Not captured anywhere in current app. |
| `Justify Row - Credit Line` | select | Line A, Line B | Justify & Review Disbursement | user input | Not captured in current app. |
| `Justify Row - Amount` | number |  | Justify & Review Disbursement | user input | Per-line justified amount. Current app has only aggregate JustifiedLoanAmount/JustifiedGrantAmount scalars. |
| `Justify Row - Remarks` | text |  | Justify & Review Disbursement | user input | Per-line remark. Current app has single CadRemarks scalar. |
| `CAD Provided Documents - Document Name` | text |  | CAD Provided Documents (EDITABLE upload table) | user input | Free-text name of a CAD-attached document. |
| `CAD Provided Documents - File` | file |  | CAD Provided Documents | user input | File upload input; add/remove rows. No document/attachment support in current app disbursement module. |
| `List - Search` | text |  | CAD List page | user input | Search by ID, Borrower or Status. |
| `List - Filter` | select |  | CAD List page | user input | Filter button (unbound in prototype). |
| `List - Status` | computed | Suggested, In Review | CAD List page | user input | Prototype list uses 'In Review' where domain uses 'Proposed'; also shows 'items' count per request (number of tranche lines). |

## Repeating / line-item tables

- Justify & Review Disbursement (EDITABLE): columns No. \| Sub No. \| Type (Loan\|Grant) \| Source of Fund (Internal\|Donor Fund) \| Credit Line (Line A\|Line B) \| Amount \| Remarks \| Action(delete). Add Row / remove-row supported.
- CAD Provided Documents (EDITABLE): columns Document Name \| File(upload) \| Action(delete). Add Document / remove supported.
- Previous Disbursements (READ-ONLY): columns No. \| Sub No. \| Type (Loan\|Grant) \| Source of Fund \| Credit Line \| Source of Disbursement (e.g. Bank Transfer) \| Amount \| Remarks.
- Agreement Related Documents (READ-ONLY): columns Document Name \| File.
- Customer Account Related Documents (READ-ONLY): columns Document Name \| File.
- CAD List (READ-ONLY): columns Reference ID \| Entry Date \| Borrower Name \| A/C No \| Project \| Total Amount \| Status \| items-count \| Action(view).

## Current application coverage

**Backend:** DisbursementRequest (aggregate) + DisbursementGlLine (child) at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/Disbursement/IDCOL.CBS.Disbursement.Domain/DisbursementRequest.cs and DisbursementGlLine.cs. CAD stage handled by Propose(cadUserId, justifiedLoanAmount, justifiedGrantAmount, cadRemarks). Application: ReviewDisbursementCommand(DisbursementId, JustifiedLoanAmount, JustifiedGrantAmount, CadRemarks) at .../Application/Commands/ReviewDisbursement.cs. API: DisbursementsController POST /api/disbursements/{id}/review at .../host/IDCOL.CBS.Api/Controllers/DisbursementsController.cs. EF: DisbursementRequestConfiguration.cs (table DISB_REQUEST) at .../LoanLifecycle.Infrastructure/Persistence/Configurations/.

Existing persisted fields: `ReferenceNo`, `DisbursementNo`, `SanctionId`, `SanctionRef`, `CustomerNo`, `ProjectName`, `LoanCurrency`, `Status`, `SuggestedLoanAmount`, `SuggestedGrantAmount`, `BuRemarks`, `InitiatedBy / InitiatedAtUtc`, `JustifiedLoanAmount`, `JustifiedGrantAmount`, `CadRemarks`, `ProposedBy / ProposedAtUtc`, `DisbursementMode / ValueDate / PostedBy / PostedAtUtc`, `EffectiveLoanAmount / EffectiveGrantAmount`, `GlLines: GlCode, GlDescription, Debit, Credit`, `Audit: CreatedBy/At, LastModifiedBy/At`, `Structural maker-checker enforcement`

**Frontend:** Disbursement / ReviewDisbursementRequest interfaces at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/core/disbursements/disbursement.models.ts; component E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/features/disbursements/disbursements.component.ts (+ .html). CAD review is a 3-field inline panel (startReview/submitReview).

## Gaps vs. prototype (14)

| Field / concept | Severity | Layers | Recommendation |
|---|---|---|---|
| Justify & Review multi-line tranche table (No / Sub No / Type / Source of Fund / Credit Line / Amount / Remarks per line) | 🔴 Missing | domain, application, api, angular | This is the core CAD deliverable and is entirely absent. Add a child collection DisbursementJustificationLine (LineNo, SubNo, Type[Loan\|Grant], SourceOfFund[Internal\|Donor Fund], CreditLine, Amount, Remarks) on DisbursementRequest, populated in Propose(). Replace scalar JustifiedLoanAmount/JustifiedGrantAmount+CadRemarks with (or derive them from) this line collection: JustifiedLoanAmount = sum of Loan-type lines, JustifiedGrantAmount = sum of Grant-type lines. Extend ReviewDisbursementCommand to carry IReadOnlyList<JustificationLineInput>, add EF config + table, expose columns in the Angular CAD entry page as an editable add/remove grid. Requires new lookup master data for Source of Fund and Credit Line. |
| Line Type (Loan\|Grant) per justification line | 🟠 Partial | domain, application, api, angular | Prototype classifies each tranche line as Loan or Grant via a dropdown; current app only has two fixed aggregate amount fields, so a request cannot express mixed multi-line Loan+Grant tranches with individual amounts/remarks. Introduce a per-line Type enum as part of the justification-line collection above. |
| Source of Fund (Internal \| Donor Fund) | 🔴 Missing | domain, application, api, angular | Not captured anywhere. Add per-line SourceOfFund field (enum/lookup with at least Internal, Donor Fund) to the justification line entity, command, DTO and Angular grid. Needed for donor-fund tracking/reporting. |
| Credit Line (Line A \| Line B) | 🔴 Missing | domain, application, api, angular | Not captured. Add per-line CreditLine field (lookup to a credit-line master) to the justification line entity, command, DTO and Angular grid. Likely FK to a credit-line/facility master table. |
| Disbursement No + Sub No (roman-numeral sub-tranches) | 🟠 Partial | domain, application, api, angular | Domain has a single scalar DisbursementNo per request; prototype needs a (No, SubNo=i/ii/iii) pair per line to support multiple sub-tranches under one disbursement number. Move No/SubNo onto the justification-line collection with auto-increment logic mirroring the prototype (SubNo advances i->ii->iii, No increments when SubNo resets). |
| CAD Provided Documents (upload table: Document Name + File, add/remove) | 🔴 Missing | domain, application, api, angular | No attachment/document capability exists in the disbursement module. Add a document collection (Name, StoredFileRef/Path, ContentType, UploadedBy/At) captured at the CAD Propose stage, a multipart or document-service upload endpoint, and an editable upload grid in the Angular CAD page. Confirm whether a shared document/attachment service already exists elsewhere to reuse. |
| Disbursement utilization summary (Already Disbursed Loan / Available Loan / Already Disbursed Grant / Available Grant) | 🔴 Missing | domain, application, api, angular | Prototype shows CAD the sanction ceiling vs cumulative disbursed so the reviewer cannot over-disburse. Current app tracks no cumulative/available figures and enforces no ceiling. Add an application query (or projection) that sums prior Proposed/Processed disbursements against the sanction loan/grant amounts and returns AlreadyDisbursed + Available for loan and grant; ideally add a domain validation in Propose() that justified totals cannot exceed the available amount. |
| Previous Disbursements history table (No, Sub No, Type, Source of Fund, Credit Line, Source of Disbursement, Amount, Remarks) | 🔴 Missing | application, api, angular | CAD needs read-only visibility of prior disbursements for the same sanction. Add a query returning prior disbursement lines for the sanction (including a 'Source of Disbursement' e.g. Bank Transfer field not currently modeled) and render as a read-only table on the CAD entry page. Note the extra 'Source of Disbursement' column has no domain field yet. |
| Sanction/agreement reference section (Business Unit, Industry Type, Product, Loan Type, dual-currency foreign+equivalent-BDT amounts, Availability Period date & months, Interest Rate Type, grace/moratorium period units, Initiated/Approved by BU & CRM, Credit Rating, agreement remarks) | 🟠 Partial | application, api, angular | The prototype auto-patches a rich read-only agreement panel; the current app's Sanction model (lifecycle.models.ts) lacks several of these: businessUnit, industryType(read but not on Sanction read model), loanType, loanCurrencyForeign/loanAmountEquivalent (dual-currency equivalent), grantCurrency/grantCurrencyForeign/grantAmountEquivalent, availabilityPeriodMonths, interestGracePeriodUnit, principalMoratoriumUnit, initiatedByBU, approvedByBU, initiatedByCRM, approvedByCRM, and agreement-level remarks. Expose these via a disbursement 'context' query (join sanction+customer+account) so the CAD page can render them, and add the missing ones to the sanction/read model where absent. |
| Account General Information (A/C No, Open Date, Expiry Date, System Date) and Customer Information (Customer ID, Name, Mobile, Email, Sector Code, Address) reference panels | 🔴 Missing | application, api, angular | The current disbursement DTO carries only CustomerNo/ProjectName. Provide a CAD-page context query that returns the loan-account header (account no, open/expiry/system dates) and customer profile (id, name, mobile, email, sector code, address) so the reviewer sees who/what they are disbursing to. Most fields already exist on Customer entity; account-level open/expiry/system dates need a loan-account source. |
| Agreement Related Documents & Customer Account Related Documents (read-only patched lists) | 🔴 Missing | application, api, angular | CAD reviewer must see sanction/account documents to justify. Add read-only document lists (Document Name, File link) to the CAD context query, sourced from the sanction/customer document stores. Depends on a document service being present. |
| Status label 'In Review' vs domain 'Proposed' / list 'items' count | 🟡 Naming | application, api, angular | Prototype list uses 'Suggested' and 'In Review'; domain/status uses 'Suggested','Proposed','Processed'. Align the Angular status badge/label mapping (display 'In Review' for 'Proposed' if that is the client's preferred wording, or reconcile terminology with CAD head). Also add an 'items' (tranche line count) and Total Amount + Entry Date to the list DTO/columns which the current list lacks. |
| Dedicated CAD 'Propose Disbursement' entry page with customer-account selector + auto-patch | 🔴 Missing | angular | Current Angular renders CAD review as a 3-field inline panel inside a shared disbursements list. The approved UX is a full-page CAD entry screen driven by a 'Select Customer A/C' selector that patches all reference sections and hosts the editable justify grid + document upload. Build a dedicated CADDisbursementEntry component/route and a CAD list component matching the prototype's columns and status filter. |
| GL lines authored/owned at CAD stage | 🟠 Semantics | angular | No gap to add at CAD stage — note only: the prototype's editable data at the CAD (Propose) stage is the justification tranche table + documents, NOT GL journal lines. GL lines belong to the Accounts (Post) stage. Current app already correctly places GL lines at the Post stage (though hardcoded Dr102030/Cr202030). Do NOT move GL entry into the CAD stage; keep the CAD stage focused on the justification lines above. |

