---
doc_id: KB-061
title: Delete-Guard Audit — every `(bool CanDelete, string Message)` guard in the business layer (INV-025)
module: risks
source_files:
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/**
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/IBusinessService/**
  - V.SMART/V.SMART.Shared/Pages/**
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
  - db/stored-procedures/
  - tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs
entities: []
api_endpoints:
  - "DELETE /api/v1/currencies/{id}"
database_tables: []
business_rules: [BR-SO-001, BR-SO-002, BR-AUTH-002]
status: complete
confidence: mixed
last_verified: 2026-08-21
dependencies: [KB-060, KB-030, KB-003, KB-020, KB-002, KB-102]
---

# Delete-Guard Audit — INV-025

**This document completes INV-025.** It is the second half of R-08's action item in
[KB-060](technical-debt-register.md): *"then audit all ~40 `CanDelete…Async` methods for the
same pattern."*

**Read the headline first, because it inverts the task's premise.**

> **The R-08 defect class is essentially eradicated.** Across the entire
> `(bool …, string Message)` guard family — **93 methods**, of which **79** are the
> `(bool CanDelete, string Message)` family proper — a detector for *"computes one boolean,
> tests another"* finds **exactly one surviving instance**, and it was **already known and
> already recorded** in KB-060 (`MfgPoService.cs:613-615`). M0-09 left it deliberately.
>
> **The real defects are elsewhere, and they are larger.** Guards that are never called
> (**14 of 79**). Guards that can never refuse under **any** input (**3**, plus a fourth that
> refuses on exactly one condition and is inert after it — **4** carry a `Stub guard` or
> *Dead computation* verdict). Delete paths with no guard of any shape (**29 service files**).
> Guards that run **outside** the delete transaction with a user round-trip in the gap
> (**77 of 79**). None of these is what the task went looking for; all of them matter more to
> the API than the defect it did go looking for.

---

## 1. Scope and method

### 1.1 The task's premise was incomplete — corrected here

The task file (`execution/tasks/M0-10.md:68-70`) scopes the audit at **64** implementations
of `public async Task<(bool CanDelete, string Message)> CanDelete…`; KB-060 says **63**.
**Both figures count only guards *named* `CanDelete*`.** The family defined by its **return
shape** is larger.

| Discovery pass | Command (run 2026-08-21, from the repository root) | Hits |
|---|---|---|
| **Pass 1 — return shape, any method name** | `grep -rnE --include=*.cs --exclude-dir=obj --exclude-dir=bin "public async Task<\(bool CanDelete, string Message\)> [A-Za-z0-9_]+" V.SMART/` | **79** across **61** files |
| **Pass 1b — the task's own signature (`CanDelete` prefix)** | `grep -rn --include=*.cs --exclude-dir=obj --exclude-dir=bin "public async Task<(bool CanDelete, string Message)> CanDelete" V.SMART/` | **64** |
| **Pass 1c — anything outside `BusinessLayer/`** | pass 1 piped through `grep -v "BusinessLayer/"` | **0** |
| **Pass 2 — the wider tuple family (any boolean name)** | `grep -rnE --include=*.cs --exclude-dir=obj --exclude-dir=bin "public async Task<\(bool [A-Za-z0-9_]+, string Message\)> [A-Za-z0-9_]+" V.SMART/` | **93** — 79 `CanDelete`, 10 `CanItemCancel`, 3 `IsValid`, 1 `Success` |
| **Pass 3 — interface declarations** | `grep -rhoE --include=*.cs "Task<\(bool CanDelete, string Message\)> [A-Za-z0-9_]+" …/IBusinessService/` \| `sed 's/.*> //'` \| `sort -u` | **71** distinct names |
| **Pass 3b — implementations, distinct names** | pass 1 reduced the same way | **71** distinct names |

**Both greps excluded `bin/` and `obj/` via `--exclude-dir`. State this when reproducing —
raw counts without those exclusions differ**, because build output contains generated copies.

The 93rd member of pass 2 is `(bool Success, string Message) DeleteEmployeeAsync`
(`MasterService/HRMasterService/EmployeeService.cs:162`), which is a **delete method, not a
guard**. The guard population of pass 2 is therefore **92** = 79 + 10 `CanItemCancel` + 3
`IsValid`.

### 1.2 Reconciling the passes

**Pass 1 minus pass 1b = 15 guards that return the identical tuple under a different name.**
These are missed both by the `Async`-suffix trap the task file warns about *and* by the
`CanDelete` prefix it recommends instead:

| Method | File:line (under `BusinessLayer/BusinessService/`) |
|---|---|
| `CanRemoveEnquiryAsync` | `OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs:939` |
| `CanRemoveQuoteAsync` | `OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs:907` |
| `CanRemoveSubConDcOutAsync` | `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:2347` |
| `CanRemoveSubConGRNAsync` | `OutSourcingService/SubContractGRNService/SubConGRNService.cs:4208` |
| `CanRemoveProductionReturnAsync` | `ProductionService/ProductionReturnCompService.cs:4763` |
| `ValidateBeforeDeleteBySlNoAsync` | `SalesService/ContractReviewService.cs:107` |
| `ToCheckStockQtyIssued` (×3) | `LabourServices/LabourGRNService.cs:162`; `OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs:1035`; `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1362` |
| `ToCheck_Stock_Qty_Issued` | `InventoryService/StoreInterTransService.cs:761` |
| `NeedTocheckRejection` (×2) | `LabourServices/LabourSCNService.cs:229`; `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1690` |
| `CheckAnyTransectionsExists` | `OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs:908` |
| `CanCancelAndRevokelabourDcOutgoingAsync` | `LabourServices/LabourDcOutgoingService.cs:4260` |
| `CanCancelAndRevokelabourSCNAsync` | `LabourServices/LabourSCNService.cs:1638` |

**Confidence: Confirmed** — every line re-read 2026-08-21.

**The three non-`Async`-suffixed methods the task file names are all present and audited** —
`CanDeletePurchaseQuote` (`PurchaseQuoteService.cs:864`), `CanDeleteSubConDcOutgoing`
(`SubConDcOutService.cs:2262`), `CanDeleteProductionLog` (`ProductionLogService.cs:192`).
**Scoping this audit by the `Async` suffix would have missed all three.** Scoping it by the
`CanDelete` prefix — the task file's own recommended remedy — would still have missed the
**15** above. **Only the return shape finds the whole family.**

**Pass 3 vs pass 3b — the interface surface maps 1:1.** Both set differences are empty:

```
comm -23 declared.txt implemented.txt   → (empty)   no declared-but-unimplemented guard
comm -13 declared.txt implemented.txt   → (empty)   no implemented-but-undeclared guard
```

The sole mismatch anywhere in the interface folder is a **commented-out declaration**:
`IBusinessService/IOutSourcingService/ISubContractSCNService/ISubConSCNService.cs:50` —
`//Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int scnId, int screenCode);`.
**Confirmed.**

(79 implementations resolve to 71 distinct names because six names are implemented more than
once: `CanDeleteAssydefAsync` ×2, `CanDeleteJobOrderAsync` ×2, `CanDeleteMINAsync` ×2,
`CanDeleteProductionSCNAssyAsync` ×3, `NeedTocheckRejection` ×2, `ToCheckStockQtyIssued` ×3.)

### 1.3 The R-08 detector, and how its blind spots were closed

The naive detector — *"a `bool x = …` whose name is absent from the `if` on the next line"* —
has three blind spots: multi-line declarations, ternary tests, and `switch` expressions. It
was replaced with a **declaration-liveness** detector immune to all three:

> For every guard body, collect every `bool NAME =` / `var NAME =` declaration, then check
> whether `NAME` appears **anywhere later in the same body**. A name declared and never
> referenced again is either the R-08 shape (a different boolean was tested) or a dead
> computation. Both are findings; neither can hide behind a ternary.

Run over the full **93-method** tuple family, it produces **three** hits and no others:

| Hit | Verdict |
|---|---|
| `SalesService/MfgPoService.cs:613` — `hasCR` in `CanSalesOrderItemCancelCheckAsync` | **REAL — the one surviving R-08 instance.** §3.1 |
| `PlanningService/EstimateService.cs:747` — `EstimateSubIds` in `CanDeleteEstimateAsync` | **REAL — dead computation inside a stub guard.** §3.3(c) |
| `HumanResourceService/AppointmentLetter_Service/AppointmentLetterService.cs:65` — `PoSubIds` in `CanDeleteAppointmentletterAsync` | **REAL — dead computation, plus a dereference-before-null-check.** §3.3(d) |

**Blind-spot census (Confirmed):** exactly **one** ternary-form guard exists in the whole
family (`LabourSCNService.cs:1638-1648`, and it is **correct** — `hasTransaction` is computed
at `:1642` and tested in the ternary at `:1646`); **zero** `switch`-form guards exist.

**Negative result worth not repeating:** an earlier, `if`-scoped detector reported
`LabourSCNService.cs:1646` and `ProductionReturnAssyService.cs:398,438` as suspects. Both are
**false positives** — the first is the ternary above; the second is `usedStock` declared once
in each of two mutually exclusive branches (`:398` and `:438`), each tested in its own branch.
The liveness detector raises neither.

**The detector was calibrated against known-defective code.** Run against `git show 8e3b19d^`
(pre-M0-09), it correctly reports the two BR-SO-002 defects that M0-09 fixed. A detector that
finds nothing is only meaningful if it has been shown to find something.

### 1.4 Call-site discovery — and two methodology traps

| Measurement (2026-08-21, over all 71 guard names) | Result |
|---|---|
| Distinct Razor pages calling a guard | **67** |
| Razor call sites | **68** |
| API controller call sites | **1** — `V.SMART.Api/Controllers/CurrencyController.cs:101` |
| **Service-internal (unqualified) calls** | **2** — `EmployeeService.cs:174`, `ProductionLogService.cs:288` |

> **Trap 1 — a dot-anchored grep undercounts, in exactly the wrong direction.**
> `grep "\.CanDelete"` finds 62 of the 64 `CanDelete*` call sites and **misses the only two
> that matter**, because those two are unqualified `await CanDelete…(…)` calls **inside the
> service, inside the delete transaction**. An audit using only the dot-anchored form would
> conclude *"no delete guard is ever enforced server-side"*, which is false. Search for
> `\.NAME(` **and** `await NAME(`.

> **Trap 2 — attribute by injected interface, never by variable name.**
> `Pages/Inventory(Stock)_Module_Pages/Stock Issue Request/StockIssueRequestList.razor:730`
> calls `minService.CanDeleteMINAsync(minId)` — but `:13` injects
> `@inject IStockIssueRequestService minService`. The variable is *named* `minService` while
> being a **different interface** from the `IMINService minService` at
> `MaterialIssueList.razor:12`. Attribution by variable name would wrongly mark
> `StockIssueRequestService.cs:319` unreachable. **It is reachable.** Confirmed.

### 1.5 Near-neighbour name sweep — all zero

`grep` over `V.SMART/**/*.cs` and `**/*.razor` for guards doing the same job under a different
name. **Every one returned zero hits (Confirmed negative):**

`IsDeletable` · `CanBeDeleted` · `CheckDelete` · `AllowDelete` · `IsDeleteAllowed` ·
`EnsureDeletable` · `VerifyDelete` · `PreDelete` · `CanDrop`

**Do not repeat this sweep.** The only aliases that exist are the 15 in §1.2, and they were
found by return shape, not by name.

> **Grep noise to know about.** `@if (CanDelete)` in list pages (e.g.
> `Pages/Master_Module_pages/Currency_Pages/CurrencyList.razor:191`) is the **screen right**
> `UserRight.CanDelete` (`Data/Master/Admin_Module/UserRight.cs:31`), **not** a guard result.
> The same column accounts for `CanDelete` hits in **115 files** under `Migrations/`. An
> unfiltered `grep -c CanDelete` over the repository is dominated by these and tells you
> nothing about guards.

---

## 2. Complete inventory — all 79 guards

Grouped by module ([KB-020](../modules/module-inventory.md) § Module table, via its *Service
folder* column). **Every guard appears, including every one judged `Correct`** — the point of
the table is that the next session need not re-read them.

- **Booleans/locals computed** and **tested** are transcribed mechanically from the method
  body (declaration site and test site, with line numbers), never inferred from variable
  names — the R-08 defect is precisely that the names look right. `—` means the method
  declares no local of that kind (it tests query results inline).
- **Reachable** = does any call site resolve to *this* implementation, attributed by injected
  interface (§1.4), not by name.
- Verdicts use the task's taxonomy plus **one declared extension — `Stub guard`** — defined in
  §3.3. *Dead computation* did not fit two of those four, because they compute nothing to
  discard.

| Module | File (under `…/BusinessLayer/BusinessService/`) | Lines | Signature | Computed (line) | Tested (line) | Verdict | Reachable | Confidence |
|---|---|---|---|---|---|---|---|---|
| 1 Masters — Admin | `MasterService/AdminService/UserService.cs` | 252–279 | `CanDeleteUserAsync(int userId)` | hasUserAuthority:263 | user:260, hasUserAuthority:266 | Correct | yes | Confirmed |
| 10 Labour Work | `LabourServices/LabourDcOutgoingService.cs` | 4260–4294 | `CanCancelAndRevokelabourDcOutgoingAsync(int Dcid, int DcSubId)` | hasTransaction:4275 | hasTransaction:4281 | Correct | yes | Confirmed |
| 10 Labour Work | `LabourServices/LabourDcOutgoingService.cs` | 490–546 | `CanDeleteLabourDcOutgoingAsync(int DcId, int screenCode, string refNo)` | hasPo:506 | dcOutgoing:499, hasPo:512, es:519, usedStock:536 | Correct | yes | Confirmed |
| 10 Labour Work | `LabourServices/LabourGRNService.cs` | 162–189 | `ToCheckStockQtyIssued(int GRNId, int screenCode, string refNo)` | — | usedStock:179 | Correct | **no — 0 callers** | Confirmed |
| 10 Labour Work | `LabourServices/LabourGRNService.cs` | 191–245 | `CanDeleteLabourGRNAsync(int GRNId, int screenCode)` | hasSCN:207 | grn:200, hasSCN:213, es:216, usedStock:235 | Correct | yes | Confirmed |
| 10 Labour Work | `LabourServices/LabourSCNService.cs` | 1638–1655 | `CanCancelAndRevokelabourSCNAsync(int scnId, int scnSubId)` | hasTransaction:1642 | hasTransaction:1646 (**ternary**, §1.3) | Correct | yes | Confirmed |
| 10 Labour Work | `LabourServices/LabourSCNService.cs` | 229–260 | `NeedTocheckRejection(int refGRNSubId, decimal rejQty)` | — | grnSub:235, poSub:241, po:247, rejQty:250 | Correct | yes | Confirmed |
| 10 Labour Work | `LabourServices/LabourSCNService.cs` | 262–318 | `CanDeleteLabourSCNAsync(int SCNId, int screenCode, string refNo)` | hasPo:278 | LabSCN:271, hasPo:284, es:291, usedStock:308 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/MaterialRequisitionService/MaterialReqService.cs` | 471–519 | `CanDeleteMaterialReqAsync(int mreqId)` | hasPurchEnq:488, hasPurchPo:495, hasCancelledItems:505 | mreq:476, hasPurchEnq:492, hasPurchPo:499, hasCancelledItems:509 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs` | 1105–1172 | `CanDeleteEnquiryAsync(int enquiryId)` | hasQuote:1118 | hasQuote:1122, vendorSummary:1136, enquiryMeta:1155, s:1161 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs` | 908–937 | `CheckAnyTransectionsExists(int enquirySubId)` | — | totalQty:925, totalBalQty:925 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs` | 939–966 | `CanRemoveEnquiryAsync(int EnquiryId, int enqsubid)` | Quotation:952 | Quotation:956 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchOrSubConPoService/PuchPoService.cs` | 1574–1647 | `CanDeletePurchaseOrderAsync(int poId)` | hasPurchPo:1595, isPoRevised:1599, grn(var):1617, dcout(var):1619 | hasPurchPo:1596, isPoRevised:1601, grn:1621, dcout:1621, PO:1624, s:1630 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs` | 864–904 | `CanDeletePurchaseQuote(int QuoteId)` | hasPo:880 | quote:873, hasPo:886, es:893 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs` | 907–979 | `CanRemoveQuoteAsync(int QuoteId, int QuoteSubId)` | Quotation:921, hasPurchPo:939 | Quotation:925, hasPurchPo:941, Quote:961 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs` | 1035–1062 | `ToCheckStockQtyIssued(int GRNId, int screenCode, string refNo)` | — | usedStock:1052 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs` | 1490–1542 | `CanDeleteGRNAsync(int grnId, int screenCode)` | hasSCN:1506 | grn:1499, hasSCN:1512, usedStock:1523, es:1531 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs` | 1362–1389 | `ToCheckStockQtyIssued(int SCNId, int screenCode, string refNo)` | — | usedStock:1379 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs` | 1690–1721 | `NeedTocheckRejection(int refGRNSubId, decimal rejQty)` | — | grnSub:1696, poSub:1702, po:1708, rejQty:1711 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs` | 1723–1763 | `CanDeletePurchaseSCNAsync(int scnId)` | hasInvoice:1739 | purchaseSCN:1732, hasInvoice:1745, es:1752 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/Purchase_Invoice_Service/PurchaseInvoiceService.cs` | 1283–1305 | `CanDeletePurchaseInvAsync(int invId)` | invoice:1287 | invoice.Balance/GrandTotal:1292, invoice.InvCancel/ShortClose:1295 | Correct — but **NRE on missing row**, §5.3a | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs` | 138–176 | `CanDeleteSubconDcOutgoingAsync(int dcId, int screenCode)` | hasGRN:154 | subConDc:147, hasGRN:160, es:163 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs` | 2262–2346 | `CanDeleteSubConDcOutgoing(int IssueId)` | hasPurchPo:2297, QuoteShortClose:2327 | hasPurchPo:2298, Quote:2318, s:2324, QuoteShortClose:2332 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs` | 2347–2424 | `CanRemoveSubConDcOutAsync(int IssueId, int IssuedIdSub)` | Quotation:2361, hasPurchPo:2379 | Quotation:2365, hasPurchPo:2381, Quote:2401, s:2408 | Correct | **no — 0 callers** | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/SubContractGRNService/SubConGRNService.cs` | 1397–1451 | `CanDeleteSubconGRNAsync(int GRNId, int screenCode)` | hasSCN:1413 | grn:1406, hasSCN:1419, es:1422, usedStock:1441 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/SubContractGRNService/SubConGRNService.cs` | 4208–4285 | `CanRemoveSubConGRNAsync(int ReturnId, int ReturnSubId)` | Quotation:4222, hasPurchPo:4240 | Quotation:4226, hasPurchPo:4242, Quote:4262, s:4269 | Correct | yes | Confirmed |
| 11/12/13 OutSourcing, Purchase, SubContract | `OutSourcingService/SubContractSCNService/SubConSCNService.cs` | 279–333 | `CanDeleteSubConSCNAsync(int SCNId, int screenCode, string refNo)` | hasPo:295 | SubConSCN:288, hasPo:301, usedStock:323 | Correct | yes | Confirmed |
| 14 Planning | `PlanningService/EstimateService.cs` | 735–760 | `CanDeleteEstimateAsync(int EstiamateId)` | — | Estimate:744 | **Dead computation** | yes | Confirmed |
| 14 Planning | `PlanningService/JobOrderService.cs` | 1193–1216 | `CanDeleteJobOrderAsync(int jobId)` | hapProductionIssueAssyExists:1202 | jobOrder:1199, hapProductionIssueAssyExists:1206 | Correct | yes | Confirmed |
| 14 Planning | `PlanningService/RcReleaseService.cs` | 803–828 | `CanDeleteRcReleaseAsync(int rcReleaseId)` | rcRelease:807, hasRouteCard:809 | hasRouteCard:815, rcRelease.IsCancel/ShortClose:818 | Correct — but **NRE on missing row**, §5.3a | yes | Confirmed |
| 14 Planning | `PlanningService/RouteCardService.cs` | 1840–1863 | `CanDeleteJobOrderAsync(int jobId)` | hapProductionIssueAssyExists:1849 | jobOrder:1846, hapProductionIssueAssyExists:1853 | Correct | **no — 0 callers** | Confirmed |
| 14 Planning | `PlanningService/RouteCardService.cs` | 86–141 | `CanDeleteRoutecardAsync(int rcId)` | hasDailyLog:102, hasProductionComp:112, hasSubcon:122 | routeCard:95, hasDailyLog:108, hasProductionComp:118, hasSubcon:128 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionIssueCompService.cs` | 1386–1432 | `CanDeleteProductionDcgoingAsync(int IssueId, int screenCode)` | hasGRN:1402, hasRC:1415 | subConDc:1395, hasGRN:1408, hasRC:1418 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionLogService.cs` | 192–273 | `CanDeleteProductionLog(long logId, int screenCode)` | — | prodLog:212, currentProcess:219, usedStock:235, nextProcess:247, previousRejectedQty:259 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionLogService.cs` | 635–666 | `CanDeleteProductionSCNAssyAsync(int scnId, int screenCode)` | — | usedStock:655 | Correct | **no — 0 callers** | Confirmed |
| 15/16 Production | `ProductionService/ProductionReturnAssyService.cs` | 378–457 | `CanDeleteProductionReturnAssyAsync(int returnId, int screenCode)` | hasSCNTrans:399, usedStock:408, usedStock:439 | returnAssy:386, hasSCNTrans:404, usedStock:415, usedStock:446 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionReturnCompService.cs` | 4763–4809 | `CanRemoveProductionReturnAsync(int ReturnId, int ReturnSubId)` | Quotation:4777, hasPurchPo:4795 | Quotation:4781, hasPurchPo:4797 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionReturnCompService.cs` | 731–794 | `CanDeleteProductionReturnCompAsync(int returnId, int screenCode)` | hasSCN:747, hasRC:763 | grn:740, hasSCN:753, hasRC:766, usedStock:782 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionSCNAssyService.cs` | 203–234 | `CanDeleteProductionSCNAssyAsync(int scnId, int screenCode)` | — | usedStock:223 | Correct | yes | Confirmed |
| 15/16 Production | `ProductionService/ProductionSCNCompService.cs` | 196–247 | `CanDeleteProductionSCNAssyAsync(int SCNId, int screenCode, string refNo)` | hasPo:212 | SubConSCN:205, hasPo:218, usedStock:237 | Correct | yes | Confirmed |
| 17 Inventory / Stock | `InventoryService/MINService.cs` | 286–323 | `CanDeleteMINAsync(int minId)` | hasPerformaInv:301, hasDcTrans:309 | hasPerformaInv:304, hasDcTrans:312 | Correct | yes | Confirmed |
| 17 Inventory / Stock | `InventoryService/SCNGenService.cs` | 776–806 | `CanDeleteSCNGenAsync(int scnGenId, int screenCode)` | — | usedStock:796 | Correct | yes | Confirmed |
| 17 Inventory / Stock | `InventoryService/StockIssueRequestService.cs` | 319–353 | `CanDeleteMINAsync(int minId)` | isTransactionMade:334 | isTransactionMade:339 | Correct | yes | Confirmed |
| 17 Inventory / Stock | `InventoryService/StoreInterTransService.cs` | 211–222 | `CanDeleteISTAsync(int istId)` | — | — | **Stub guard** | **no — 0 callers** | Confirmed |
| 17 Inventory / Stock | `InventoryService/StoreInterTransService.cs` | 761–788 | `ToCheck_Stock_Qty_Issued(int ISTId, int screenCode, string refNo)` | — | usedStock:778 | Correct | yes | Confirmed |
| 17 Inventory / Stock | `InventoryService/ToolCribReturnService.cs` | 494–524 | `CanDeleteToolCribReturnAsync(int tcReturnId, int screenCode)` | — | usedStock:514 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/AssemblyDefLabourService.cs` | 1617–1671 | `CanDeleteAssydefAsync(int id)` | hasMfgEnquiry(var):1633, hasMfgQuote(var):1638, hasMfgPo(var):1643, hasJobcard(var):1648, StoreTransferNoteCount(var):1653, hasMaterialIssueSub(var):1658 | assmblyDef:1625, usedIn:1630, hasMfgEnquiry:1635, hasMfgQuote:1640, hasMfgPo:1645, hasJobcard:1650, StoreTransferNoteCount:1655, hasMaterialIssueSub:1660 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/AssemblyDefService.cs` | 1504–1558 | `CanDeleteAssydefAsync(int id)` | hasMfgEnquiry(var):1520, hasMfgQuote(var):1525, hasMfgPo(var):1530, hasJobcard(var):1535, StoreTransferNoteCount(var):1540, hasMaterialIssueSub(var):1545 | assmblyDef:1512, usedIn:1517, hasMfgEnquiry:1522, hasMfgQuote:1527, hasMfgPo:1532, hasJobcard:1537, StoreTransferNoteCount:1542, hasMaterialIssueSub:1547 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/CategoryService.cs` | 212–240 | `CanDeleteCategoryAsync(int categoryId)` | hasItem:223 | category:220, hasItem:227 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/FactorService.cs` | 96–119 | `CanDeleteFactorsAsync(int factorId)` | — | factor:104, usedIn:109 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/GroupingService.cs` | 136–156 | `CanDeleteGroupingAsync(int grouppingId)` | — | grouping:145 | **Stub guard** | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/ItemService.cs` | 1268–1294 | `CanDeleteItemAsync(int itemId)` | — | item:1276, usedIn:1284 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/ProcessService.cs` | 90–113 | `CanDeleteProcessAsync(int processId)` | — | process:98, usedIn:103 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/RawMaterialService.cs` | 118–141 | `CanDeleteRMAsync(int rmId)` | — | store:126, usedIn:131 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/InventoryService/StoreService.cs` | 94–120 | `CanDeleteStoreAsync(int storeId)` | — | store:102, usedIn:110 | Correct | yes | Confirmed |
| 2 Masters — Inventory | `MasterService/TermsAndConditionsService.cs` | 196–222 | `CanDeleteTermsAsync(int termsId)` | — | terms:204, usedIn:209 | Correct | yes | Confirmed |
| 20 Human Resources | `HumanResourceService/AppointmentLetter_Service/AppointmentLetterService.cs` | 43–78 | `CanDeleteAppointmentletterAsync(int appointmentId)` | hasStaff:54 | hasStaff:58, po:62 | **Dead computation** | yes | Confirmed |
| 20 Human Resources | `HumanResourceService/AttendanceService/AttendanceService.cs` | 102–134 | `CanDeleteIdAsync(int Id)` | — | attendance:111, salaryExists:122 | Correct | yes | Confirmed |
| 20 Human Resources | `HumanResourceService/EmployeeLoanService/StaffLoanService.cs` | 233–262 | `CanDeleteStaffLoanAsync(int loanId)` | salaryExists:245 | staffloan:241, salaryExists:250 | Correct | yes | Confirmed |
| 20 Human Resources | `HumanResourceService/OfferLetter_Service/OfferLetterService.cs` | 45–76 | `CanDeleteOfferLetterAsync(int offerId)` | hasAppointment:58 | Offer:54, hasAppointment:62 | Correct | yes | Confirmed |
| 20 Human Resources | `HumanResourceService/PayrollService/SalaryService.cs` | 206–241 | `CanDeleteSalaryAsync(int rowId)` | hasSalary:222 | loan:215, hasSalary:228 | Correct | yes | Confirmed |
| 3 Masters — General | `MasterService/GeneralService/CustomerService.cs` | 142–168 | `CanDeleteCustomerAsync(int custId)` | — | customer:150, usedIn:155 | Correct | yes | Confirmed |
| 3 Masters — General | `MasterService/GeneralService/MachineService.cs` | 122–145 | `CanDeleteMachineAsync(int machineId)` | — | machine:130, usedIn:135 | Correct | yes | Confirmed |
| 3 Masters — General | `MasterService/GeneralService/VendorService.cs` | 138–164 | `CanDeleteVendorAsync(int vendorId)` | — | vendor:146, usedIn:151 | Correct | yes | Confirmed |
| 4 Masters — Accounts | `MasterService/AccountsService/CostCenterService.cs` | 137–166 | `CanDeleteCostCenterAsync(int CostId)` | — | costCenter:145, usedIn:156 | Correct | yes | Confirmed |
| 4 Masters — Accounts | `MasterService/AccountsService/CurrencyService.cs` | 323–348 | `CanDeleteCurrencyAsync(int id)` | — | currency:331, usedIn:338 | Correct | yes | Confirmed |
| 4 Masters — Accounts | `MasterService/AccountsService/ExpenseService.cs` | 121–144 | `CanDeleteExpenseAsync(int id)` | — | expense:129, usedIn:134 | Correct | **no — 0 callers** | Confirmed |
| 4 Masters — Accounts | `MasterService/AccountsService/IncomeService.cs` | 121–144 | `CanDeleteIncomeAsync(int id)` | — | income:129, usedIn:134 | Correct | **no — 0 callers** | Confirmed |
| 5 Masters — HR | `MasterService/HRMasterService/CandidateService.cs` | 32–60 | `CanDeleteCandidateAsync(int candidateID)` | hasOffer:44 | candidate:40, hasOffer:48 | Correct | yes | Confirmed |
| 5 Masters — HR | `MasterService/HRMasterService/EmployeeLeavebalanceService.cs` | 112–136 | `CanDeleteEmpLeaveBalAsync(int leaveBalId)` | — | employeeLeaveBalance:121, usedIn:126 | Correct | yes | Confirmed |
| 5 Masters — HR | `MasterService/HRMasterService/EmployeeService.cs` | 248–290 | `CanDeleteEmployeeAsync(int staffId)` | hasUser:259, hasLeaveApp:266, hasLeaveBal:272 | staff:256, hasUser:262, hasLeaveApp:268, hasLeaveBal:274 | Correct | yes | Confirmed |
| 5 Masters — HR | `MasterService/HRMasterService/LeaveTypeService.cs` | 121–144 | `CanDeleteLeaveTypeAsync(int typeId)` | — | leaveType:129, usedIn:134 | Correct | yes | Confirmed |
| 5 Masters — HR | `MasterService/HRMasterService/ShiftAllocationService.cs` | 127–154 | `CanDeleteShiftAllocationAsync(int shiftId)` | — | shiftAllocation:135, usedIn:140 | Correct | yes | Confirmed |
| 8/9 Sales & Mfg Work | `SalesService/ContractReviewService.cs` | 107–132 | `ValidateBeforeDeleteBySlNoAsync(int slNo)` | — | master:113, isUsed:118 | Correct | **no — 0 callers** | Confirmed |
| 8/9 Sales & Mfg Work | `SalesService/ContractReviewService.cs` | 314–337 | `CanDeleteReviewMasterAsync(int reviewId)` | — | contractReviewMaster:322, usedIn:327 | Correct | yes | Confirmed |
| 8/9 Sales & Mfg Work | `SalesService/EnquirySalesService.cs` | 264–315 | `CanDeleteSalesEnquiryAsync(int enqId)` | hasEnqFeasibility:281, hasQuotation:289 | enquiry:273, hasEnqFeasibility:285, hasQuotation:295, es:304 | Correct | yes | Confirmed |
| 8/9 Sales & Mfg Work | `SalesService/MfgPoService.cs` | 465–565 | `CanDeleteSalesOrderAsync(int poId)` | hasDc:482, hasInvoice:491, hasExpInvoice:499, hasPerforma:509, hasRc:519, hasCR:524, hasMR:529, hasJO:537, hasPCI:545 | po:474, hasDc:487, hasInvoice:496, hasExpInvoice:504, hasPerforma:515, hasRc:520, hasCR:525, hasMR:533, hasJO:541, hasPCI:548, es:555 | Correct | yes | Confirmed |
| 8/9 Sales & Mfg Work | `SalesService/MfgQuotationService.cs` | 416–456 | `CanDeleteSalesQuotationAsync(int quoteId)` | hasPo:432 | quote:425, hasPo:438, es:445 | Correct | yes | Confirmed |
| 8/9 Sales & Mfg Work | `SalesService/PerformaInvService.cs` | 142–165 | `CanDeletePerfromaInvAsync(int invId)` | — | performaInv:151 | Correct | yes | Confirmed |

---

## 3. Defects

### 3.1 The one surviving R-08 instance — `CanSalesOrderItemCancelCheckAsync`

**Severity: High. Confidence: Confirmed — by reading *and* by an executed test (below, §7).**

`V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs:607-615`
(re-verified 2026-08-21):

```csharp
607
608                bool hasRc = await _unitOfWork.RouteCards.GetQueryable().AnyAsync(qs => qs.RefPoId == subItem.PoId);
609                if (hasRc)
610                    return (false, "Cannot cancel this Item as a Route-Card transaction exists.");
611
612
613                bool hasCR = await _unitOfWork.ContractReviews.GetQueryable().AnyAsync(qs => qs.PoId == subItem.PoId);
614                if (hasRc)                                   // ← computes hasCR, tests hasRc
615                    return (false, "Cannot cancel this Item as a Contract Review transaction exists.");
```

**Consequence, in domain terms.** A Sales Order **line** whose only downstream document is a
**Contract Review** can be cancelled. The Contract Review branch is unreachable: by the time
control reaches `:614`, `hasRc` is necessarily `false` (otherwise `:609` would already have
returned), so `:615` never fires whatever the Contract Review query found.

**This is not a new discovery.** It is already recorded in
[KB-060](technical-debt-register.md) R-08 (*"Second unreported same-pattern instance, found by
the M0-09 validator, 2026-08-19"*). **M0-09 left it deliberately**, outside its authorised
two-line surface. This audit's contribution is to confirm it is still present at those exact
lines, and that it is the **only** surviving instance in the entire 93-method family.

**Verified empirically — observed output.** A proving test was written in the shape M0-09
established, added to
`tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs` as
`CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused`, and run un-skipped against
**unmodified** `MfgPoService.cs`. It seeds a `ContractReview` on the Sales Order header id
with **no** Route Card, so `hasRc` is `false`, and asserts the refusal the guard's own
`Message` string promises. Observed, 2026-08-21, verbatim:

```text
  Failed V.SMART.Shared.Tests.Services.MfgPoServiceDeleteGuardTests.CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused [4 s]
  Error Message:
   Assert.Equal() Failure: Values differ
Expected: Tuple (False, "Cannot cancel this Item as a Contract Review trans"···)
Actual:   Tuple (True, "Item can be safely Cancell.")
```

`Actual: Tuple (True, "Item can be safely Cancell.")` **is the defect**: the guard permitted a
cancellation it says it refuses. Full run summaries, the reason the test is left `Skip`-ped
rather than deleted, and the correction of attempt 1's contrary claim are in **§7**.

**Note it is not one of the 79.** The method returns `(bool CanItemCancel, string Message)`,
not `(bool CanDelete, …)`, and it is named `CanSalesOrderItemCancelCheckAsync`. **An audit
scoped by *either* the `Async` suffix or the `CanDelete` prefix would have missed the only
real defect it was chartered to find.** What actually caught it was the **pass-2** family —
`(bool <any>, string Message)`, 93 methods (§1.1) — not the pass-1 shape
`(bool CanDelete, string Message)`, 79 methods, that defines the §2 inventory. So the precise
lesson is narrower than "scope guard work by return shape": it is **scope it by the
*two-element `(bool, string Message)` tuple shape*, with the boolean's name left free**.
Scoping by the *specific* return shape `(bool CanDelete, …)` would have missed this defect
exactly as the name-based scoping did.

**Proposed fix.** Change `hasRc` at `:614` to `hasCR`. One identifier, exactly M0-09's shape.
**Estimate 0.5 d**, which now includes only removing the `Skip` from the proving test this
audit already wrote and left in
`tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs` — it must go green as
the fix lands. **Do not bundle it into another change** — M0-09's value came from a
two-character diff that a reviewer could verify against a test. Proposed as task **M0-10a**
(§8).

### 3.2 Fourteen guards are unreachable — nothing calls them

**Severity: Medium (they are not wrong; they simply never run). Confidence: Confirmed.**

Determined by a per-name call-site census over all 71 guard names, then attributing
duplicate-name hits to the **injected interface** at each call site (§1.4, trap 2).

**Nine names have zero call sites anywhere** in `V.SMART/`, `tests/`, `frontend/` or `docs/`
— the only other occurrences are the interface declaration, the implementation, and in some
cases the method's own `LogDeveloperError` string:

| Guard | File:line | Note |
|---|---|---|
| `CanDeleteExpenseAsync` | `MasterService/AccountsService/ExpenseService.cs:121` | orphan |
| `CanDeleteIncomeAsync` | `MasterService/AccountsService/IncomeService.cs:121` | orphan |
| `CanDeleteISTAsync` | `InventoryService/StoreInterTransService.cs:211` | orphan **and** a stub (§3.3) |
| `CanDeleteSubConDcOutgoing` | `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:2262` | orphan |
| `CanRemoveEnquiryAsync` | `OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs:939` | orphan |
| `CanRemoveQuoteAsync` | `OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs:907` | orphan |
| `CanRemoveSubConDcOutAsync` | `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:2347` | orphan |
| `ValidateBeforeDeleteBySlNoAsync` | `SalesService/ContractReviewService.cs:107` | orphan |
| `ToCheckStockQtyIssued` ×3 | `LabourServices/LabourGRNService.cs:162`; `OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs:1035`; `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1362` | declared in three interfaces, implemented three times, **called zero times**. The `ToCheckStockQtyIssued` strings that appear in five *other* services are copy-pasted `LogDeveloperError` messages inside differently-named methods, not calls. |

**Three further implementations are unreachable because the name resolves elsewhere** — the
call site injects a different interface:

| Guard | File:line | Which implementation the callers actually reach |
|---|---|---|
| `CanDeleteJobOrderAsync` | `PlanningService/RouteCardService.cs:1840` | `JobOrderList.razor:834` calls `jobOrderService.CanDeleteJobOrderAsync`, and `:14` injects `IJobOrderService` → `PlanningService/JobOrderService.cs:1193` |
| `CanDeleteProductionSCNAssyAsync` | `ProductionService/ProductionLogService.cs:635` | `ProductionAssySCNList.razor:630` injects `IProductionSCNAssyservice` (`:15`) → `ProductionSCNAssyService.cs:203`; `ProductionSCNCompList.razor:625` injects `IProductionSCNCompService` (`:11`) → `ProductionSCNCompService.cs:196` |
| `NeedTocheckRejection` | `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1690` | the only call site, `LabourSCNList.razor:745`, injects `ILabourSCNService` (`:13`) → `LabourServices/LabourSCNService.cs:229` |

**Consequence, in domain terms.** Nothing today. These guards are dead code: no delete is
wrongly permitted *because of them*, because they are not consulted. **The consequence is
future**: a delete endpoint author who greps for a guard, finds one, and calls it will get
whichever of these was never exercised against real data — untested logic promoted to
production enforcement in one commit.

**Both duplicate pairs are logically identical**, differing only in message wording —
`"Prouction SCN"` vs `"Prouction SCN Assembly"` (spelling as in source). They are **dead
clones, not divergent logic**, so the risk is duplication rot rather than inconsistency
today.

> **Scoping note for readers of the handover.** Restricted to the 64 `CanDelete*`-named
> guards, this count is **six** — `ExpenseService.cs:121`, `IncomeService.cs:121`,
> `StoreInterTransService.cs:211`, `SubConDcOutService.cs:2262`, `RouteCardService.cs:1840`,
> `ProductionLogService.cs:635`. The other eight come from the 15 renamed guards in §1.2,
> which the `CanDelete`-prefix scope never censused. **14 is the number for the full family.**

**Proposed fix.** Decide per guard: wire it to its delete path, or delete it. The two clones
should simply go. **Estimate 1 d.** Proposed as task **M0-10b** (§8).

### 3.3 Four inert guards — three of which can never refuse — `Stub guard`

**Severity: High for the three reachable ones. Confidence: Confirmed.**

**Three, not four, can never refuse.** The mechanical detector — *"no `(false, …)` return
outside the `catch` block"* — yields exactly **three**: (a), (b) and (c) below. The fourth,
(d) `CanDeleteAppointmentletterAsync`, **does** refuse, on a `Staff`-name match at
`AppointmentLetterService.cs:58-61`; it reached this section through the **liveness** detector
(§1.3) as a *dead computation*, and is grouped here because everything after its single
refusal is inert. Four guards carry a `Stub guard` or *Dead computation* verdict in §2 (§4:
2 + 2); **three** of them cannot refuse at all.

**Taxonomy extension, declared.** The task's six verdicts have no slot for *"the guard
contains no refusal path at all"*. *Dead computation* fits only two of these four; the other
two compute nothing. A seventh verdict, **`Stub guard`**, is therefore introduced and used in
the §2 table: *every non-exception return is `(true, …)`; the method cannot refuse a delete
under any input.*

**All four, with the full body where it is short enough to quote:**

**(a) `InventoryService/StoreInterTransService.cs:211-222` — `CanDeleteISTAsync`.** The body
is one line:

```csharp
211        public async Task<(bool CanDelete, string Message)> CanDeleteISTAsync(int istId)
212        {
213            try
214            {
215                return (true, "Inter Store Transfer can be safely deleted.");
216            }
217            catch (Exception ex)
218            {
219                await _logs.LogDeveloperError(ex, $"Error in CanDeleteISTAsync for ISTId: {istId}");
220                throw new Exception("Error checking Inter Store Transfer delete eligibility", ex);
221            }
222        }
```

Mitigating: it is also unreachable (§3.2). The live Inter-Store-Transfer list calls
`ToCheck_Stock_Qty_Issued` instead (`StoreInterTransList.razor:624`), which does contain real
checks.

**(b) `MasterService/InventoryService/GroupingService.cs:136-156` —
`CanDeleteGroupingAsync`.** Loads the `Grouping` with its `GroupingSubs`, returns
`(true, "Groouping can be safely deleted.")` if it is `null` (`:146`, spelling as in source),
then returns `(true, "Grouping can be safely deleted.")` unconditionally at `:149`. **It is
called from live UI**: `Pages/Master_Module_pages/Grouping_Pages/GroupingList.razor:549`.

**(c) `PlanningService/EstimateService.cs:735-760` — `CanDeleteEstimateAsync`.** Also a
**dead computation**: `EstimateSubIds` is built at `:747-749` and never used; the method then
returns `(true, "Estimation can be safely deleted.")` at `:753`. **Called from live UI**:
`Pages/Planning_Module_Pages/Estimation_Pages/EstimationList.razor:411`.

**(d) `HumanResourceService/AppointmentLetter_Service/AppointmentLetterService.cs:43-77` —
`CanDeleteAppointmentletterAsync`.** Not a pure stub — it does refuse when a `Staff` row
exists whose `StaffName` equals the candidate's name (`:54-61`) — but everything after that
is inert: `PoSubIds` is built at `:65-67` and discarded, and the method returns
`(true, "Sales Order can be safely deleted.")` at `:71`. **Called from live UI**:
`Pages/HumanResource_Pages/AppointmentLetter_Pages/AppointmentLetterList.razor:485`.

> **Second defect in the same method (Confirmed).** `:52` dereferences `po.CandidateID`
> **before** the null check at `:62`. If the appointment letter does not exist, the method
> throws `NullReferenceException` at `:52`, is caught at `:73`, and rethrown as
> `Exception("Error checking Sales Order delete eligibility", ex)` — so a missing row
> surfaces to the user as an error, not as the `(true, …)` the null check intended. The null
> check at `:62-63` is unreachable for the same reason. **This is a defect *class*, not a
> one-off — it has two siblings; see §5.3.**

**Consequence, in domain terms.** Three delete buttons in the live Blazor UI — Grouping,
Estimation, Appointment Letter — perform an eligibility check that says yes to essentially
everything. For Grouping and Estimation it says yes **always**; for Appointment Letter it says
yes to everything except a candidate who already has a `Staff` row of the same name, and — per
the box below — errors rather than answering when the letter does not exist. The user sees a
guard; there is next to no guard. Whether that is correct depends on whether those documents
have downstream references at all, which **cannot be determined from the code** — see
**Q-64**.

**One of them returns message text belonging to a different document**, which is
product-visible. `AppointmentLetterService.cs:63` and `:71` both say *"Sales Order can be
safely deleted."* for an **Appointment Letter**; its `catch` at `:75` logs *"Error in
CanDeleteSalesOrderAsync"* and rethrows *"Error checking Sales Order delete eligibility"*.
Per BR-SO-001's migration note the `Message` strings are **product UX**, so this is a
user-facing defect, not a cosmetic one. (`EstimateService`'s own user-facing messages name
the right document but are misspelled — *"Estmation can be safely deleted."* at `:745`; so is
`GroupingService.cs:146`, *"Groouping…"*. Spelling as in source, quoted verbatim per
BR-SO-001.)

**Proposed fix.** Not a code task first — a **rule-discovery** task: establish, per document,
what downstream references should block deletion, then implement. Folded into **M0-10d**
(§8); the message-string corrections are a 0.25 d subset of **M0-10c**.

### 3.4 Three delete buttons are live with their guard commented out

**Severity: High. Confidence: Confirmed.**

| Page | Commented guard call | Delete still wired to |
|---|---|---|
| `Pages/.../PaymentList.razor:451` | `EnquiryPurchaseService.CanDeleteEnquiryAsync(enquiry.EnquiryId)` | `_PaymentsService.DeletePaymentAsync` (`:418`) |
| `Pages/.../ReceiptList.razor:452` | same | `_ReceiptsService.DeletePaymentAsync` (`:419`) |
| `Pages/.../AdvanceAdjustmentList.razor:433` | same | see §5.1 |

At `PaymentList.razor:441-459` the **entire** `HandleDelete` body is commented out except two
statements:

```csharp
441    private async Task HandleDelete(PaymentsVM paymnets)
442    {
443        // var totalQty = enquiry.EnquiryVendorSubs.Sum(s => s.Qty);
...
451        //      var (canDelete, message) = await EnquiryPurchaseService.CanDeleteEnquiryAsync(enquiry.EnquiryId);
...
457            DeleteEnqID = paymnets.PaymentId;
458            _JS.InvokeVoidAsync("ShowConfirmationModal");
459    }
```

**The commented-out guard never applied to Payments in the first place.** It is
`CanDeleteEnquiryAsync`, operating on `enquiry.EnquiryId` — a **copy-paste from the Purchase
Enquiry page**, left in place and commented out. Restoring it verbatim would not produce a
correct guard; it would produce a guard on the wrong document. Note also the field name
`DeleteEnqID` surviving in the Payments page as evidence of the same copy-paste.

**Consequence, in domain terms.** A Payment, a Receipt and an Advance Adjustment can be
deleted with no eligibility check whatsoever — in a module (Cash Flow / Accounts) where the
downstream effects are TDS and adjustment records.

**Proposed fix.** A product decision first (**Q-63**), then either specify a real guard or
formally waive it and delete the dead comment. **Estimate 0.5 d** after the decision.
Proposed as task **M0-10e** (§8).

---

## 4. Summary counts

**By verdict (the 79-member `(bool CanDelete, string Message)` family):**

| Verdict | Count |
|---|---|
| **Correct** | **75** |
| **Unreachable guard** (the R-08 pattern) | **0** |
| **Dead computation** | **2** — `EstimateService.cs:735`, `AppointmentLetterService.cs:43` |
| **Stub guard** *(taxonomy extension, §3.3)* | **2** — `StoreInterTransService.cs:211`, `GroupingService.cs:136` |
| **Duplicate condition** | **0** |
| **Wrong key** | **0** |
| **Inconclusive** | **0** |
| **Total** | **79** |

**By reachability (orthogonal to verdict):**

| | Count |
|---|---|
| Reachable from at least one call site | **65** |
| **Unreachable — zero callers** | **14** |
| Total | **79** |

**The wider tuple family (pass 2), 92 guards + 1 delete method:**

| | Count |
|---|---|
| `(bool CanDelete, string Message)` — audited row by row in §2 | 79 |
| `(bool CanItemCancel, string Message)` | 10 — **1 defect**, §3.1 |
| `(bool IsValid, string Message)` | 3 — no defect found by the liveness detector |
| `(bool Success, string Message)` — a delete method, not a guard | 1 |

**Failure mode — fail-closed everywhere. A clean negative result.**

| | Count |
|---|---|
| Of the 64 `CanDelete*` guards: have a `catch` | **64 / 64** |
| …of which **rethrow** (`throw new Exception("Error checking … delete eligibility", ex)`) | **36** |
| …of which **return `(false, …)`** | **28** |
| …of which **return `(true, …)` from a `catch`** — the dangerous fail-open shape | **0** |
| Across all 79: guards with **no** `catch` at all | **2** — both `NeedTocheckRejection` (`LabourSCNService.cs:229`, `PurchaseSCNService.cs:1690`) |

**The fail-open shape does not exist in this family. Confirmed.** A guard that throws
refuses; a guard that catches refuses. Neither ever accidentally permits.

> **Consequence for the API, and it is not neutral.** The **36 rethrowing** guards surface as
> HTTP **500**, not as a refusal. A delete endpoint that does
> `var (ok, msg) = await _svc.CanDeleteXAsync(id);` and maps `!ok` to `409` will map a
> transient database error to `500` and a genuine refusal to `409` — but a *guard-internal*
> failure to `500` as well, so the client cannot distinguish "you may not" from "we broke".
> This is the same ambiguity INV-040 recorded for `InvalidOperationException` carrying both
> meanings. See **Q-60** and Q-34.

---

## 5. Gaps and limits

### 5.1 Delete paths with no guard of any shape — the largest hole

**Severity: High. Confidence: Confirmed for the spot-checked five; Confirmed as a set
membership for all 29.**

Measured 2026-08-21 over `BusinessLayer/BusinessService/`, excluding `IBusinessService/`:

```
public Delete* methods                163  across  89 files
files containing a tuple guard                    61 files
files with a Delete* and no guard of any shape    29 files
```

`comm -23 delete-files.txt guard-files.txt` — the 29:

`AccountsService/AdvaceAdjustmentService.cs` · `AccountsService/PaymentsService.cs` ·
`AccountsService/ReceiptsService.cs` · `InspectionService/DefectInfoService.cs` ·
`InspectionService/FinalInspectionService.cs` ·
`InspectionService/IncomingInspectionService.cs` ·
`InspectionService/MasterInspectionService.cs` · `InventoryService/StockManagerService.cs` ·
`InventoryService/ToolCribIssueService.cs` · `LabourServices/LabourInvoiceService.cs` ·
`LeadService/LeadService.cs` · `MaintenanceService/BreakdownMaintenanceService.cs` ·
`MaintenanceService/CalibrationHistoryAndMaintenanceService.cs` ·
`MaintenanceService/MaintenanceScheduleService.cs` ·
`MasterService/AccountsService/BankService.cs` ·
`MasterService/AdminService/UserAuthorityservice.cs` · `MasterService/CompanyService.cs` ·
`MasterService/InventoryService/HSNService.cs` ·
`OutSourcingService/DebitNote_Service/DebitNoteService.cs` ·
`OutSourcingService/SubContractInvoiceService/SubConInvService.cs` ·
`ProductionService/ProductionIssueAssyService.cs` · `SalesService/CreditNoteService.cs` ·
`SalesService/EnqiryFeasibility/EnqiryFeasibilityService.cs` · `SalesService/ExpInvService.cs` ·
`SalesService/MfgDcService.cs` · `SalesService/MfgInvService.cs` ·
`ServiceBillService/ServiceBillsService.cs` · `SettingsService/ProdLogSettingsService.cs` ·
`SettingsService/RejectionMasterService.cs`

**Five spot-checked as live, reachable UI delete paths (Confirmed, 2026-08-21):**

| Service method | Called from |
|---|---|
| `SalesService/MfgDcService.cs:322 DeleteMfgDcByDcIdAsync` | `MfgDcList.razor:818` |
| `SalesService/MfgInvService.cs:936 DeleteInvoiceByInvIdAsync` | `MfgInvList.razor:1031` |
| `AccountsService/PaymentsService.cs:1310 DeletePaymentAsync` | `PaymentList.razor:418` |
| `AccountsService/ReceiptsService.cs:1341 DeletePaymentAsync` | `ReceiptList.razor:419` |
| `ProductionService/ProductionIssueAssyService.cs:1049 DeleteProdAssyIssueByIssueIdAsync` | `ProductionIssueAssyList.razor:721` |

> Correction to an earlier draft of this finding: Payments and Receipts are **two separate
> implementations**, `PaymentsService.cs:1310` and `ReceiptsService.cs:1341`, both named
> `DeletePaymentAsync`. They are not one method serving two pages.

`InventoryService/StockManagerService.cs` appears in the list but is **owned by a parallel
task** and was read only, never modified.

**Asymmetry worth its own risk row (R-63).** `CanDeleteSalesOrderAsync`
(`MfgPoService.cs:465-565`, BR-SO-001) refuses deletion when a **Sales DC**, a **Tax
Invoice** or an **Export Invoice** exists — yet the Sales DC (`MfgDcService.cs:322`), the Tax
Invoice (`MfgInvService.cs:936`) and the Export Invoice (`ExpInvService`) **can themselves be
deleted with no check at all.** The integrity model guards **upstream only**. Whether that is
deliberate (delete the DC, then the order becomes deletable — a legitimate unwind order) or
an omission **cannot be determined from the code**: see **Q-62**.

### 5.2 Guards are advisory — 77 of 79 run outside the delete transaction

**Severity: High. Confidence: Confirmed. This is the single most important finding for the
API.**

**Only 2 of 79 guards are called inside the delete transaction:**

| Delete method | Transaction opened | Guard called |
|---|---|---|
| `MasterService/HRMasterService/EmployeeService.cs:162 DeleteEmployeeAsync` | `:164 BeginTransactionAsync()` | `:174 await CanDeleteEmployeeAsync(staffId)` |
| `ProductionService/ProductionLogService.cs:275 DeleteProductionLogByLogId` | `:277 BeginTransactionAsync()` | `:288 await CanDeleteProductionLog(logId, screenCode)` |

**The other 77 are advisory only**, invoked from Razor `@code` (67 pages, 68 call sites) plus
one API controller (`V.SMART.Api/Controllers/CurrencyController.cs:101`).

**The house pattern is check-then-act with a user round-trip in the gap.** `HandleDelete(id)`
runs the guard and then calls the JavaScript `ShowConfirmationModal`; a separate
`ConfirmDelete_Click` performs the delete **without re-checking**:

| Page | Guard | Delete, no re-check |
|---|---|---|
| `Pages/Master_Module_pages/Currency_Pages/CurrencyList.razor` | `:597` | `:630` |
| `Pages/Master_Module_pages/Items_Pages/ItemList.razor` | `:855` | `:919` |
| `Pages/SalesAndLabour_pages/SalesPo_Pages/MfgPOList.razor` | `:1083` | `:1118` |

KB-060 recorded this gap for `MfgPOList` only, as a lead for this task. **It is universal.**
The window between guard and delete is not microseconds of scheduling jitter — it is however
long the user takes to read a modal and click *Yes*.

**Consequence for the API.** A delete endpoint that calls the guard and then the delete
method has the same race, but reachable concurrently by any HTTP client rather than by one
user with one browser tab. See **Q-60**: whether guards should move **inside** the delete
transaction is binding on every delete endpoint in M3/M4 and should be decided once, here,
not per endpoint.

### 5.3 Null-handling is split, and it is a behaviour decision for the API

**Confidence: Confirmed as a split; the exact tallies are scanner-dependent, see below.**

When the header row is not found, guards disagree:

| Behaviour on "row not found" | Count (of 79) |
|---|---|
| `return (true, "…can be safely deleted.")` — permissive | **24** |
| `return (false, "…not found.")` — refusing | **35** |
| No `== null` test on a header row | **20** |

Measured by a scanner that takes the **first** `== null` test in each body and the polarity of
the return within the following six lines; the result is stable at a 3-line and a 6-line
window. A handover draft of this audit reported **26 / 35**; the refusing count agrees exactly
and the permissive count differs by two, which is a measurement-window artefact, not a
disagreement about the finding. **Both conventions are live.** The number to trust for a
specific guard is its row in §2, not either tally.

`CanDeleteSalesOrderAsync` is on the permissive side —
`MfgPoService.cs:474-475`: `if (po == null) return (true, "Sales Order can be safely deleted.");`

**Consequence for the API.** `GET /api/v1/<doc>/{id}/can-delete` must pick one convention
deliberately, because it changes behaviour for a **concurrently deleted** row: permissive
answers *"yes, go ahead"* for a row that no longer exists; refusing answers *"not found"*. See
**Q-61**.

#### 5.3a The 20-guard bucket is not homogeneous — two of them throw on a missing row

**Confidence: Confirmed. All five re-read individually, 2026-08-21.**

An earlier draft described the 20-guard bucket as *"queries downstream tables directly"*. That
is true of **15** of them and **wrong for five**, which do load a header row. Those five were
re-read one by one:

| Guard | Loads | Null-safe? | Evidence |
|---|---|---|---|
| `PurchaseInvoiceService.cs:1283 CanDeletePurchaseInvAsync` | `invoice` at `:1287-1290` | **NO** | dereferenced at `:1292` (`invoice.Balance != invoice.GrandTotal`) and `:1295`, with no null test anywhere in the body |
| `PlanningService/RcReleaseService.cs:803 CanDeleteRcReleaseAsync` | `rcRelease` at `:807` | **NO** | dereferenced at `:818` (`rcRelease.IsCancel \|\| rcRelease.ShortClose`), no null test anywhere in the body |
| `LabourServices/LabourSCNService.cs:229 NeedTocheckRejection` | `grnSub` at `:231-233` | yes | guarded by `if (grnSub != null)` at `:235`, and by `poSub != null` at `:241`, `po != null` at `:247` — positive-form tests, which is why the `== null` scanner did not see them |
| `OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1690 NeedTocheckRejection` | `grnSub` at `:1692-1694` | yes | same shape: `:1696`, `:1702`, `:1708` |
| `ProductionService/ProductionReturnCompService.cs:4763 CanRemoveProductionReturnAsync` | **not a header row** — a `List<int>` at `:4767-4771` | yes | `.Any()` at `:4773` is null-safe on a materialised list; the later `sums` is explicitly tested `sums != null` at `:4795` |

**So the dereference-before-null-check class has three members, not one**
(`AppointmentLetterService.cs:52`, §3.3(d)) **— `PurchaseInvoiceService.cs:1292` and
`RcReleaseService.cs:818` are the other two.** All three throw `NullReferenceException` on a
row that does not exist. All three catch it and rethrow, so all three surface as HTTP **500**
from a delete endpoint rather than as a decision — which is precisely the §5.3 / **Q-61**
concern, arrived at by a different route. Recorded as **R-62** in
[KB-060](technical-debt-register.md), widened from one guard to three.

**Their §2 verdicts are left `Correct` and that is deliberate.** The §2 verdict column answers
the task's question — *"is every computed boolean tested, and does each `if` test the variable
computed above it?"* — and for these two the answer is genuinely yes. Null safety is an
orthogonal axis, recorded here and in R-62 rather than by overloading a taxonomy the task
defined. The §2 rows for both guards now cross-reference this section.

**Scope limit, stated plainly.** The three-member census above covers the **five header-row
loaders inside the 20-guard "no `== null` test" bucket**. It was **not** extended to the 59
guards that do have a null test, because a guard that tests for null before use is null-safe
by construction on that row — but it was also not extended to *second* and *third* entities
loaded inside a body that null-checks only its first (`AppointmentLetterService.cs:56`
dereferences `candidate`, itself loaded at `:52` and never tested, is one such). **That
sweep was not performed**; it is a named gap, folded into **M0-10c** (§8).

### 5.4 Inconclusive methods

**None.** Every one of the 79 was readable to a verdict. Where the *intent* could not be
determined — the four inert guards of §3.3, three of which can never refuse at all; and the
upstream-only asymmetry (§5.1) — the code is not
ambiguous; the **business rule behind it** is unknown, and that is recorded as an open
question (Q-62, Q-64) rather than as an `Inconclusive` verdict. Marking those `Inconclusive`
would imply someone should re-read the code, and they should not: they should ask the owner.

### 5.5 Stored procedures — a bounded negative, not a complete one

**Confidence: Partial / Unknown, deliberately.**

| Measurement | Result |
|---|---|
| `.sql` files under `db/stored-procedures/` | **78** |
| `.sql` files under `Existing Store Procedures/` | **0** |
| …of the 78, containing a `DELETE` statement | **1** — `Sp_GetItemModificationReport.sql`, a report procedure |

So **no delete-eligibility logic exists in any committed stored procedure**. But **R-04**
records that a large share of the deployed procedures have **no DDL in source control**
([KB-102](../architecture/stored-procedure-inventory.md)), so this negative covers only what is
committed. **Named gap: delete logic implemented in procedures that exist only in the tenant
databases was not examined, and cannot be from this repository.** Settling it needs a live
database (Q-14's tooling from KB-103 would serve).

### 5.6 Other findings, recorded so they are not rediscovered

- **`ProductionReturnAssyService.cs:378 CanDeleteProductionReturnAssyAsync` is the only guard
  that calls another guard** — `ValidateDeleteAsync` at `:428`, testing `result.IsValid`. It
  is also the only guard whose "not found" is `(false, "Production Return Assy not found.")`
  while its immediate siblings say `(true, …)`. Verdict **Correct**; noted because it is the
  one guard whose control flow is not flat.
- **Wrong method name in a log string.**
  `MasterService/HRMasterService/CandidateService.cs:57` logs
  `"Error in CanDeleteAsync for CandidateID: {candidateID}"` from inside
  `CanDeleteCandidateAsync`. Harmless at runtime; misleading when grepping logs.
  `AppointmentLetterService.cs:75` has the same class of defect, logging
  `"Error in CanDeleteSalesOrderAsync"`.
- **Duplicate-name guards that are *not* defects.** `CanDeleteAssydefAsync` ×2
  (`AssemblyDefService.cs:1504`, `AssemblyDefLabourService.cs:1617`) and `CanDeleteMINAsync`
  ×2 (`MINService.cs:286`, `StockIssueRequestService.cs:319`) are each reached by their own
  page through their own interface. Both pairs are genuinely distinct guards on distinct
  documents that happen to share a method name.

---

## 6. What this means for the delete endpoints

Every `DELETE` endpoint written in M3 and M4 inherits this surface. The four decisions that
must be made **once**, not per endpoint:

1. **Does the guard run inside the delete transaction?** Today 2 of 79 do (§5.2). **Q-60.**
2. **What does "row not found" answer?** Today it is 24 permissive / 35 refusing (§5.3).
   **Q-61.**
3. **Is the upstream-only integrity model intended?** (§5.1). **Q-62.**
4. **Do the three commented-out Cash Flow guards get restored or formally waived?** (§3.4).
   **Q-63.**

And one that is not a decision but a constraint: **36 of 64 guards rethrow on internal error**
(§4), so a delete endpoint cannot treat "guard threw" as "guard refused" without turning
transient faults into business refusals.

---

## 7. Confidence, and the empirical verification

**The surviving R-08 defect (§3.1) was verified empirically. The task's acceptance criterion
*"at least one defect verified empirically, with the observed output quoted"* is MET.**

> **Attempt 1 of this task claimed the opposite, on a false premise.** It recorded the
> criterion NOT MET because *"no test project exists on this branch"*. That statement was
> **wrong**: `tests/V.SMART.Shared.Tests/` has existed since `9557de2` (M0-12-01) and
> `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs` since `8e3b19d`
> (M0-09) — the very file §3.1 already cited as the shape to model a proving test on. The
> document contradicted itself. Corrected here and in the § Execution Record.

**The test.** `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs`,
`CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused`. It seeds one Sales Order with one
line, adds a `ContractReview` against the Sales Order **header** id — the key
`MfgPoService.cs:613` joins on — and deliberately adds **no** Route Card, so `hasRc` is
`false` and the guard at `:614` cannot fire on the wrong document's evidence. It then calls
`CanSalesOrderItemCancelCheckAsync` and asserts the refusal the guard's own `Message` string
says it performs. It reuses `SalesOrderDeleteGuardHarness`, the harness M0-09 built.

**Observed output**, run un-skipped against **unmodified** `MfgPoService.cs` on branch
`migration/M0-10-candelete-guard-audit`, 2026-08-21, quoted verbatim:

```text
[xUnit.net 00:00:04.94]     V.SMART.Shared.Tests.Services.MfgPoServiceDeleteGuardTests.CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused [FAIL]
  Failed V.SMART.Shared.Tests.Services.MfgPoServiceDeleteGuardTests.CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused [4 s]
  Error Message:
   Assert.Equal() Failure: Values differ
Expected: Tuple (False, "Cannot cancel this Item as a Contract Review trans"···)
Actual:   Tuple (True, "Item can be safely Cancell.")

Failed!  - Failed:     1, Passed:    84, Skipped:     0, Total:    85, Duration: 8 s
```

Baseline immediately before, same command, same branch:
`Passed!  - Failed:     0, Passed:    84, Skipped:     0, Total:    84, Duration: 9 s`.

**`Actual: Tuple (True, "Item can be safely Cancell.")` is the defect, observed.** A Sales
Order line whose only downstream document is a Contract Review is reported cancellable. The
surviving R-08 instance is therefore **Confirmed as to runtime effect**, not merely Confirmed
by reading.

**The test is left `Skip`-ped, not deleted** — M0-10 step 8 permits either. Reason recorded in
the file and in its `Skip` string: M0-10 is an audit and must not repair `MfgPoService.cs:614`
(that is **M0-10a**), so the test cannot be left enabled without turning CI red; and it is
kept rather than deleted because M0-10a needs exactly this test — fix `:614`, remove the
`Skip`, and it must go green. Deleting it would force M0-10a to re-derive the seeding
requirements already recorded on `SalesOrderDeleteGuardHarness`. With the `Skip` in place the
suite is green:
`Passed!  - Failed:     0, Passed:    84, Skipped:     1, Total:    85, Duration: 8 s`.

**No production code was changed to obtain this.** `git diff --stat -- V.SMART/` is empty; the
only changed file outside `docs/` is the test file, which the task's *Files That Must Not
Change* section explicitly exempts for exactly this purpose.

The verdicts in §2 are **Confirmed** in the sense KB-002 defines: each is traced to a
`file:line` and derived from a mechanical transcription of the method body, not from a reading
of variable names. They remain Confirmed-by-reading; only §3.1 carries a runtime observation.

---

## 8. Proposed follow-up tasks

**None of these were created.** They are proposals for the owner to schedule; ids are
suggestions.

| Id | Scope | Estimate | Priority |
|---|---|---|---|
| **M0-10a** | Fix the surviving R-08 instance — `MfgPoService.cs:614`, `hasRc` → `hasCR`. The proving test already exists and was already observed red (§3.1, §7): remove the `Skip` from `CanSalesOrderItemCancel_WithOnlyContractReview_IsRefused` in `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs` and it must go green. One identifier plus one attribute; nothing else in the diff. | 0.5 d | **P1** |
| **M0-10b** | Resolve the 14 unreachable guards (§3.2): wire each to its delete path or delete it. The two duplicate clones (`RouteCardService.cs:1840`, `ProductionLogService.cs:635`) should simply go; record the genuine orphans as intentional if that is the answer. | 1 d | P2 |
| **M0-10c** | Decide the null-handling convention (Q-61) and apply it across all 79, including the three dereference-before-null-check guards in §5.3a (`AppointmentLetterService.cs:52`, `PurchaseInvoiceService.cs:1292`, `RcReleaseService.cs:818`) and the four wrong-document message strings in §3.3. **Also completes the sweep §5.3a names as not performed**: second and subsequent entities loaded inside bodies that null-check only their first. Behaviour-affecting: needs the decision first. | 1.5 d | P2 |
| **M0-10d** | Specify guards for the 29 unguarded delete paths (§5.1). **Analysis first — rule discovery, not coding.** Also covers the four stub guards (§3.3), which are the same question asked about documents that happen to have a guard-shaped hole already. | 3–5 d | **P1** |
| **M0-10e** | Restore or formally waive the three commented-out Cash Flow guards (§3.4). Blocked on Q-63; note the commented code is a copy-paste from the Enquiry page and must not be restored verbatim. | 0.5 d | P2 |

---

## § Execution Record

| | |
|---|---|
| Task | **M0-10** — Audit all `CanDelete…` guards (INV-025) |
| Branch | `migration/M0-10-candelete-guard-audit` |
| Date | 2026-08-21 |
| Source changes | **None under `V.SMART/`.** `git diff --stat -- V.SMART/` is empty; every defect found is recorded here, not repaired. The one non-documentation change is the `Skip`-ped proving test appended to `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs`, which the task's *Files That Must Not Change* section explicitly exempts. |
| Build / test run | **`dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` was run**, three times. Baseline before the proving test: `Passed!  - Failed: 0, Passed: 84, Skipped: 0, Total: 84`. With the proving test enabled, against unmodified `MfgPoService.cs`: `Failed!  - Failed: 1, Passed: 84, Skipped: 0, Total: 85` — the defect, observed; output quoted in §3.1 and §7. With the test `Skip`-ped for commit: `Passed!  - Failed: 0, Passed: 84, Skipped: 1, Total: 85`. No `dotnet build` was run: the task changes no file under `V.SMART/`, and `dotnet test` compiled `V.SMART.Shared` as a project reference anyway. |
| Verification actually performed | Static analysis **plus one executed test**. Static: the six greps in §1.1; two independent `awk` scanners over all 93 tuple-family method bodies (declaration-liveness, §1.3; catch-behaviour and null-polarity, §4/§5.3); a per-name call-site census over all 71 guard names with injected-interface attribution; and manual re-reading of every method cited by line number in §3 and §5. Dynamic: the §3.1 proving test. |
| Detector calibration | The liveness detector was run against `git show 8e3b19d^` (pre-M0-09) and correctly reported the two known BR-SO-002 defects, before being run against `HEAD`. |
| Ids claimed | **KB-061** (this document — verified free in [KB-005](../INDEX.md) before claiming); **R-60 … R-64** in [KB-060](technical-debt-register.md); **Q-60 … Q-64** in [KB-004](../open-questions.md). No new `INV` id: this task **is** INV-025. |
| Acceptance criteria | **All met.** Criterion 9 (*"at least one defect verified empirically"*) is met by §3.1/§7. See the attempt-2 note below for the correction that made it so. |

### Attempt 2 — 2026-08-21

Attempt 1 (commit `10926ea`) failed validation as `implementation-error`. The audit body was
validated as sound; six corrections were required and are recorded here.

| # | Correction | Where |
|---|---|---|
| 1 | **The proving test was written, run and observed.** Attempt 1 asserted *"no test project exists on this branch"* and marked criterion 9 NOT MET. That was **factually wrong** — `tests/V.SMART.Shared.Tests/` has existed since `9557de2` (M0-12-01) and `Services/MfgPoServiceDeleteGuardTests.cs` since `8e3b19d` (M0-09), the very file §3.1 already cited as the shape to model a proving test on. The document contradicted itself. The escape clause in M0-10 step 8 — *"if the test project does not exist, mark the finding Inferred"* — therefore did not apply. | §3.1, §7, and the `Build / test run` row above |
| 2 | The false claim about the test project's absence was removed and replaced with the observed run summaries. | `Build / test run` row above |
| 3 | Criterion 9 flipped **NOT MET → MET**, only after the test had actually run and its output been quoted. | §7; [KB-080 §7](../execution/README.md#7--m0--stabilise) |
| 4 | [KB-005](../INDEX.md)'s `Q-nn` collision-register row was **malformed** — a 4-cell row under a 3-column header, so GFM silently dropped the `Q-60…Q-64 claimed by M0-10` allocation note on render. In a cross-branch collision register an invisible allocation is exactly where a double-claim happens. Fixed to 3 cells; the adjacent `R-nn` row's lost trailing `\|` restored. | KB-005 |
| 5 | **R-61's severity was self-contradictory** — [KB-060](technical-debt-register.md) filed it under `## High` while §3.2 called it *Medium*. Reconciled to **Medium**, and the row moved into the `## Medium` section so placement and text agree. | KB-060, §3.2 |
| 6 | The **dereference-before-null-check** class was recorded for `AppointmentLetterService.cs:52` (R-62) but not for its structural siblings. The five header-loading guards in the "no null check" bucket were re-read individually; **two more** are genuinely unsafe and **three** are not. Recorded in full, and §5.3's description of that bucket corrected. | §5.3, §3.3(d), R-62 |

Three smaller corrections were made at the same time: the headline block's *"guards that can
never refuse (4)"* now says **three** (`AppointmentLetterService.cs:58` **does** refuse on a
`Staff`-name match, as §3.3(d) already said); the §2 rows for
`LabourSCNService.cs:1638` and `PurchaseInvoiceService.cs:1283` had transcription gaps in
their *Computed*/*Tested* columns and are filled in; and §3.1's claim about *"scoping guard
work by return shape"* is tightened to name the **pass-2** `(bool <any>, string Message)`
family, which is what actually caught the defect, rather than the pass-1 shape that defines
the §2 inventory.

**No verdict in §2 changed, and no count in §4 changed.** The corrections affect prose,
severity placement, one table's transcription columns, and the empirical status of §3.1.
