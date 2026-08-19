---
doc_id: KB-030
title: Business Rule Inventory (As-Is, with source evidence)
module: all
source_files:
  - V.SMART/V.SMART.Shared/Services/CalculationService.cs
  - V.SMART/V.SMART.Shared/Utility_Constants/CommonConstants.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/StockManagerService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/IBusinessService/IPlanningService/IApprovalService.cs
  - V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs
  - V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesPo_Pages/MfgPOUpsert.razor
entities: [MfgPo, MfgPoSub, StockAdd, StockIssue, StockIssueTrack, User, ApprovalHistory, TenantInfo]
api_endpoints: []
database_tables: [MfgPo, MfgPoSub, StockAdd, StockIssue, StockIssueTrack, Users, ApprovalHistory]
business_rules: [BR-CALC-001, BR-CALC-002, BR-STK-001, BR-STK-002, BR-SO-001, BR-SO-002, BR-SO-003, BR-AUTH-001, BR-AUTH-002, BR-APPR-001, BR-RPT-001, BR-TEN-001]
status: partial
confidence: mixed
last_verified: 2026-08-19
dependencies: [KB-011, KB-012, KB-013]
---

# Business Rule Inventory (As-Is)

> **Status: partial.** This document holds the rules verified in the Phase-1 sweep. It is
> the template and the seed, not the complete catalogue. Per-module rule extraction is
> Phase-1b work — see [`investigation-registry.md`](../investigation-registry.md).
>
> Rule IDs are stable. Add rules; never renumber.

## Format

Every rule records: **Statement · Evidence (file:line) · Confidence · Preserve/Replace ·
Migration note.**

---

## Calculation rules

### BR-CALC-001 — One engine computes all document totals and taxes

**Statement.** Every transactional document's gross, discount, freight/packing/insurance,
taxable value, CGST/SGST/IGST, TCS, round-off and grand total are computed by a single
implementation, `CalculationService.UpdateTotalsAsync(ICalculationDocument)`. Documents
opt in by implementing `ICalculationDocument` / `ICalculationDocumentSubItem`.

Ordered algorithm:
1. `TotalGrossAmount` = Σ `LineGross`
2. Header discount = Σ line discounts **if any line has one**; else fixed amount when
   `DiscAmtOrPer` is true, else `Gross × DiscountPercent / 100`
3. `TotalBasicAmount` = Gross − Discount
4. Packing / Insurance computed as % of `TotalBasicAmount` when `…AmtOrPer` is false
5. `TotalTaxable` = Basic + Freight + Packing + Insurance
6. Tax — **item-wise** (`HasItemWiseTax`): per line, taxable base =
   `LineGross − LineDiscountAmount − proportionalHeaderDiscount + proportional(Freight+Packing+Insurance)`,
   where `proportion = LineGross / Σ LineGross`; else **header-wise**: `TotalTaxable × rate / 100`
7. TCS = fixed amount or `(taxable + taxes) × TCSPercent / 100`
8. Grand total = taxable + taxes + TCS + `OtherCharges`; if `IsRoundOffEnabled`,
   `Math.Round(x, 0, MidpointRounding.AwayFromZero)` with the delta stored in `RoundOff`

**Precision and rounding (Confirmed, M0-12-02, 2026-08-19).** **No intermediate rounding
occurs anywhere in the method.** Every intermediate — discount, basic, packing, insurance,
taxable, each per-line taxable base, each tax accumulation, TCS and the pre-round grand
total — is full `decimal` precision. There is exactly **one** `Math.Round` call in the whole
method, at `CalculationService.cs:103`, and it executes only when `IsRoundOffEnabled`
(`:101`). It rounds to **0 decimal places** with `MidpointRounding.AwayFromZero`, not
banker's rounding. `RoundOff` is **signed** — `rounded − preRound` at `:104` — so it is
negative whenever the total rounds down. When round-off is disabled, `RoundOff` is set to
`0` and `GrandTotal` is the unrounded value (`:109-110`).

This is the contract the React client (M2-C10) and the future
`POST /api/documents/calculate` endpoint must honour: an implementation that rounded to
currency precision at any intermediate step would produce different money. Pinned by
`S18_NoIntermediateRounding_FullDecimalPrecisionSurvivesToTheGrandTotal`, which shows five
decimal places surviving to `GrandTotal`.

*Not established:* whether the decimal **scale** of the returned values is part of that
contract. Exact-value assertions cannot see scale (`18.000m` equals `18m` under
`decimal.Equals`) — recorded as an open question in [KB-004](../open-questions.md).

**Evidence.** `V.SMART.Shared/Services/CalculationService.cs:12-114` (`UpdateTotalsAsync`
begins at `:12`; the file is 117 lines — a previously cited `:12-118` was out of range;
re-verified again 2026-08-19 by M0-12-02, unchanged);
`Utility_Constants/ICalculationDocument.cs:11-40`;
`Utility_Constants/ICalculationDocumentSubItem.cs:12-25`.

**Confidence.** Confirmed.
**Disposition.** **Preserve verbatim — do not port to TypeScript.**
**Migration note.** Expose as a `POST /api/documents/calculate` preview endpoint. The
React client may render an optimistic local estimate for responsiveness, but the server
result is authoritative and must overwrite it before save.

**Pinned by executable tests (M0-12-02, 2026-08-19).** Every step of the algorithm above is
now asserted by
`tests/V.SMART.Shared.Tests/Services/CalculationServiceCharacterisationTests.cs`
(30 tests, all green; the file header carries the full statement-to-test map). Both tax
branches are covered — the item-wise branch with three lines at three different rate shapes
— as are the boundaries the method handles silently:

| Test | What it pins |
|---|---|
| `S01_NullDocument_…` / `S01_NullSubItemCollection_…` / `S01_EmptySubItemCollection_SilentlyReturnsWithoutComputing_AndMutatesNothing` | the three silent returns at `:14-15`; twelve pre-set output fields are asserted **unchanged**, so a future "zero it out" early return fails |
| `S05_WhenDiscountExceedsGross_TotalBasicAmountGoesNegative_WithNoFloorAtZero` | no floor at zero at `:35` — a discount above gross gives a negative basic amount, taxable and grand total |
| `S10_ItemWiseBranch_WhenEveryLineGrossIsZero_ProportionGuardYieldsZeroTax` | the divide-by-zero guard at `:61`, and its consequence: freight is in `TotalTaxable` but attracts no tax |
| `S11_ItemWiseBranch_WhenHeaderDiscountIsAFixedAmount_ItDoesNotReduceTheTaxableBase` | ⚠️ `:63-65` — a **fixed** header discount does not reduce the item-wise taxable base; 1000 gross less 100 fixed discount is taxed on **1000**, not 900 |
| `S11_ItemWiseBranch_WhenHeaderDiscountIsAPercentage_ItIsSpreadAcrossLinesByProportion` | the contrast: the same 100 of discount, spread by proportion, taxes 900 |
| `S15_TcsPercent_IsAppliedToATaxInclusiveBase_AndOtherChargesAreExcluded` | TCS base at `:87-95` is tax-inclusive and excludes `OtherCharges` |
| `S17_WhenRoundOffEnabled_MidpointRoundsAwayFromZero_NotBankers` and `S17_…AndAnOddIntegerMidpoint_…` | `MidpointRounding.AwayFromZero` at `:103`, proved on both a `.5` that banker's would round down and one it would round up |
| `S17_WhenRoundingDown_RoundOffIsNegative` | `RoundOff` is signed |
| `S13_ItemWiseBranch_PerLineTaxAmountsAreNeverWritten_OnlyHeaderTotalsAccumulate` | the service never writes `LineCGSTAmount`/`LineSGSTAmount`/`LineIGSTAmount` |
| `S19_UpdateTotalsAsync_CompletesSynchronously_DespiteTheAsyncSignature` | the returned `Task` is already completed — four `DebitNoteUpsert.razor` call sites depend on it (see R-39) |

A red test there means **this rule changed**. If the change was intended, update the test in
the same commit as the production change and amend this entry.

### BR-CALC-002 — GST rates are restricted to a fixed set

**Statement.** IGST is one of `0, 0.1, 0.25, 1, 1.5, 3, 5, 6, 7.5, 12, 18, 28` %.
CGST/SGST use the half-rate list `0, 0.05, 0.125, 0.5, 0.75, 1.5, 2.5, 3, 3.75, 6, 9, 14` %.

**Evidence.** `Utility_Constants/CommonConstants.cs:11-16` (`IGSTRates`), `:18-23`
(`GSTRates`), `:25-26` (`GetIGST(decimal)`, `GetGST(decimal)`) — re-verified 2026-08-19
(M0-12-02).

**Confidence.** Confirmed.
**Disposition.** Preserve.
**Migration note.** Serve as a reference-data endpoint so the React select cannot drift
from the server list. Note `GetIGST`/`GetGST` return `0` (via `FirstOrDefault`) for an
unlisted rate rather than raising — silently coercing an invalid rate to zero tax. Worth
tightening; see risk R-15.

**Pinned by executable tests (M0-12-02, 2026-08-19).**
`tests/V.SMART.Shared.Tests/Services/CommonConstantsGstRateTests.cs` (7 tests, all green)
pins both lists **by content and by order**, the round-trip of a listed rate, and the R-15
defect itself: `GetIGST_WithUnlistedRate_SilentlyReturnsZero_R15` and
`GetGST_WithUnlistedRate_SilentlyReturnsZero_R15` assert the zero coercion **green**, as a
characterisation baseline. `GetIGST_CannotDistinguishNotFoundFromTheListedZeroRate_R15`
records why a caller cannot detect the defect: `GetIGST(17m)` and `GetIGST(0m)` return the
same value.

**Additional current behaviour observed while pinning (Confirmed, M0-12-02).**
`CalculationService` neither calls `GetIGST`/`GetGST` nor validates a rate — there is no
reference to `CommonConstants` anywhere in `CalculationService.cs:12-114`. It multiplies by
whatever rate the document carries, including one absent from these lists. Pinned by
`S21_R15_AnUnlistedGstRate_IsAppliedWithoutValidationOrCoercionToZero`, which shows a 17%
IGST rate producing 170 on a taxable value of 1000. So R-15 is **latent in the engine and
live only at whatever call site routes a rate through `GetIGST`/`GetGST`** — the two paths
disagree, one charging 170 and the other 0 for the same document.

These tests **must not be "fixed"**. If R-15 is repaired, update them in the **same commit**
as the production change.

---

## Inventory rules

### BR-STK-001 — Stock is consumed FIFO by receipt date, tracked line by line

**Statement.** Issuing stock allocates against `StockAdd` batches for the same
`(ItemId, StoreId, RcSubId)` with `BalQty > 0`, ordered by `AddDate` ascending. Each
allocation decrements `StockAdd.BalQty` and writes a `StockIssueTrack { IssueId, AddId, UsedQty }`
row. Re-issuing an existing `StockIssue` first **reverses** all prior tracks (adding
`UsedQty` back to each `StockAdd.BalQty`, deleting the track rows) before re-allocating.

`RcSubId` participates in the match key, so route-card-bound stock is consumed
operation-by-operation and does not mix with free stock.

**Evidence.** `BusinessLayer/BusinessService/InventoryService/StockManagerService.cs:177-249`
(`private async Task TrackStockUsageAsync`, declared at `:177`), called from
`IssueOrUpdateStockAsync` (declared at `:105`) at `:133` and `:157`.
*(Line numbers re-verified 2026-08-12; previously cited as `:175-243` / `:105-171`.)*

**Confidence.** Confirmed.
**Disposition.** **Preserve verbatim.** This is the most safety-critical algorithm in the
system.
**Migration note.** Never expose a stock mutation endpoint that bypasses
`IStockManagerService`. Add integration tests around it **before** any module that writes
stock is migrated (Phase 5 pulled forward — see migration strategy).

**Pinned by executable tests (M0-13, 2026-08-19).** Every statement above is now asserted by
`tests/V.SMART.Shared.Tests/Services/StockManagerServiceCharacterisationTests.cs`
(25 tests, all green). The FIFO ordering itself is pinned by
`S05_Issue_AcrossThreeBatches_ConsumesOldestAddDateFirst` (three batches at distinct
`AddDate` values, inserted newest-first so insertion order cannot explain the result),
`RcSubID` discrimination by `S06_Issue_WithNullRcSubId_DoesNotConsumeRouteCardBoundBatches`
and `S06_Issue_WithRouteCardRcSubId_DoesNotConsumeFreeStockBatches`, and track reversal on
re-issue by `S04_ReIssue_WithSmallerQuantity_ReversesPriorTracksBeforeReallocating`. The
file header of that suite carries the full statement-to-test map. A red test there means
this rule changed.

**Not pinned — Unknown (M0-13).** `.OrderBy(sa => sa.AddDate)` at `:206` declares no
secondary sort key, so FIFO order between two batches sharing an identical `AddDate` is
undefined. The test harness (EF Core InMemory, INV-031) evaluates `OrderBy` as
LINQ-to-objects, which is a *stable* sort; SQL Server's `ORDER BY` guarantees no such thing.
The suite therefore uses distinct `AddDate` values throughout and asserts nothing about
ties. Resolving this needs a run against a real SQL Server instance.

### BR-STK-002 — ⚠️ Over-issue is silently permitted when batch balance is insufficient

**Statement (defect, not intended behaviour).** `TrackStockUsageAsync` throws
`InvalidOperationException("No available stock to issue.")` **only when no batch exists at
all** with `BalQty > 0`. If batches exist but their total balance is less than the
requested quantity, the loop exhausts them, leaves `remainingQty > 0`, and **returns
without any error**. The `StockIssue` row records the full `IssueQty`, but
`StockIssueTrack` accounts for less. The ledger silently goes out of balance.

**Evidence.** `StockManagerService.cs:209-210` — the guard
`if (!stockAdds.Any()) throw new InvalidOperationException("No available stock to issue.")`,
fed by the candidate-batch query at `:203-207` — versus the allocation loop at `:212-231`
(`foreach (var add in stockAdds)`, breaking when `remainingQty <= 0`). **There is no
post-loop `remainingQty > 0` check between the loop's close at `:231` and
`await _unitOfWork.SaveAsync()` at `:233`** — re-verified 2026-08-12, and this absence is
the defect. *(Previously cited as `:203-206` / `:208-231`.)*

**Confidence.** Confirmed (read directly from the code path).
**Disposition.** **Bug — must be decided before migration, not during.**
**Migration note.** Do **not** silently "fix" this while building the API: some tenants may
depend on negative/over-issue being permitted for back-dated entry. Raise as a product
decision (Q-01). If confirmed a bug, fix once in `StockManagerService` so both the Blazor
UI and the API get the fix. Risk R-07.

**Pinned by executable tests (M0-13, 2026-08-19).** The defect is now asserted **green** —
deliberately, as a characterisation baseline — in
`tests/V.SMART.Shared.Tests/Services/StockManagerServiceCharacterisationTests.cs`:

| Test | What it pins |
|---|---|
| `S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue` | 100 requested against **zero** balance → `InvalidOperationException("No available stock to issue.")` |
| `S14_R07_IssueOrUpdateStock_WhenBatchesExistButTotalBalanceIsShort_SilentlyUnderAllocatesAndDoesNotThrow` | 100 requested against 30 available → **no exception**; `IssueQty` 100, `Σ UsedQty` 30, drift asserted as exactly `70m` |
| `S15_R07_IssueOrUpdateStock_WhenReIssueIncreasesQuantityBeyondAvailableStock_SilentlyUnderAllocatesOnTheUpdatePathToo` | the same drift occurs on the **update** path (re-issue 3 → 100 against a batch of 5; drift `95m`) |
| `S16_R07_IssuingOneHundred_ThrowsAgainstZeroStock_ButSilentlyDriftsByNinetyNineAgainstOneUnit` | the asymmetry of statement 16: one unit of stock is the whole difference between a hard refusal and a silent 99-unit hole |

These tests **must not be "fixed"**. If the product decision (M0-11 / Q-01) is to add the
missing `remainingQty > 0` check, update these tests in the **same commit** as the production
change.

**Additional current behaviour observed while pinning (Confirmed, M0-13).** When the throw at
`:209-210` fires on the *create* path, the `StockIssue` row has **already been created and
committed** by `_unitOfWork.SaveAsync()` at `:154-155`. A refused issue therefore leaves an
orphan `StockIssue` row carrying the full requested quantity with **no** `StockIssueTrack`
rows at all. Pinned by the last three assertions of
`S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue`. This is part
of what Q-01 has to decide about.

---

## Sales Order rules

### BR-SO-001 — A Sales Order cannot be deleted once any downstream document exists

**Statement.** `CanDeleteSalesOrderAsync(poId)` returns `false` with a user-facing message
if any of the following reference the order's `PoSubId` values:
Sales DC (`MfgDcSub.RefPoSubId`), Tax Invoice (`MfgInvSub.RefPoSubId`),
Export Invoice (`ExpInvSub.RefPoSubId`), Proforma Invoice (`PerformaInvSub.RefPoSubId`),
Route Card (`RouteCard.RefPoId`), Contract Review (`ContractReview.PoId`).

**Evidence.** `BusinessLayer/BusinessService/SalesService/MfgPoService.cs:465-565`.

**Confidence.** Confirmed.
**Disposition.** Preserve.
**Migration note.** The returned `Message` strings are product UX — surface them verbatim
in the React error toast. Do not replace with generic "Cannot delete".

### BR-SO-002 — ⚠️ Two guards in that chain are unreachable (copy-paste defects)

**Statement (defect).** In the same method:
- The **Export Invoice** guard computes `hasExpInvoice` but then tests
  `if (hasInvoice)` — so an order with only an export invoice and no domestic invoice
  **can be deleted**. `MfgPoService.cs:499-505`.
- The **Contract Review** guard computes `hasCR` but then tests `if (hasRc)` — so an order
  with a contract review and no route card **can be deleted**. `MfgPoService.cs:523-525`.

**Evidence.** `MfgPoService.cs:499-505` and `:523-525`.

**Confidence.** Confirmed.
**Disposition.** **Bug.** Fix in the service (one place, benefits both UIs).
**Migration note.** Fix before exposing the delete endpoint — an API makes the wrong
branch far easier to hit. Risk R-08. Audit the other ~40 `CanDelete…Async` methods for the
same copy-paste pattern (INV-011).

### BR-SO-003 — Order and line cancellation require a reason and check transactions first

**Statement.** Cancelling a whole Sales Order or an individual line requires:
1. a check that downstream transactions do not already consume the quantity
   (`IsPoTransactionsMatchedAsync`, `CanSalesOrderItemCancelCheckAsync`,
   `IsPoTransactionsMatchedRowRemoveAsync`),
2. a mandatory **cancellation reason** captured in the UI,
3. reverting balance quantities upstream
   (`UpdateItemCancelAndAddorRevertAsync`, `UpdatedCancelStatusAndAddOrRevertQty`,
   `ValidateBeforeRevertAsync`, `ValidateQuotationBalanceBeforeRevertAsync` —
   which pushes quantity back to the source quotation line).

A separate **short-close** path (`UpsertSalesOrderShortCloseAsync`) closes an order at less
than ordered quantity without cancelling it.

**Evidence.** Service contract:
`BusinessLayer/.../IBusinessService/ISalesService/IMfgPoService.cs`.
Orchestration and the mandatory-reason UI: `Pages/SalesAndLabour_pages/SalesPo_Pages/MfgPOUpsert.razor`
`@code` — `Cancel()` (:3039), `OnItemCancelChanged()` (:3113), `HandleModalConfirmation()` (:3234),
`ShortClosePo()` (:3284), `CancelItem()` (:3314), `CancelPO()` (:3382), with
`showCancelReasonBox` / `cancelReason` state.

**Confidence.** Confirmed for the service contract; **Confirmed** that the *sequencing and
the mandatory-reason enforcement live in the Razor page*, not in the service.
**Disposition.** Preserve the behaviour; **relocate the orchestration into the service**.
**Migration note.** This is the archetype of the extraction problem. The API needs
`POST /api/sales-orders/{id}/cancel { reason }` and
`POST /api/sales-orders/{id}/lines/{lineId}/cancel { reason }` that perform the *whole*
sequence server-side. Do not reimplement the sequence in React.

---

## Authentication and authorization rules

### BR-AUTH-001 — Login requires an active user and a verified PBKDF2 hash

**Statement.** `LoginAsync` matches on `UserName` **and** `IsActive == true`, then verifies
the password with `IPasswordHasher<User>.VerifyHashedPassword`. Only
`PasswordVerificationResult.Success` authenticates.

**Evidence.** `Repository/MasterRepository/Admins/UserRepository.cs:34-49`.
**Confidence.** Confirmed. **Disposition.** Preserve unchanged (existing hashes must keep working).

### BR-AUTH-002 — Screen rights are deny-by-default and enforced only in the UI

**Statement.** For each of the 152 catalogued screens, a user has `CanView`, `CanCreate`,
`CanEdit`, `CanDelete`, `IsHide`. Absence of a `UserRight` row for a screen means **no
right** (`?? false`). Enforcement happens exclusively in `BaseUserRightsComponent`, which
296 of 333 pages inherit. **No service or repository checks permissions.**

**Evidence.** `Shared/RightsHelper.cs` (the `?? false` defaults);
`Shared/BaseUserRightsComponent.cs:22-40`; negative evidence from grepping
`BusinessLayer/`, `Repository/`, `Services/`.
**Confidence.** Confirmed.
**Disposition.** **Preserve the model; relocate the enforcement to the server.**
**Migration note.** Blocker. See [ADR-004](../decisions/ADR-004-server-side-authorization.md).

### BR-AUTH-003 — QR login requires token match, QR enabled, and active user

**Statement.** `GetUserByQrToken(Guid)` returns a user only when
`QrToken == token && IsQrEnabled && IsActive`.
**Evidence.** `UserRepository.cs:52-60`.
**Confidence.** Confirmed. **Gap (Confirmed):** `QrExpiryDate` is stored but **not checked**
in this query — expired QR tokens appear to remain valid. See Q-05. Risk R-16.

---

## Approval rules

### BR-APPR-001 — Rejection requires a reason; approvals are audited by level

**Statement.** `RejectAsync` / `BulkRejectAsync` take a mandatory `reason`; every approve
or reject writes `ApprovalHistory { RecordId, ApprovalType, Level, Status, ActionBy, ActionDate, Reason }`.
A user may act only on document types and levels granted in `UserAuthority`
(12 type/level pairs: Sales Quotation, PR, PO, Purchase SCN, SubCon SCN, Prod Assy SCN,
Prod Comp SCN, Labour SCN, Leave, Route Card, Sales Order, Stock Request).

**Evidence.** `BusinessLayer/.../IPlanningService/IApprovalService.cs`;
`Data/Planning/ApprovalHistory.cs`; `Data/Master/Admin_Module/UserAuthority.cs`.
**Confidence.** Confirmed.
**Disposition.** Preserve.
**Migration note.** The `IApprovalService` interface currently `using static`-imports a
Razor page type — decouple first. Then enforce `UserAuthority` **server-side** in the
approval endpoints, not just in the UI.

---

## Reporting rules

### BR-RPT-001 — Print templates resolve per tenant with a `default` fallback, and print settings drive copies

**Statement.** `Generate_Report` looks for `{reportRoot}/{tenant.Hostname}/{fileName}`,
falling back to `{reportRoot}/default/{fileName}`; throws if neither exists. The report's
DB connection is overwritten with the **tenant's** connection string. `PrintSetting` rows
for the screen control watermark, logo, ISO logo, copy name, and number of copies
(0 is coerced to 1); inactive settings are skipped. A `Sp_Print_CompanyDetails` data source
and a data source named after the passed `procedureName` are both mandatory.

**Evidence.** `Services/ReportViewer/ReportService.cs:39-120`.
**Confidence.** Confirmed.
**Disposition.** Preserve wholesale.
**Migration note.** Wrap in `GET /api/reports/{screen}/{id}?template=…` returning
`application/pdf`. No logic changes required.

---

## Tenancy rules

### BR-TEN-001 / BR-TEN-002

See [`architecture/multi-tenancy.md`](../architecture/multi-tenancy.md) — recorded there
to avoid duplication.

---

## Rules known to exist but not yet extracted

These are visible in the code structure but have not been individually traced. Each is an
open investigation.

| Area | Where it lives | Investigation |
|---|---|---|
| Document numbering (next PO/DC/invoice number, financial-year suffixes) | ~20 `SELECT TOP 1 … ORDER BY … DESC` repository methods; `DcRunningNumber`, `InvoiceAutoRunningNumber`, `FinancialYearHelper.cs` | INV-012 |
| Balance-quantity derivation across every `Ref*SubId` chain | services + `@code` | INV-013 |
| Payroll calculation (salary heads, loan deduction, attendance→salary) | `HumanResourceService/PayrollService/SalaryService.cs` | INV-014 |
| e-Invoice / e-Way payload construction and error handling | `E_Invoice/`, `EinvoiceDatabaseService.cs` (2,136 LOC) | INV-015 |
| Costing / labour-cost rules | `CostingService.cs`, `AssemblyDefLabourService.cs` (1,839 LOC) | INV-016 |
| Route-card operation sequencing and WIP | `PlanningService/RouteCardService.cs` (1,934 LOC) | INV-017 |
| Subcontract material reconciliation (`SubConGRNTrack`) | `SubConGRNService.cs` (5,631 LOC) | INV-018 |
| Labour DC outgoing rules | `LabourDcOutgoingService.cs` (6,112 LOC) + 6,528-LOC page | INV-019 |
| TDS / advance adjustment | `AccountsService/` | INV-020 |
