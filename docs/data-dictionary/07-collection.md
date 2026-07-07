# Data Dictionary — Collection

> **Department:** CAD / Accounts  ·  **App route:** `/collections`  ·  **Backend module:** Collection

## Purpose

Two prototype pages under the CAD area. CollectionEntry.tsx is a single-screen receipt entry form with three stacked sections: (1) General Information — Project, Borrower, Currency, Payment Identifier (Unique), Instrument Amount, Instrument No, Payment Value Date; (2) Payment Information — Pay In Mode, Bank, Branch, A/C No, Title, plus mode-conditional instrument fields (Cheque No when mode=Cheque; Corresponding A/C No when SWIFT/RTGS; Account No when EFT/RTGS; Pay Order No when Pay Order), Payment Receive Date, LPC Date; (3) Payment Amount Entry — Principal, Interest, LPC, and a computed read-only Total Amount (auto-sum of Principal+Interest+LPC). Actions: Clear and Save Collection (navigates to list). CollectionList.tsx is a read-only table with search/filter/download-icon buttons and a per-row view (eye) action; columns ID, Project, Borrower, Identifier, Mode, Amount, Value Date, Status (badge). Status values shown: PENDING, VERIFIED. The prototype has NO explicit multi-stage workflow buttons beyond Save; verification/GL posting is not shown in the prototype (it only records the receipt and its P/I/LPC split, with status column implying a downstream Pending->Verified transition).

## Fields (22)

| Field | Type | Options / Values | Section | Source | Notes |
|---|---|---|---|---|---|
| `project` | select | Solar Park Phase II, RMG Efficiency Upgrade, Infrastructure Dev A | General Information | user input | Maps to loan/sanction ProjectName in the app (which is derived from the selected sanction, not chosen independently). |
| `borrower` | select | ABC Corporation Ltd., John Doe, Green Energy Solutions | General Information | user input | Maps to CustomerNo. In the app this is derived from the selected sanction, not a standalone dropdown. |
| `currency` | select | BDT, USD, EUR | General Information | user input | App has Currency (derived from sanction). Prototype allows EUR in addition to BDT/USD. |
| `paymentIdentifier` | text |  | General Information | user input | 'Payment Identifier (Unique)' - a user-entered unique external payment reference. NOT the same as the app's system-generated ReferenceNo (CO-yyyyMMdd-xxxx). No equivalent in current app. |
| `instrumentAmount` | number |  | General Information | user input | Present in app as InstrumentAmount. |
| `instrumentNo` | text |  | General Information | user input | Present in app as InstrumentNo (nullable). |
| `paymentValueDate` | date |  | General Information | user input | Present in app as ValueDate. Defaults to today. |
| `payInMode` | select | Cheque, SWIFT, EFT, RTGS, Pay Order, Cash | Payment Information | user input | App PaymentMode allowed set is Cash\|Cheque\|PayOrder\|EFT\|RTGS\|SWIFT\|PDC. Prototype uses label 'Pay Order' (app uses 'PayOrder' - naming/format divergence). Prototype has NO 'PDC'; app has 'PDC' but no distinct handling. Drives conditional instrument fields below. |
| `bank` | select | Sonali Bank, Janata Bank, City Bank, Standard Chartered | Payment Information | user input | App stores BankName as free-text (nullable) with no lookup; prototype models it as a selectable master list. |
| `branch` | select | Main Branch, Gulshan Branch, Motijheel Branch | Payment Information | user input | No equivalent in current app at any layer. |
| `acNo` | text |  | Payment Information | user input | 'A/C No' - the payer/collection bank account number. No equivalent in current app. |
| `title` | text |  | Payment Information | user input | Account title / name on the instrument. No equivalent in current app. |
| `chequeNo` | text |  | Payment Information | user input | Conditional: shown only when Pay In Mode = Cheque. App collapses all instrument identifiers into the single generic InstrumentNo field. |
| `correspondingAcNo` | text |  | Payment Information | user input | Conditional: shown when Pay In Mode = SWIFT or RTGS. No dedicated equivalent in current app. |
| `accountNo` | text |  | Payment Information | user input | Conditional: shown when Pay In Mode = EFT or RTGS. No dedicated equivalent in current app. |
| `payOrderNo` | text |  | Payment Information | user input | Conditional: shown when Pay In Mode = Pay Order. No dedicated equivalent in current app. |
| `paymentReceiveDate` | date |  | Payment Information | user input | Present in app as ReceiveDate. Defaults to today. |
| `lpcDate` | date |  | Payment Information | user input | Present in app as LpcDate (nullable). |
| `principal` | number |  | Payment Amount Entry | user input | Present in app as PrincipalAmount. |
| `interest` | number |  | Payment Amount Entry | user input | Present in app as InterestAmount. |
| `lpc` | number |  | Payment Amount Entry | user input | Present in app as LpcAmount. |
| `totalAmount` | computed |  | Payment Amount Entry | computed | Read-only auto-sum = Principal + Interest + LPC. App enforces the same as a validation invariant (P+I+LPC must equal InstrumentAmount) but does not persist a separate Total; equivalent behaviour exists. Note prototype does NOT enforce Total == Instrument Amount, it just displays the sum. |

## Repeating / line-item tables

- CollectionList table columns: ID, Project, Borrower, Identifier, Mode, Amount, Value Date, Status, Action
- GL journal lines: NOT present in the prototype (no GL entry table). The current app additionally implements a ReceiptGlLine table (GL Code, Description, Debit, Credit) generated on verification - this is app-side, beyond prototype scope.
- Payment amount allocation buckets (Principal / Interest / LPC + computed Total) - three parallel numeric inputs, not a repeating table.

## Current application coverage

**Backend:** Receipt aggregate at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/Collection/IDCOL.CBS.Collection.Domain/Receipt.cs (with ReceiptGlLine.cs, PaymentAllocation.cs). Application: EnterReceipt.cs, VerifyReceipt.cs, ListReceipts.cs. Controller: host/IDCOL.CBS.Api/Controllers/CollectionsController.cs. EF config: src/Modules/LoanLifecycle/IDCOL.CBS.LoanLifecycle.Infrastructure/Persistence/Configurations/ReceiptConfiguration.cs (tables COLL_RECEIPT, COLL_RECEIPT_GL_LINE).

Existing persisted fields: `ReferenceNo`, `SanctionId`, `SanctionRef`, `CustomerNo`, `ProjectName`, `Currency`, `PaymentMode`, `InstrumentNo`, `BankName`, `InstrumentAmount`, `ValueDate`, `ReceiveDate`, `LpcDate`, `PrincipalAmount`, `InterestAmount`, `LpcAmount`, `Status`, `EnteredBy/EnteredAtUtc`, `VerifiedBy/VerifiedAtUtc/VerifyComment`, `GlLines[]`, `Audit: CreatedBy/CreatedAtUtc/LastModifiedBy/LastModifiedAtUtc`, `Domain invariant: Principal+Interest+LPC must equal InstrumentAmount`, `PaymentWaterfall.Allocate helper`

**Frontend:** E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/core/collections/collection.models.ts (Receipt, EnterReceiptRequest, ReceiptGlLine) + collection.service.ts; feature at features/collections/collections.component.ts + .html.

## Gaps vs. prototype (12)

| Field / concept | Severity | Layers | Recommendation |
|---|---|---|---|
| paymentIdentifier (Payment Identifier - Unique) | 🔴 Missing | domain, application, api, angular | Add a user-entered unique external PaymentIdentifier field distinct from the system ReferenceNo. Add Receipt.PaymentIdentifier (string, nullable or required per client), include in Enter() factory + EnterReceiptCommand + validator, add COLL_RECEIPT column with a unique index, surface it in ReceiptDto, the Angular EnterReceiptRequest/Receipt models, the entry form, and the CollectionList 'Identifier' column (the list currently has no Identifier field at all). |
| branch | 🔴 Missing | domain, application, api, angular | Add a Branch field (of the collection/payer bank). Prototype models Bank+Branch as a hierarchy; add Receipt.BranchName (and ideally a BranchCode/lookup id) through domain factory, command, EF config column, DTO, and Angular model + form select. Consider a proper Bank/Branch master lookup rather than free text. |
| acNo (A/C No) | 🔴 Missing | domain, application, api, angular | Add the payer account number field (Receipt.AccountNo / payer A/C No) end-to-end. Currently no place to store the account the payment came from. |
| title (Account Title) | 🔴 Missing | domain, application, api, angular | Add the instrument/account title (name on the account/instrument) as Receipt.AccountTitle end-to-end. No equivalent exists. |
| chequeNo / correspondingAcNo / accountNo / payOrderNo (mode-specific instrument identifiers) | 🟠 Partial | domain, application, api, angular | The prototype captures a DIFFERENT instrument identifier per Pay In Mode (Cheque No for Cheque; Corresponding A/C No for SWIFT/RTGS; Account No for EFT/RTGS; Pay Order No for Pay Order), whereas the app collapses everything into one generic InstrumentNo. Either (a) add these as discrete nullable fields with mode-conditional capture, or (b) keep InstrumentNo but add an InstrumentType/label and add the missing CorrespondingAcNo/AccountNo/PayOrderNo semantics. Recommend discrete fields (ChequeNo, CorrespondingAccountNo, TransferAccountNo, PayOrderNo) with the same conditional-visibility rules in the Angular form; validation should require the relevant one based on PaymentMode. |
| bank (as master lookup) | 🟠 Partial | domain, application, angular | App stores BankName as free-text (nullable). Prototype presents Bank as a controlled selection (Sonali/Janata/City/Standard Chartered). Introduce a bank master lookup (BankCode + BankName) and make the Angular field a select. Low priority relative to the missing fields but needed for data quality. |
| payInMode option set (Pay Order label + Cash + missing/extra values) | 🟡 Naming | application, angular | Prototype label is 'Pay Order' (with space); app enum uses 'PayOrder'. Align the display/stored value. Also reconcile the option set: prototype has {Cheque, SWIFT, EFT, RTGS, Pay Order, Cash} and NO 'PDC'; app has 'PDC' additionally. Confirm with CAD head whether PDC belongs in Collection and whether the canonical labels should be spaced ('Pay Order'). Centralise the mode list (currently hardcoded in both Angular component and referenced as a comment in the domain). |
| currency EUR option | 🟠 Partial | angular | Prototype currency dropdown offers BDT/USD/EUR; app currency is derived from the sanction and defaults BDT. Minor: ensure EUR is a supported currency value if collections can differ from sanction currency; otherwise no action needed since currency follows the sanction. |
| project / borrower as independent selectors | 🟠 Semantics | angular | In the prototype Project and Borrower are independent dropdowns; in the app they are read-only values derived from the chosen Sanction (onSanctionChange sets projectName/customerNo). This is arguably a better model, but confirm with the client that a collection is always tied to a selected sanction/loan. No backend change needed; note the UX divergence and keep the sanction-driven derivation. |
| totalAmount (computed, display-only, no equality enforcement) | 🟠 Semantics | angular | Prototype shows Total = Principal+Interest+LPC purely as a computed display and does NOT force it to equal Instrument Amount. The app enforces P+I+LPC == InstrumentAmount as a hard invariant (domain + command validator + Angular allocationBalanced gate). Confirm the business rule: if partial allocation (Total != Instrument Amount, e.g. unallocated/suspense amount) must be allowed, the app's strict equality is too rigid and would need relaxing plus a suspense/unallocated bucket. If strict equality is correct, keep as-is (app is more complete than prototype here). |
| CollectionList search / filter / export | 🔴 Missing | angular, api | Prototype list has a search box, a filter button, and a download/export button. Angular list currently renders all receipts with no search/filter/export. Add client-side (or server-side) search and filter, and an export action; if server-side, extend ListReceiptsQuery with filter/paging params (prototype also implies pagination via Previous/Next controls). |
| CollectionList columns (Project, Borrower, Identifier, Value Date, ID) | 🟠 Partial | angular | Prototype list columns are ID, Project, Borrower, Identifier, Mode, Amount, Value Date, Status. Angular list shows Reference, Sanction, Mode, Amount, Principal, Interest, LPC, Status. Add Project (projectName), Borrower (customerNo), Identifier (new paymentIdentifier), and Value Date columns to match; ReceiptDto already carries projectName/customerNo/valueDate so the API side is covered for those three. |

