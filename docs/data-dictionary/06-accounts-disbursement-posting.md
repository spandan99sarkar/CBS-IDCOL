# Data Dictionary — Accounts Disbursement Posting

> **Department:** Accounts  ·  **App route:** `/disbursements`  ·  **Backend module:** Disbursement (Stage 3 – Accounts) + GL

## Purpose

AccountsDisbursementEntry.tsx is the Accounts "Process Disbursement" screen — the third and final maker-checker stage where the Accounts department verifies CAD/BU-provided data, builds the actual multi-line disbursement, captures the client payment instruction, and posts a balanced GL voucher. It is composed of: (1) A CAD-info banner declaring all blue sections READ ONLY. (2) Seven collapsible read-only auto-patched sections: Account General Information, Customer Information, Borrower Agreement Details (Core Info / Repayment & Interest Info / Other Details), Customer Account Details, Agreement Related Documents (table), Customer Account Related Documents (table), and Call For Disbursement (BU) showing BU suggested loan/grant amounts + remarks. (3) 'Disbursement Suggestions (CAD)' — a READ ONLY, INTEGRITY-CHECK-tagged selectable table of CAD-proposed tranche lines (Select checkbox, No, Sub No, Type, Source of Fund, Credit Line, Amount, Remarks) that seed the final disbursement. (4) 'General Instruction' — editable Transaction Entry Date + Remarks. (5) 'Disbursement Summary' (collapsible, historical) — four aggregate figures (Already Disbursed Loan, Available Loan Amount, Already Disbursed Grant, Available Grant Amount) plus a read-only Previous Disbursements table (No, Sub No, Type, Source of Fund, Credit Line, Source of Disbursement, Amount, Remarks). (6) 'Final Disbursement' — the core editable area: a 5-tab payment-instrument selector (Cheque, SWIFT, EFT, RTGS, Pay Order), an editable multi-row Final Disbursement Details line-item table with Add Row / delete and a computed Total Disbursement Amount, and a per-instrument 'Client Instruction' field group that changes by active tab, plus Borrower Receive Date. (7) 'Transaction Details' — a manual double-entry voucher with add-able Debit Side and Credit Side GL/account+amount rows and computed Total Debit / Total Credit / Difference. (8) Actions: Clear, Submit (workflow submit), and a 'See Voucher' button. Workflow/status: the list page (AccountsDisbursementList.tsx) shows Suggested Disbursements with status Pending|Processed and dept CAD|Accounts, and a 'Process' action routing into this entry page; the entry page's Submit transitions the request to Processed (posts to GL). Note the prototype's amount modelling supports 'Equivalent BDT' dual-currency display (foreign amount + BDT equivalent) for both loan and grant.

## Fields (77)

| Field | Type | Options / Values | Section | Source | Notes |
|---|---|---|---|---|---|
| `Account A/C No` | text (read-only, CAD-provided) |  | Account General Information | computed | Auto-patched from account |
| `A/C Open Date` | date (read-only) |  | Account General Information | computed |  |
| `A/C Expiry Date` | date (read-only) |  | Account General Information | computed |  |
| `System Date` | date (read-only) |  | Account General Information | computed |  |
| `Customer ID` | text (read-only) |  | Customer Information | computed |  |
| `Customer Name` | text (read-only) |  | Customer Information | computed |  |
| `Mobile No` | text (read-only) |  | Customer Information | computed |  |
| `Email Address` | text (read-only) |  | Customer Information | computed |  |
| `Sector Code` | text (read-only) |  | Customer Information | computed |  |
| `Address` | text (read-only) |  | Customer Information | computed |  |
| `Sanction ID` | text (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Business Unit` | text (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Product` | text (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Project` | text (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Industry Type` | text (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Loan Amount (foreign + BDT equivalent)` | computed dual-currency |  | Borrower Agreement Details - Core | computed | Displays 'USD x / BDT y' when loanCurrency=='Equivalent BDT', else single currency |
| `Grant Amount (foreign + BDT equivalent)` | computed dual-currency |  | Borrower Agreement Details - Core | computed |  |
| `Agreement Date` | date (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Availability Period (Date)` | date (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Availability Period (Months)` | number (read-only) |  | Borrower Agreement Details - Core | computed |  |
| `Interest Rate Type` | text (read-only) | Fixed, Floating | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Initial Interest Rate` | number% (read-only) |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Loan Tenor (Months)` | number (read-only) |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `No of Principal Repayments` | number (read-only) |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Interest Grace Period + Unit` | number+unit (read-only) |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Principal Moratorium Period + Unit` | number+unit (read-only) |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Repayment Method` | text (read-only) | Level Principal, Equal Installment (EMI) | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Principal Frequency` | text (read-only) | Monthly, Quarterly, Half-Yearly, Yearly | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Interest Frequency` | text (read-only) |  | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Day Count Basis` | text (read-only) | 360, 365, Actual/Actual | Borrower Agreement Details - Repayment/Interest | computed |  |
| `Initiated by (BU)` | text (read-only) |  | Borrower Agreement Details - Other | computed |  |
| `Approved by (BU)` | text (read-only) |  | Borrower Agreement Details - Other | computed |  |
| `Initiated by (CRM)` | text (read-only) |  | Borrower Agreement Details - Other | computed |  |
| `Approved by (CRM)` | text (read-only) |  | Borrower Agreement Details - Other | computed |  |
| `Credit Rating` | text (read-only) |  | Borrower Agreement Details - Other | computed |  |
| `Agreement Remarks` | text (read-only) |  | Borrower Agreement Details - Other | computed |  |
| `Account Details: Loan Currency` | text (read-only) |  | Customer Account Details | computed |  |
| `Account Details: Loan Amount (dual-currency)` | computed |  | Customer Account Details | computed |  |
| `Account Details: Loan Type` | text (read-only) | New, Additional/Top-up, Restructured | Customer Account Details | computed |  |
| `Account Details: Grant Amount (dual-currency)` | computed |  | Customer Account Details | computed |  |
| `BU Suggested Loan Disbursement Amount` | currency (read-only) |  | Call For Disbursement (BU) | computed |  |
| `BU Suggested Grant Disbursement Amount` | currency (read-only) |  | Call For Disbursement (BU) | computed |  |
| `BU Call Remarks` | text (read-only) |  | Call For Disbursement (BU) | computed |  |
| `CAD Suggestion: Select` | checkbox |  | Disbursement Suggestions (CAD) | user input | Selects which CAD tranche lines seed the final disbursement; INTEGRITY CHECK |
| `Transaction Entry Date` | date |  | General Instruction | user input | Editable; the GL value/posting date entered by Accounts |
| `General Instruction Remarks` | textarea |  | General Instruction | user input |  |
| `Already Disbursed Loan` | computed currency |  | Disbursement Summary | computed |  |
| `Available Loan Amount` | computed currency |  | Disbursement Summary | computed | Sanctioned loan minus already disbursed |
| `Already Disbursed Grant` | computed currency |  | Disbursement Summary | computed |  |
| `Available Grant Amount` | computed currency |  | Disbursement Summary | computed |  |
| `Payment Instrument Tab (Mode)` | tab select | Cheque, SWIFT, EFT, RTGS, Pay Order | Final Disbursement | user input | Drives which Client Instruction fields are shown; equivalent to disbursement mode |
| `Final line: No` | text (auto) |  | Final Disbursement Details table | computed | Auto-incremented main sequence |
| `Final line: Sub No` | text (auto) |  | Final Disbursement Details table | computed | Roman i/ii/iii sub-sequence |
| `Final line: Type` | select | Loan, Grant | Final Disbursement Details table | user input |  |
| `Final line: Source of Fund` | select | Internal, Donor Fund | Final Disbursement Details table | user input |  |
| `Final line: Credit Line` | select | Line A, Line B | Final Disbursement Details table | user input |  |
| `Final line: Currency` | select | BDT, USD, EUR | Final Disbursement Details table | user input |  |
| `Final line: FI Bank A/C No.` | text |  | Final Disbursement Details table | user input |  |
| `Final line: Amount` | number |  | Final Disbursement Details table | user input |  |
| `Final line: Remarks` | text |  | Final Disbursement Details table | user input |  |
| `Total Disbursement Amount` | computed |  | Final Disbursement Details table | computed | Sum of all final line amounts |
| `Client Instruction: Bank` | select | Sonali Bank, Janata Bank, Agrani Bank, City Bank, Standard Chartered | Final Disbursement - Client Instruction | user input | Shown for Cheque/SWIFT/EFT/RTGS |
| `Client Instruction: Branch` | select | Main Branch, Gulshan Branch, Motijheel Branch, Banani Branch | Final Disbursement - Client Instruction | user input | Cheque/SWIFT/EFT/RTGS |
| `Client Instruction: Routing No.` | text |  | Final Disbursement - Client Instruction | user input | Cheque/SWIFT/EFT/RTGS |
| `Client Instruction: A/C No.` | text |  | Final Disbursement - Client Instruction | user input | All modes except SWIFT (which uses Corresponding A/C No.); Pay Order also has its own A/C No. |
| `Client Instruction: Corresponding A/C No.` | text |  | Final Disbursement - Client Instruction | user input | SWIFT only |
| `Client Instruction: Cheque Leaf No.` | text |  | Final Disbursement - Client Instruction | user input | Cheque only |
| `Client Instruction: Beneficiary Name` | text |  | Final Disbursement - Client Instruction | user input | Cheque and Pay Order |
| `Client Instruction: Pay Order No.` | text |  | Final Disbursement - Client Instruction | user input | Pay Order only |
| `Borrower Receive Date` | date |  | Final Disbursement - Client Instruction | user input | Shown for all modes |
| `Debit line: Account/GL` | select | 102030 - Loan Disbursement, 102040 - Grant Disbursement | Transaction Details (voucher) | user input |  |
| `Debit line: Amount` | number |  | Transaction Details (voucher) | user input |  |
| `Credit line: Account/GL` | select | 202030 - Bank Account, 202040 - Cash Account | Transaction Details (voucher) | user input |  |
| `Credit line: Amount` | number |  | Transaction Details (voucher) | user input |  |
| `Total Debit` | computed |  | Transaction Details (voucher) | computed |  |
| `Total Credit` | computed |  | Transaction Details (voucher) | computed |  |
| `Difference (Debit-Credit)` | computed |  | Transaction Details (voucher) | computed | Must be 0 for a balanced voucher |

## Repeating / line-item tables

- Disbursement Suggestions (CAD) [read-only, selectable]: columns = Select(checkbox), No, Sub No, Type(Loan\|Grant), Source of Fund, Credit Line, Amount, Remarks
- Previous Disbursements [read-only, historical]: columns = No, Sub No, Type(Loan\|Grant), Source of Fund, Credit Line, Source of Disbursement, Amount, Remarks
- Final Disbursement Details [editable]: columns = No(auto), Sub No(auto), Type(Loan\|Grant), Source of Fund(Internal\|Donor Fund), Credit Line(Line A\|Line B), Currency(BDT\|USD\|EUR), FI Bank A/C No., Amount, Remarks, + Total Disbursement Amount footer
- Transaction Details voucher [editable]: Debit Side rows (Account/GL, Amount) and Credit Side rows (Account/GL, Amount) with Total Debit / Total Credit / Difference
- Agreement Related Documents [read-only]: columns = Document Name, File
- Customer Account Related Documents [read-only]: columns = Document Name, File

## Current application coverage

**Backend:** DisbursementRequest (aggregate root) + DisbursementGlLine (child entity) at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/Disbursement/IDCOL.CBS.Disbursement.Domain/DisbursementRequest.cs and DisbursementGlLine.cs. Stage-3 posting is the Post(accountsUserId, disbursementMode, valueDate, glLines) method. Application: PostDisbursementCommand + GlLineInput (Commands/PostDisbursement.cs); DisbursementDto + DisbursementGlLineDto (Queries/ListDisbursements.cs). API: DisbursementsController POST api/disbursements/{id}/post. EF: DisbursementRequestConfiguration.cs (tables DISB_REQUEST, DISB_GL_LINE).

Existing persisted fields: `DisbursementRequest.ReferenceNo`, `DisbursementRequest.DisbursementNo`, `DisbursementRequest.SanctionId`, `DisbursementRequest.SanctionRef`, `DisbursementRequest.CustomerNo`, `DisbursementRequest.ProjectName`, `DisbursementRequest.LoanCurrency`, `DisbursementRequest.Status`, `DisbursementRequest.SuggestedLoanAmount`, `DisbursementRequest.SuggestedGrantAmount`, `DisbursementRequest.BuRemarks`, `DisbursementRequest.InitiatedBy`, `DisbursementRequest.InitiatedAtUtc`, `DisbursementRequest.JustifiedLoanAmount`, `DisbursementRequest.JustifiedGrantAmount`, `DisbursementRequest.CadRemarks`, `DisbursementRequest.ProposedBy`, `DisbursementRequest.ProposedAtUtc`, `DisbursementRequest.DisbursementMode`, `DisbursementRequest.ValueDate`, `DisbursementRequest.PostedBy`, `DisbursementRequest.PostedAtUtc`, `DisbursementRequest.EffectiveLoanAmount/EffectiveGrantAmount`, `DisbursementRequest.Created/LastModified audit fields`, `DisbursementGlLine.GlCode`, `DisbursementGlLine.GlDescription`, `DisbursementGlLine.Debit`, `DisbursementGlLine.Credit`

**Frontend:** disbursement.models.ts + disbursement.service.ts (E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/core/disbursements/) and disbursements.component.ts/.html (features/disbursements/). Single combined component covers all 3 stages as an inline table with expandable Review/Post panels.

## Gaps vs. prototype (21)

| Field / concept | Severity | Layers | Recommendation |
|---|---|---|---|
| Final Disbursement Details line-item table (multi-row: No, Sub No, Type, Source of Fund, Credit Line, Currency, FI Bank A/C No., Amount, Remarks) | 🔴 Missing | domain, application, api, angular | Introduce a child collection DisbursementLine on DisbursementRequest (fields: LineNo, SubNo, Type[Loan\|Grant], SourceOfFund, CreditLine, Currency, FiBankAcNo, Amount, Remarks). The prototype's actual disbursement is a set of tranche lines, not one scalar loan+grant amount. Add EF config + DISB_LINE table, capture lines in PostDisbursementCommand, and replace the Angular single-amount post panel with the editable Add-Row line-item grid + computed Total Disbursement Amount. |
| Source of Fund (per final line: Internal \| Donor Fund) | 🔴 Missing | domain, application, api, angular | Add SourceOfFund to the new DisbursementLine entity as a constrained value (Internal\|Donor Fund). It appears in CAD suggestions, previous disbursements, and final disbursement — a first-class dimension not present anywhere in the current model. |
| Credit Line (per final line: Line A \| Line B) | 🔴 Missing | domain, application, api, angular | Add CreditLine to DisbursementLine (ideally a FK to a CreditLine master rather than a free enum, since prototype shows 'Line A (IDA-678)' style codes). Required to attribute disbursements against donor/funding credit lines. |
| Per-line Currency (BDT \| USD \| EUR) and dual-currency (Equivalent BDT) foreign+BDT amounts | 🟠 Partial | domain, application, api, angular | Current model has one LoanCurrency string on the request and no foreign/equivalent split. Add per-line Currency plus support for the prototype's 'Equivalent BDT' dual-amount concept (foreign amount + BDT-equivalent). At minimum add Currency to DisbursementLine; ideally add ForeignAmount + EquivalentBdtAmount + ExchangeRate. |
| FI Bank A/C No. (per final line) | 🔴 Missing | domain, application, api, angular | Add FiBankAcNo to DisbursementLine — the funding-source bank account the disbursement is drawn from. Not currently captured. |
| Type per line (Loan \| Grant) as a line dimension | 🟠 Partial | domain, application, api, angular | Current model splits loan vs grant only as two scalar totals (Suggested/Justified Loan/Grant amounts). Prototype treats Loan/Grant as a per-line Type so a single disbursement can mix multiple loan and grant tranches. Add Type to DisbursementLine; derive loan/grant totals from lines. |
| No / Sub No line numbering (main sequence + roman sub-sequence i/ii/iii) | 🔴 Missing | domain, application, angular | Persist LineNo and SubNo on each DisbursementLine; the prototype auto-generates these (main increments, sub cycles i->ii->iii). Needed to match the No/Sub No shown in CAD suggestions, previous disbursements, and final lines. |
| Client Instruction / payment instrument details (Bank, Branch, Routing No., A/C No., Corresponding A/C No. [SWIFT], Cheque Leaf No., Pay Order No., Beneficiary Name) | 🔴 Missing | domain, application, api, angular | The app persists only DisbursementMode as a bare enum with zero instrument-specific data. Add a PaymentInstruction value object / owned entity keyed by mode capturing Bank, Branch, RoutingNo, AcNo, CorrespondingAcNo (SWIFT), ChequeLeafNo (Cheque), PayOrderNo (Pay Order), BeneficiaryName (Cheque/Pay Order). Angular must render mode-conditional fields exactly like the 5 tabs (Cheque/SWIFT/EFT/RTGS/Pay Order). |
| Borrower Receive Date | 🔴 Missing | domain, application, api, angular | Add BorrowerReceiveDate (DateOnly) to the posting payload — the date the borrower actually receives the instrument, distinct from ValueDate/Transaction Entry Date. |
| Transaction Entry Date (General Instruction) vs ValueDate | 🟠 Semantics | application, angular | Prototype has a separate editable 'Transaction Entry Date' in General Instruction plus 'Value Date' semantics via posting. Confirm mapping: the current single ValueDate likely corresponds to Transaction Entry Date. Clarify and, if both are needed (entry/posting date vs value date vs borrower receive date), model all three explicitly rather than overloading ValueDate. |
| General Instruction Remarks (Accounts-side posting remarks) | 🔴 Missing | domain, application, api, angular | Add an AccountsRemarks / PostingRemarks field captured at Post time. Current app has BuRemarks and CadRemarks but no Accounts-stage remarks field. |
| Manual double-entry voucher (Accounts-entered Debit/Credit GL lines with Total Debit / Total Credit / Difference) | 🟠 Partial | application, angular | Domain already supports arbitrary balanced GlLines and enforces debit==credit, but the Angular UI hardcodes a fixed 2-line Dr Loan/Cr Bank posting from effectiveLoanAmount and never lets Accounts enter lines. Replace submitPost()'s hardcoded lines with the prototype's editable Debit/Credit voucher (Account/GL select + Amount rows, add/remove, live Total Debit/Total Credit/Difference) so the posted GL reflects Accounts input including grant legs and multiple accounts. |
| GL account picklist (Debit: 102030 Loan Disbursement / 102040 Grant Disbursement; Credit: 202030 Bank / 202040 Cash) | 🟠 Partial | angular | Prototype offers a chart-of-accounts dropdown for both debit and credit legs (incl. Grant Disbursement and Cash Account options the app never emits). Wire the voucher GL selects to the GL/chart-of-accounts master so Accounts can pick real accounts rather than the two hardcoded codes. |
| CAD Disbursement Suggestions selection + integrity link into final disbursement | 🔴 Missing | domain, application, api, angular | Prototype shows CAD-proposed tranche lines (No, Sub No, Type, Source of Fund, Credit Line, Amount, Remarks) that Accounts selects (checkbox, INTEGRITY CHECK) to seed the final disbursement. Current CAD stage only stores two scalar JustifiedLoan/JustifiedGrant amounts — there are no CAD suggestion line items to select. Model CAD-proposed lines (extend Stage-2 to capture proposed lines) and persist which lines the Accounts final disbursement derives from, plus the integrity check that final lines reconcile to selected CAD suggestions. |
| Disbursement Summary aggregates: Already Disbursed Loan, Available Loan Amount, Already Disbursed Grant, Available Grant Amount | 🔴 Missing | application, api, angular | Add a query that returns per-sanction cumulative disbursed loan/grant and available (sanctioned minus disbursed) loan/grant. Needed to validate that the new disbursement does not exceed availability. Not currently computed or exposed. |
| Previous Disbursements history table (No, Sub No, Type, Source of Fund, Credit Line, Source of Disbursement, Amount, Remarks) | 🟠 Partial | application, api, angular | Data mostly exists across prior DisbursementRequests but the app has no per-sanction history view with these columns, and 'Source of Disbursement' and per-line Source of Fund/Credit Line are not modelled. Add a ListBySanction query returning prior processed disbursement lines with these columns. |
| Read-only verification context: Account info (A/C No, Open/Expiry/System Date), Customer info (ID, Name, Mobile, Email, Sector Code, Address), full Borrower Agreement Details, Customer Account Details (Loan Type), Agreement & Account documents | 🔴 Missing | application, api, angular | The Accounts posting screen displays a large read-only CAD-provided verification packet (account, customer, agreement terms incl. interest/repayment params, loan type, and two document tables). None is surfaced on the current post panel. Provide a GetDisbursementContext query aggregating account + customer + sanction/agreement + linked documents for display; most source data lives in Customer/Sanction/LoanAgreement modules and needs joining, not re-storing. |
| Loan Type (New \| Additional/Top-up \| Restructured) | 🔴 Missing | application, api, angular | Shown in Customer Account Details. If not already on the Sanction/Account aggregate, surface it in the disbursement context DTO for Accounts verification. |
| List page fields: Suggested Date, Borrower Name, A/C No, Project, Suggested Amount, Status (Pending\|Processed), Dept (CAD\|Accounts) | 🟠 Partial | application, api, angular | Current list shows referenceNo/sanctionRef/project/effectiveLoan/mode/status. Add/verify: human Borrower Name (not just customerNo), account number, suggested date/amount, and a 'Dept'/queue indicator. Note prototype status labels (Pending/Processed) differ from domain (Suggested/Proposed/Processed) — reconcile the Accounts-queue view to show Pending for Proposed items awaiting Accounts. |
| 'See Voucher' preview action | 🔴 Missing | angular | Add a voucher preview (the assembled Dr/Cr journal) before final Submit, matching the prototype's See Voucher button. Can be a client-side render of the entered voucher lines; no domain change required. |
| DisbursementMode enum value naming (prototype 'Pay Order' vs domain 'PayOrder') | 🟡 Naming | angular | Minor: prototype tab label is 'Pay Order' while domain/Angular use 'PayOrder'. Keep the stored code as PayOrder but display 'Pay Order'; ensure the mode selected on the tab maps to the stored enum. |

