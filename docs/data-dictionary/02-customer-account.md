# Data Dictionary — Customer Account

> **Department:** BU/CRM  ·  **App route:** `/customers`  ·  **Backend module:** PartyKyc / AccountOpening

## Purpose

The prototype "Open/View Customer Account" screen (CustomerAccountEntry.tsx + CustomerAccountList.tsx) is a CAD/BU account-opening form that materializes a loan account from an approved Sanction. Sections: (1) General Information — read-only System Date and auto-generated A/C No, plus selectors for Customer ID and Sanction ID (Sanction options filtered by selected customer), and editable A/C Open Date and A/C Expiry Date. (2) Customer Information — a collapsible READ-ONLY panel echoing the selected customer (name, mobile, email, sector code, address) sourced from the customer master. (3) Borrower Agreement Details — a collapsible READ-ONLY panel echoing the linked Sanction across Core Information, Repayment & Interest Information, and Other Details (initiated/approved by BU and CRM, credit rating, remarks); explicitly labeled a full read-only view of the Borrower Agreement. (4) Details — the account's OWN editable copies: Proposed Loan Currency, Sanction Loan Amount, Loan Type, Proposed Grant Currency, Sanction Grant Amount; when currency is "Equivalent BDT" the form splits into Foreign Currency + Sanction Amount (Foreign) + fixed Local Currency BDT + Sanction Amount (BDT Eq.) for both loan and grant. (5) Bangladesh Banks Code — regulatory classification dropdowns (Economy Code NBFI-3 & CIB, Security Code NBFI-3 & CIB, Sector Type, Sector Code CIB, NBDC Code, SME Status, Refinance with conditional Refinance Ref, Agro, Agri, Women Entrepreneur). (6) Customer Account Related Documents — a repeating add/remove table of {Document Name, File}. Workflow is minimal: Clear and Save Account (navigates back to list); no maker-checker/Suggest-Propose-Process stages. The list page shows A/C No, Customer, Open Date, Expiry Date, Sanction ID, Loan Amount, and a Status (mock value "Active") with search and a View action.

## Fields (33)

| Field | Type | Options / Values | Section | Source | Notes |
|---|---|---|---|---|---|
| `systemDate` | date (read-only, computed = today) |  | General Information | computed | new Date().toISOString().split('T')[0]; display only |
| `accountNo` | text (read-only, auto-generated) |  | General Information | computed | 'ACC-' + random 12-digit; the account's business key |
| `customerId` | select | CUST-100234 - ABC Corporation Ltd., CUST-789012 - John Doe | General Information | user input | FK to Customer; drives the Customer Information panel and filters Sanction options |
| `sanctionId` | select | SA-2024-001 - Term Loan for Infrastructure, SA-2024-002 - Working Capital Support | General Information | user input | FK to Sanction; options filtered to selected customer; drives Borrower Agreement Details panel |
| `openDate` | date |  | General Information | user input | A/C Open Date, user-entered |
| `expiryDate` | date |  | General Information | user input | A/C Expiry Date, user-entered (distinct from Sanction expiry) |
| `loanCurrency` | select | USD, EUR, BDT, Equivalent BDT | Details | user input | Proposed Loan Currency; 'Equivalent BDT' triggers dual foreign/local capture |
| `loanCurrencyForeign` | select | USD, EUR | Details | user input | Foreign Currency (Loan); only shown when loanCurrency='Equivalent BDT' |
| `loanAmount` | number |  | Details | user input | Sanction Loan Amount; when Equivalent BDT this is the Foreign amount |
| `loanAmountEquivalent` | number |  | Details | user input | Sanction Loan Amount (BDT Eq.); only for Equivalent BDT, paired with fixed local currency 'BDT' |
| `loanType` | select | Conversion, New, Takeover | Details | user input | default 'New' |
| `grantCurrency` | select | USD, EUR, BDT, Equivalent BDT | Details | user input | Proposed Grant Currency |
| `grantCurrencyForeign` | select | USD, EUR | Details | user input | Foreign Currency (Grant); only when grantCurrency='Equivalent BDT' |
| `grantAmount` | number |  | Details | user input | Sanction Grant Amount; when Equivalent BDT this is the Foreign amount |
| `grantAmountEquivalent` | number |  | Details | user input | Sanction Grant Amount (BDT Eq.); only for Equivalent BDT |
| `economyCodeNBFI` | select | NBFI-3-A, NBFI-3-B | Bangladesh Banks Code | user input | Economy Code (NBFI-3) |
| `economyCodeCIB` | select | CIB-E-10, CIB-E-20 | Bangladesh Banks Code | user input | Economy Code (CIB) |
| `securityCodeNBFI` | select | NBFI-3-S1, NBFI-3-S2 | Bangladesh Banks Code | user input | Security Code (NBFI-3) |
| `securityCodeCIB` | select | CIB-S-05, CIB-S-10 | Bangladesh Banks Code | user input | Security Code (CIB) |
| `sectorType` | select | Government, Public, Private | Bangladesh Banks Code | user input | Sector Type |
| `sectorCodeCIB` | select | CIB-SEC-01, CIB-SEC-02 | Bangladesh Banks Code | user input | Sector Code (CIB); distinct from Customer.SectorCode read-only panel value |
| `nbdcCode` | select | NBDC-001, NBDC-002 | Bangladesh Banks Code | user input | NBDC Code |
| `smeStatus` | select | SME, Non-SME | Bangladesh Banks Code | user input | SME Status |
| `refinance` | select (Yes/No) | Yes, No | Bangladesh Banks Code | user input | Refinance flag; 'Yes' reveals Refinance Ref |
| `refinanceRef` | text |  | Bangladesh Banks Code | user input | Refinance Ref; conditional, shown only when refinance='Yes' |
| `agro` | select (Yes/No) | Yes, No | Bangladesh Banks Code | user input | Agro flag |
| `agri` | select (Yes/No) | Yes, No | Bangladesh Banks Code | user input | Agri flag |
| `womenEntrepreneur` | select (Yes/No) | Yes, No | Bangladesh Banks Code | user input | Women Entrepreneur flag |
| `documents[].name` | text (table-line-item) |  | Customer Account Related Documents | user input | repeating add/remove table; document name |
| `documents[].file` | file (table-line-item) |  | Customer Account Related Documents | user input | file attachment per document row |
| `status` | computed (list only) | Active | Customer Account List | computed | account status shown in list; only 'Active' present in mock; implies at least one lifecycle state |
| `customerInfo (name, mobile, email, sectorCode, address)` | read-only panel (from Customer master) |  | Customer Information | computed | echoed from selected customer; address not currently on Customer entity/DTO |
| `borrowerAgreementDetails (all Sanction fields incl. businessUnit, product, project, industryType, loan/grant amounts, agreementDate, availability period date+months, interestRateType, initialInterestRate, loanTenor, noOfPrincipalRepayments, interestGracePeriod+unit, principalMoratoriumPeriod+unit, repaymentMethod, lpcRate, principalFrequency, interestFrequency, dayCountBasis, initiatedByBU, approvedByBU, initiatedByCRM, approvedByCRM, creditRating, remarks)` | read-only panel (from Sanction) |  | Borrower Agreement Details | computed | echoed from linked Sanction for verification; mostly covered by existing Sanction entity except grace/moratorium unit, availabilityPeriod months, initiated/approved-by fields, remarks |

## Repeating / line-item tables

- Customer Account Related Documents table — columns: Document Name (text input), File (file upload), Action (add/remove rows). Repeating: user can Add Document / remove rows (min 1).
- Details section Equivalent-BDT expansion — when Loan or Grant currency = 'Equivalent BDT', a paired sub-structure appears: Foreign Currency (USD/EUR) + Sanction Amount (Foreign) + Local Currency (fixed 'BDT') + Sanction Amount (BDT Eq.), for loan and grant independently.
- Customer Account List table — columns: A/C No, Customer (name + customerId), Open Date, Expiry Date, Sanction ID, Loan Amount, Status, Action (View).

## Current application coverage

**Backend:** There is NO CustomerAccount / account-opening entity in the backend. The nearest existing entities are: (1) Customer at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/PartyKyc/IDCOL.CBS.PartyKyc.Domain/Customer.cs (borrower master, backs the read-only Customer Information panel), and (2) LoanAgreement/Sanction at E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-api/src/Modules/CreditSanction/IDCOL.CBS.CreditSanction.Domain/LoanAgreement.cs (backs the read-only Borrower Agreement Details panel). CreateCustomerCommand/CustomerDto at .../PartyKyc.Application/Customers/{CreateCustomer.cs,ListCustomers.cs}; CustomersController at host/IDCOL.CBS.Api/Controllers/CustomersController.cs; EF map CustomerConfiguration.cs (table PARTYKYC_CUSTOMER) under LoanLifecycle.Infrastructure. No AccountConfiguration, no AccountsController, no CreateCustomerAccount command exist.

Existing persisted fields: `Customer.CustomerNo`, `Customer.CustomerType`, `Customer.Name`, `Customer.BusinessUnitCode`, `Customer.Mobile`, `Customer.Email`, `Customer.SectorCode`, `Customer.KycStatus`, `Customer.RiskLevel`, `Customer.Source`, `Customer.IsActive`, `Customer audit fields`, `Sanction.SanctionId`, `Sanction.CustomerId`, `Sanction.CustomerNo`, `Sanction.ProductCode`, `Sanction.ProjectName`, `Sanction.IndustryType`, `Sanction.LoanCurrency`, `Sanction.LoanAmount`, `Sanction.GrantCurrency`, `Sanction.GrantAmount`, `Sanction.AgreementDate`, `Sanction.ExpiryDate`, `Sanction.InterestRateType`, `Sanction.InitialInterestRatePercent`, `Sanction.LoanTenorMonths`, `Sanction.NoOfPrincipalRepayments`, `Sanction.InterestGracePeriodMonths`, `Sanction.PrincipalMoratoriumMonths`, `Sanction.RepaymentMethod`, `Sanction.PrincipalFrequency`, `Sanction.InterestFrequency`, `Sanction.DayCountBasis`, `Sanction.LpcRatePercent`, `Sanction.CreditRating`, `Sanction.Status`

**Frontend:** No customer-account model or component. Nearest: Customer + CreateCustomerRequest and Sanction + CreateSanctionRequest interfaces in E:/Projects/IDCOL/2026/Repayment Doc/CBS-IDCOL/cbs-web/src/app/core/lifecycle/lifecycle.models.ts; features/customers/customers.component.ts (customer master CRUD, not account opening); features/sanctions/sanctions.component.ts. No customer-account entry/list component, no route /cad/customer-account*, no account service method in lifecycle.service.ts.

## Gaps vs. prototype (30)

| Field / concept | Severity | Layers | Recommendation |
|---|---|---|---|
| CustomerAccount aggregate (accountNo, customerId, sanctionId, openDate, expiryDate + all fields below) | 🔴 Missing | domain, application, api, angular | Create a net-new CustomerAccount aggregate root (own module or under LoanLifecycle). It is the parent for every field on this screen. Include AccountNo (unique business key, system-generated 'ACC-' + 12 digits), CustomerId (FK to PARTYKYC_CUSTOMER), SanctionId (FK to LoanAgreement), OpenDate, ExpiryDate, Status, audit fields. Add EF AccountConfiguration (e.g. table LOAN_CUSTOMER_ACCOUNT) with unique index on AccountNo, an AccountsController (List, GetById, Create), a CreateCustomerAccountCommand + AccountDto, an ICustomerAccountRepository, and Angular CustomerAccount/CreateCustomerAccountRequest models + entry/list components + lifecycle.service methods + routes /cad/customer-accounts and /cad/customer-account-entry(/:id). |
| accountNo | 🔴 Missing | domain, application, api | System-generated account number is the account's business key; implement a server-side generator (not the prototype's client random) and persist as unique. Do not trust a client-supplied value. |
| openDate | 🔴 Missing | domain, application, api, angular | Add A/C Open Date (DateOnly) to the account entity/command/DTO and Angular model. Distinct from Sanction.AgreementDate. |
| expiryDate | 🔴 Missing | domain, application, api, angular | Add account A/C Expiry Date (DateOnly). Semantically distinct from Sanction.ExpiryDate (availability-period expiry), so do NOT reuse the sanction column. |
| loanType | 🔴 Missing | domain, application, api, angular | Add LoanType string with allowed values Conversion\|New\|Takeover (default New) plus FluentValidation Must(...) constraint. No such concept exists anywhere today. |
| loanCurrency (proposed, incl. Equivalent BDT) | 🟠 Partial | domain, application, api, angular | Add account-level ProposedLoanCurrency. Existing Sanction.LoanCurrency does not enforce/support the 'Equivalent BDT' dual-capture mode. Model currency plus a boolean/enum for equivalent mode; allowed values USD\|EUR\|BDT\|Equivalent BDT. |
| loanCurrencyForeign | 🔴 Missing | domain, application, api, angular | Add ForeignLoanCurrency (USD\|EUR), populated only when loanCurrency='Equivalent BDT'. No backing today. |
| loanAmount (proposed) + loanAmountEquivalent (BDT eq.) | 🔴 Missing | domain, application, api, angular | Add account-level ProposedLoanAmount and ProposedLoanAmountBdtEquivalent (foreign amount + BDT-equivalent amount for the Equivalent-BDT case). Sanction.LoanAmount is a single value with no equivalent field; the account keeps its own proposed copy. |
| grantCurrency (proposed, incl. Equivalent BDT) | 🟠 Partial | domain, application, api, angular | Add account-level ProposedGrantCurrency with USD\|EUR\|BDT\|Equivalent BDT and the same equivalent-mode handling as loan. Sanction.GrantCurrency lacks equivalent-BDT support. |
| grantCurrencyForeign | 🔴 Missing | domain, application, api, angular | Add ForeignGrantCurrency (USD\|EUR), only for Equivalent-BDT mode. |
| grantAmount (proposed) + grantAmountEquivalent (BDT eq.) | 🔴 Missing | domain, application, api, angular | Add account-level ProposedGrantAmount and ProposedGrantAmountBdtEquivalent. No equivalent field exists on Sanction. |
| economyCodeNBFI (NBFI-3) | 🔴 Missing | domain, application, api, angular | Add EconomyCodeNbfi regulatory classification field. No Bangladesh Bank code exists anywhere; consider a lookup/reference table rather than hardcoded options. |
| economyCodeCIB | 🔴 Missing | domain, application, api, angular | Add EconomyCodeCib. Model as FK to a CIB economy-code lookup. |
| securityCodeNBFI (NBFI-3) | 🔴 Missing | domain, application, api, angular | Add SecurityCodeNbfi. Lookup-backed. |
| securityCodeCIB | 🔴 Missing | domain, application, api, angular | Add SecurityCodeCib. Lookup-backed. |
| sectorType | 🔴 Missing | domain, application, api, angular | Add SectorType with allowed values Government\|Public\|Private and a validation constraint. |
| sectorCodeCIB | 🔴 Missing | domain, application, api, angular | Add SectorCodeCib on the account. Note: this is a regulatory CIB sector code distinct from Customer.SectorCode (shown read-only in the Customer panel) — do NOT conflate the two (divergent-semantics with the existing SectorCode). |
| nbdcCode | 🔴 Missing | domain, application, api, angular | Add NbdcCode. Lookup-backed. |
| smeStatus | 🔴 Missing | domain, application, api, angular | Add SmeStatus (SME\|Non-SME). Could be bool but prototype uses an explicit 2-value enum; keep as constrained string to match reporting semantics. |
| refinance + refinanceRef | 🔴 Missing | domain, application, api, angular | Add Refinance (Yes/No -> bool) and conditional RefinanceRef (string, required when Refinance=Yes). Implement conditional validation. |
| agro | 🔴 Missing | domain, application, api, angular | Add Agro flag (Yes/No -> bool). |
| agri | 🔴 Missing | domain, application, api, angular | Add Agri flag (Yes/No -> bool). Keep distinct from Agro. |
| womenEntrepreneur | 🔴 Missing | domain, application, api, angular | Add WomenEntrepreneur flag (Yes/No -> bool). |
| documents[] (name + file upload table) | 🔴 Missing | domain, application, api, angular | Add a child collection AccountDocument { Name, StoredFileRef/Path/BlobId } with a child EF table and repository handling, plus multipart file upload endpoints and an Angular add/remove documents table. No document-upload capability exists in any current module. |
| status (account lifecycle) | 🔴 Missing | domain, application, api, angular | Add Status to the account entity (at minimum Active seen in the list). Confirm the full state set with the client; even if opening is single-step Save, the list requires a persisted status. |
| customer address (Customer Information panel) | 🔴 Missing | domain, application, api, angular | The read-only Customer panel shows Address, but the Customer entity/CustomerDto/Angular Customer interface have no Address field. Add Address to Customer (domain + CustomerConfiguration + CustomerDto + Create + Angular Customer model) so the panel can render it. |
| Sanction panel: initiatedByBU / approvedByBU / initiatedByCRM / approvedByCRM | 🔴 Missing | domain, application, api, angular | The Borrower Agreement Details panel displays BU/CRM initiator+approver names, but LoanAgreement has none of these. Add them to the Sanction entity/DTO (or the maker-checker audit) if this read-only display is required at account opening. |
| Sanction panel: remarks | 🔴 Missing | domain, application, api, angular | Panel shows Sanction Remarks; LoanAgreement has no Remarks field. Add Remarks to the Sanction entity/DTO to support the read-only display. |
| Sanction panel: availabilityPeriod (months) and interestGracePeriodUnit / principalMoratoriumPeriodUnit | 🟠 Partial | domain, application, api, angular | Panel shows Availability Period in months (Sanction has ExpiryDate as the date, but no numeric months field) and shows grace/moratorium with a Month/other unit. Existing Sanction stores InterestGracePeriodMonths and PrincipalMoratoriumMonths as fixed-month integers with no unit. If the client needs a selectable unit and an explicit availability-months number, extend the Sanction model; otherwise derive months and hardcode the unit label in the read-only view. |
| Customer/Sanction GET-by-id for panels | 🟠 Partial | application, api, angular | CustomersController and Sanctions only expose List today; the account screen needs single Customer and single Sanction fetches (and Sanctions filtered by customerId). Add GetById endpoints and a by-customer sanction filter so the two read-only panels can be populated when a customer/sanction is selected. |

