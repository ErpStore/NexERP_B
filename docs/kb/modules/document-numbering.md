---
doc_id: KB-100
title: "Document Numbering and Financial-Year Suffixes (As-Is)"
module: modules
status: active
produced_by: M2-B12-01 (INV-012)
source_files:
  - V.SMART/V.SMART.Shared/Services/FinancialYearHelper.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/CommonService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgDcService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgInvService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/ExpInvService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/PerformaInvService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/ContractReviewService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourInvoiceService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourDcOutgoingService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractDcOutService/SubConDcOutService.cs
  - V.SMART/V.SMART.Shared/Repository/DcRunningNoRep/DcRunningNumberRepository.cs
  - V.SMART/V.SMART.Shared/Repository/InvoiceAutoRunnNo/InvoiceAutoRunningNumberRepository.cs
  - V.SMART/V.SMART.Shared/Repository/ProductionRepository/ProductionIssueWOAssyRepo/ProductionIssueAssyRepository.cs
  - V.SMART/V.SMART.Shared/Repository/HumanResourceRepository/EmployeeLoanRepository/StaffLoanRepository.cs
  - V.SMART/V.SMART.Shared/Repository/HumanResourceRepository/AttendanceRepository/AttendanceRepository.cs
  - V.SMART/V.SMART.Shared/Repository/UnitOfWork.cs
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
entities: [DcRunningNumber, InvoiceAutoRunningNumber, MfgDc, MfgInv, ExpInv, LabInv, MfgQuote, MfgPo, ContractReview, ProductionIssueAssy, StaffLoan, Attendance]
database_tables: [DcRunningNumbers, InvoiceAutoRunningNumbers, MfgDc, MfgInv, ExpInv, LabInv, MfgQuote, MfgPo, ContractReview, ProductionIssueAssy]
business_rules: [BR-DOC-001, BR-DOC-002, BR-DOC-003, BR-DOC-004, BR-DOC-005, BR-DOC-006, BR-DOC-007, BR-DOC-008, BR-DOC-009, BR-DOC-010]
confidence: mixed
last_verified: 2026-08-20
---

# Document Numbering and Financial-Year Suffixes (As-Is)

> **Document numbers appear on statutory documents.** Invoices, delivery challans and e-way
> bills carry them; customers quote them; auditors reconcile on them. **Changing their shape
> is a compliance change, not a refactor.** Section 4 (the format catalogue) exists so that
> any remedy can be *proven* format-preserving.

This is the output of **INV-012**, run by task
[`M2-B12-01`](../execution/tasks/M2-B12-01.md) on **2026-08-20**. It is the input to
[`M2-B12-02`](../execution/tasks/M2-B12-02.md) (live-database duplicate census — see
**§9**) and to [`M2-B12-03`](../execution/tasks/M2-B12-03.md) (the remedy).

Every claim below is labelled **Confirmed** (traced to `file:line` in the working tree),
**Inferred** (reasoned, reasoning shown) or **Unknown** (recorded in
[KB-004](../open-questions.md)), per [KB-002](../source-of-truth-rules.md).

---

## 1. Scope and method

**Question.** How does V.SMART allocate a document number, what exactly does the resulting
string look like, and what would break if that changed?

**Method.** Source-only. **No database was accessed** — that is deliberate and is
[`M2-B12-02`](../execution/tasks/M2-B12-02.md)'s job. Everything here is derived from the
working tree on branch `migration/M2-B12-01-inv-012-numbering`, cut from `master`.

**Commands run and their observed output (2026-08-20).** These are the counts the rest of
the document reconciles against.

| Command (from the repository root) | Observed |
|---|---|
| `git grep --untracked -ic "TOP 1" -- V.SMART/V.SMART.Shared/Repository/` | **36 files**, **38 occurrences** |
| `git grep --untracked -n "UPDLOCK" -- V.SMART/V.SMART.Shared/` | **37 lines** |
| `git grep --untracked -nE "HOLDLOCK\|sp_getapplock\|IsolationLevel\|Serializable" -- V.SMART/` | **0** |
| `git grep --untracked -nE "CREATE SEQUENCE\|HasSequence" -- V.SMART/` | **0** |
| `git grep -n "IsUnique" -- V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs` | **3** (`:582`, `:595`, `:618`) |
| `git grep --untracked -c "IsDuplicate.*Async" -- V.SMART/` | **59 files**, **81 occurrences** |
| `git grep --untracked -c "GetFinancialYearSuffix" -- V.SMART/` | **63 files**, **77 occurrences** |

**Reconciliation against the task file's tables (`M2-B12-01.md`).** All four mandated counts
reproduced **exactly**; every Mechanism A method-declaration line in the task file's table
resolved unchanged. Three differences were found and are **not** silently adopted:

| Task file says | Observed 2026-08-20 | Note |
|---|---|---|
| 62 `IsDuplicate*Async` occurrences across 41 files | **81 across 59** | Task file figure is **stale**. This document uses 81/59. |
| `ApplicationDbContext.cs:579-582` is the unique index | Comment at `:579`, statement at **`:580-582`** | Minor drift. Cited here as `:579-582` (comment + statement). |
| `AttendanceRepository.GetLastIdAsync` at `:41-49` | Declaration `:41`, body ends **`:48`** | Minor drift. |

Two things the task file does **not** contain were found and are recorded here as
first-class results: a **second discriminator** (`Company.BookTypeInvoice`, §6) and the fact
that **7 of the 38** raw-SQL sites are **dead code** (§3.4).

---

## 2. Mechanism taxonomy

There is no single numbering service. Three independent mechanisms coexist, and the
mechanism is what determines the remedy — which is why this document groups by mechanism
rather than by module.

| | Mechanism | Shape | Sites |
|---|---|---|---|
| **A** | **Raw-SQL last-number read** | `FromSqlRaw` / `Database.SqlQuery<string>` issuing `SELECT TOP 1 … FROM <T> WITH (UPDLOCK, ROWLOCK) WHERE Suffix = {0} ORDER BY TRY_CAST(<col> AS INT) DESC`, then `+1` in C# | **38** sites in **36** repository files |
| **B** | **Lock-free LINQ last-number read** | `OrderByDescending(…).Select(…).FirstOrDefaultAsync()`, then `+1` or a regex increment in C#. No hint of any kind | **6** methods (2 repositories, 3 services) |
| **C** | **Allocation-table read-modify-write** | Read a row from `DcRunningNumbers` / `InvoiceAutoRunningNumbers`, mutate `LastNumber`, `SaveAsync()`. Plain EF, no hint | 4 methods in `CommonService` + inline copies in 4 document services |

**Confirmed.** All three are live simultaneously. Mechanism C is the *newer* one: the DC and
invoice families migrated from A to C and left their A methods behind as dead code (§3.4).

**Confirmed (negative result).** There is **no fourth mechanism**. Zero matches across
`V.SMART/` for `CREATE SEQUENCE`, `HasSequence`, `sp_getapplock`, `HOLDLOCK`,
`IsolationLevel` and `Serializable`. There is no database sequence, no application lock and
no serializable transaction anywhere in the solution. This is the finding that rules out
"it must be handled somewhere else".

---

## 3. Call-site inventory

### 3.1 Mechanism A — raw-SQL `SELECT TOP 1 … ORDER BY TRY_CAST(<col> AS INT) DESC`

All paths are relative to `V.SMART/V.SMART.Shared/Repository/`. **Decl** is the method
declaration line; **SQL** is the `SELECT TOP 1` line. All **Confirmed, 2026-08-20**.
`Live?` is the count of non-commented call sites outside `Repository/` (method in §3.4);
where several repositories share a method name the count is per *name*, not per site.

| # | File | Method | Decl | SQL | Table | Number col | Scope (WHERE) | Hint | Live? |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `SalesAndLabourRepository/SalesDCRepository/MfgDcRepository.cs` | `GetLastDcNoAsync` | 29 | 33 | `MfgDc` | `DcNo` | `suffix` | `UPDLOCK, ROWLOCK` | **0 — dead** |
| 2 | `SalesAndLabourRepository/PerformaInvoiceRepository/PerformaInvRepository.cs` | `GetLastPerformaInvNoAsync` | 32 | 36 | `PerformaInv` | `InvNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 3 | `SalesAndLabourRepository/MfgQuotation/MfgQuoteRepository.cs` | `GetLastQuoteNoAsync` | 29 | 34 | `MfgQuote` | `QuoteNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 4 | `SalesAndLabourRepository/MfgInvoice/MfgInvRepository.cs` | `GetLastInvNoAsync` | 30 | 35 | `MfgInv` | `InvNo` | `Suffix` | `UPDLOCK, ROWLOCK` | **0 — dead** |
| 5 | `SalesAndLabourRepository/LabourSCN_Repository/LabourSCNRepository.cs` | `GetLastSCNNoAsync` | 31 | 36 | `LabourSCN` | `SCNNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 6 (name shared by 5 repositories) |
| 6 | `SalesAndLabourRepository/LabourInvoice_Repository/LabInvRepository.cs` | `GetLastLabInvoiceNoAsync` | 29 | 36 | `LabInv` | `LabInvNo` | `Suffix` | `UPDLOCK, ROWLOCK` — **column-list variant** (`SELECT TOP 1 LabInvNo`) | **0 — dead** |
| 7 | `SalesAndLabourRepository/LabourGRN_Repository/LabourGRNRepository.cs` | `GetLastGRNNoAsync` | 30 | 35 | `LabourGRN` | `GRNNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 3 (name shared by 3 repositories) |
| 8 | `SalesAndLabourRepository/LabourDcOutgoing_Repository/LabourDcOutgoingRepository.cs` | `GetLastDcNoAsync` | 26 | 31 | `LabourDcOutgoing` | `DcNo` | `Suffix` **and `NonReturnDc = 0`** | `UPDLOCK, ROWLOCK` | **0 — dead** |
| 9 | `SalesAndLabourRepository/LabourDcOutgoing_Repository/LabourDcOutgoingRepository.cs` | `GetNextNrDcNoAsync` | 59 | 63 | `LabourDcOutgoing` | `DcNo` | **`NonReturnDc = 1` and `DcNo LIKE 'NR%'` — the `suffix` parameter is passed but never referenced in the SQL** | `UPDLOCK, ROWLOCK` | 1 |
| 10 | `SalesAndLabourRepository/ExportInvoiceRepository/ExpInvRepository.cs` | `GetLastExpInvNoAsync` | 31 | 36 | `ExpInv` | `ExpInvNo` | `Suffix` | `UPDLOCK, ROWLOCK` | **0 — dead** |
| 11 | `SalesAndLabourRepository/CreditNote_Repository/CreditNoteRepository.cs` | `GetLastCreditNoAsync` | 30 | 35 | `CreditNote` | `CreditNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 12 | `ProductionRepository/ProuctionCompRepo/ProductionSCNCompRepository.cs` | `GetLastSCNNoAsync` | 26 | 31 | `ProductionSCNComp` | `SCNNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 6 (shared name) |
| 13 | `ProductionRepository/ProuctionCompRepo/ProductionReturnCompRepository.cs` | `GetLastReturnNoAsync` | 24 | 27 | `ProductionReturnComp` | `ReturnNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 3 (shared name) |
| 14 | `ProductionRepository/ProuctionCompRepo/ProductionIssueCompRepository.cs` | `GetLastIssueNoAsync` | 27 | 31 | `ProductionIssueComp` | `IssueNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 2 (shared name) |
| 15 | `ProductionRepository/ProductionSCNAssyRepo/ProductionSCNAssyRepository.cs` | `GetLastSCNNoAsync` | 24 | 29 | `ProductionSCNAssy` | `SCNNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 6 (shared name) |
| 16 | `ProductionRepository/ProductionReturnAssyRepo/ProductionReturnAssyRepository.cs` | `GetLastReturnNoAsync` | 25 | 29 | `ProductionReturnAssy` | `ReturnNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 3 (shared name) |
| 17 | `ProductionRepository/ProductionLogRepo/ProductionLogRepository.cs` | `GetLastLogNoAsync` | 27 | 30 | `ProductionLog` | `LogNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 18 | `ProductionRepository/ProductionIssueWOAssyRepo/ProductionIssueAssyRepository.cs` | `GetLastIssueNoAsync` | 28 | 32 | `ProductionIssueAssy` | `IssueNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 2 (shared name) |
| 19 | `ProductionRepository/ProductionIssueWOAssyRepo/ProductionIssueAssyRepository.cs` | `GetMonthWiseProductionIssueNumberAsync` | 49 | 75 | `ProductionIssueAssy` | `IssueNo` | **`DepartmentCode`, `MonthCode`, `Suffix`** | **NONE — the only unhinted raw-SQL site** | 2 |
| 20 | `PlanningRepository/RouteCardRepo/RouteCardRepository.cs` | `GetLastRcNoAsync` | 26 | 30 | `RouteCard` | `RCNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 21 | `PlanningRepository/RouteCardRepo/RcReleaseRepository.cs` | `GetLastRcReleaseNoAsync` | 24 | 28 | `RouteCardRelease` | `RcReleaseNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 22 | `PlanningRepository/JobOrderRepo/JobOrderRepository.cs` | `GetLastJobNoAsync` | 25 | 29 | `JobOrder` | `JobNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 23 | `PlanningRepository/EstimationRepository/EstimateRepository.cs` | `GetLastEstimateNoAsync` | 25 | 30 | `Estimate` | `EstiamateNo` *(sic — misspelled in the schema)* | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 24 | `OutSourcingRepository/SubContractSCNRepository/SubConSCNRepository.cs` | `GetLastSCNNoAsync` | 24 | 29 | `SubConSCN` | `SCNNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 6 (shared name) |
| 25 | `OutSourcingRepository/SubContractGRNRepository/SubConGRNRepository.cs` | `GetLastGRNNoAsync` | 20 | 23 | `SubConGRN` | `GRNNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 3 (shared name) |
| 26 | `OutSourcingRepository/SubContractDcOutRepository/SubConDcOutRepository.cs` | `GetLastDcNoAsync` | 25 | 29 | `SubConDcOut` | `DcNo` | `Suffix` | `UPDLOCK, ROWLOCK` | **0 — dead** |
| 27 | `OutSourcingRepository/PurchOrSubConPORepository/PurchPoRepository.cs` | `GetLastPONoAsync` | 27 | 31 | `PurchPo` | `PONo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 28 | `OutSourcingRepository/PurchOrSubConEnquiryRepository/EnquiryPurchaseRepository.cs` | `GetLastEnqNoAsync` | 27 | 31 | `EnquiryPurchase` | `EnquiryNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 29 | `OutSourcingRepository/PurchaseSCN_Repository/PurchaseSCNRepository.cs` | `GetLastSCNNoAsync` | 33 | 37 | `PurchaseSCN` | `SCNNo` | `suffix` | `UPDLOCK, ROWLOCK` | 6 (shared name) |
| 30 | `OutSourcingRepository/PurchaseGRN_Repository/PurchaseGRNRepository.cs` | `GetLastGRNNoAsync` | 31 | 35 | `PurchaseGRN` | `GRNNo` | `suffix` | `UPDLOCK, ROWLOCK` | 3 (shared name) |
| 31 | `OutSourcingRepository/MaterialRequsiationRepo/MaterialReqRepository.cs` | `GetLastMReqNoAsync` | 24 | 28 | `MaterialReq` | `MReqNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 4 |
| 32 | `OutSourcingRepository/DebitNote_Repository/DebitNoteRepository.cs` | `GetLastDebitNoAsync` | 30 | 35 | `DebitNote` | `DebitNo` | `Suffix` | `UPDLOCK, ROWLOCK` — carries the **misleading comment at `:32`** | 1 |
| 33 | `InventoryStockRepository/ToolCribReturnRepo/ToolCribReturnRepository.cs` | `GetLastReturnNoAsync` | 29 | 35 | `ToolCribReturns` | `TCReturnNo` | `Suffix` | `UPDLOCK, ROWLOCK` — **`ORDER BY` strips a trailing `/…` before casting** (`:38`) | 3 (shared name) |
| 34 | `InventoryStockRepository/TooCribIssueRepo/ToolCribIssueRepository.cs` | `GetLastToolCribIssueNoAsync` | 30 | 34 | `ToolCribIssue` | `TCIssueNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 35 | `InventoryStockRepository/StoreInterTransferRepository/StoreInterTransRepository.cs` | `GetLastISTNoAsync` | 28 | 32 | `StoreInterTrans` | `ISTNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 36 | `InventoryStockRepository/StockRequestIssueRepository/StockRequestIssueRepository.cs` | `GetLastReqNoAsync` | 25 | 29 | `StockIssueRequest` | `IssueNo` | `Suffix` | `UPDLOCK, ROWLOCK` | **0 — dead** |
| 37 | `InventoryStockRepository/StockAdditionRepository/SCNGenRepository.cs` | `GetLastSCNGenNoAsync` | 27 | 32 | `SCNGen` | `SCNGenNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 1 |
| 38 | `InventoryStockRepository/MaterialIssueNoteRepository/MaterialIssNoteRepository.cs` | `GetLastMINNoAsync` | 25 | 29 | `MaterialIssNote` | `IssueNo` | `Suffix` | `UPDLOCK, ROWLOCK` | 2 |

**38 sites, 36 files** — rows 8/9 and 18/19 share a file. **37 of 38 carry the hint**; row 19
is the sole exception. All are scoped by `Suffix` except row 9 (which ignores its `suffix`
parameter entirely) and row 19 (three scope columns).

**Enclosing transaction.** *Confirmed:* the repository method is **never** transactional — it
issues one statement and returns. Whether the read+insert pair is transactional is decided by
the *calling service*. Two shapes were observed and both matter:

- `MfgDcService.UpsertDCAsync` opens one — `using var transaction = await _unitOfWork.BeginTransactionAsync();`
  (`MfgDcService.cs:802`) — wrapping the allocation and the insert. So do
  `MfgInvService.cs:425`, `ExpInvService.cs:1183` and `LabourInvoiceService.cs:246`.
- But `UnitOfWork.BeginTransactionAsync` (`Repository/UnitOfWork.cs:798-801`) is
  `return await _db.Database.BeginTransactionAsync();` with **no `IsolationLevel` argument**
  — i.e. **READ COMMITTED**. See §8: transactional is not the same as protected.

**Do not** write that these allocations are never inside a transaction. For the DC and
invoice families they are. What they lack is a *range* lock, not a transaction.

### 3.2 Mechanism B — lock-free LINQ

| File | Method | Lines | Reads | Scope | Notes |
|---|---|---|---|---|---|
| `Repository/HumanResourceRepository/EmployeeLoanRepository/StaffLoanRepository.cs` | `GetLastLoanNoAsync` | 31-47 | `StaffLoan.LoanNo`, ordered by `LoanId` | **none at all — no suffix, no year** | Returns `lastNumber + 1`, else `1` |
| `Repository/HumanResourceRepository/AttendanceRepository/AttendanceRepository.cs` | `GetLastIdAsync` | 41-48 | `Attendance.Id`, ordered by `Id` | none | Returns `lastId + 1`, or `1` when `lastId == 0` |
| `BusinessLayer/BusinessService/SalesService/MfgPoService.cs` | `GetNextSaleOrderNoAsync` | 1623-1660 | `MfgPo.SaleOrderNo`, ordered by `PoId` | **`PoTypeId` only — not financial-year scoped** | See §4.3; **fails open** |
| `BusinessLayer/BusinessService/SalesService/MfgPoService.cs` | `GetNextOANumberAsync` | 1699-1727 | `MfgPo.OANo`, ordered by `PoId` | `Suffix == suffix` | Returns `$"{nextNumber}{suffix}"` |
| `BusinessLayer/BusinessService/SalesService/PerformaInvService.cs` | `GetNextOANumberAsync` | 1072-1100 | **`MfgPo.OANo`** — its own table is not read | `Suffix == suffix` | **Byte-identical** to the above |
| `BusinessLayer/BusinessService/SalesService/ContractReviewService.cs` | `GetNextOANumberAsync` | 761-789 | **`ContractReview.OANo`**, ordered by `Id` | **`OANo.EndsWith(suffix)`** | **Materially different** — see §4.4 |

**Answering the task's question — are the three `GetNextOANumberAsync` byte-identical?**
**No: two are, one is not.** *Confirmed.* `MfgPoService.cs:1699-1727` and
`PerformaInvService.cs:1072-1100` are byte-identical, including the fact that
**`PerformaInvService` reads `_unitOfWork.MfgPos`** — the PO table, not its own.
`ContractReviewService.cs:761-789` differs in four material ways: it reads
`_unitOfWork.ContractReviews`, scopes with `x.OANo.EndsWith(suffix)` instead of
`q.Suffix == suffix`, orders by `x.Id`, and returns `$"{nextNumber}/{suffix}"` — **an extra
slash the other two do not emit**. Consequence in §4.4.

*Confirmed (negative result).* `PerformaInvService.GetNextOANumberAsync` has **no caller**.
The only call sites for the OA / sale-order generators across `V.SMART/` are
`ContractReviewCheckListUpsert.razor:927`, `MfgPOUpsert.razor:2342`
(`GetNextSaleOrderNoAsync`) and `MfgPOUpsert.razor:3940`. Do not migrate the dead path.

### 3.3 Mechanism C — allocation-table read-modify-write

Two dedicated tables, `DcRunningNumbers` (key `DcType` + `Suffix`) and
`InvoiceAutoRunningNumbers` (key `InvoiceType` + `Suffix`), each holding a `long LastNumber`.
Both are read and written with **plain EF and no lock hint at all**.

| File | Method / block | Lines | Table |
|---|---|---|---|
| `BusinessLayer/BusinessService/CommonService.cs` | `GenerateAutoRunningNoAsync(dcType, suffix)` | **1845-1963** | `DcRunningNumbers` |
| `BusinessLayer/BusinessService/CommonService.cs` | `GeneratePreviewAutoDcRunningNoAsync` | 1965-… (read-only preview) | `DcRunningNumbers` |
| `BusinessLayer/BusinessService/CommonService.cs` | `GenerateInvoiceAutoRunningNoAsync(invType, Suffix)` | **2078-2201** | `InvoiceAutoRunningNumbers` |
| `BusinessLayer/BusinessService/CommonService.cs` | `GeneratePreviewInvoiceAutoRunningNoAsync` | 2204-… (read-only preview) | `InvoiceAutoRunningNumbers` |
| `SalesService/MfgDcService.cs` | inline write inside `UpsertDCAsync`, **manual** branch | 817-841 | `DcRunningNumbers` |
| `SalesService/MfgDcService.cs` | **compensating decrement** in `DeleteMfgDcByDcIdAsync` | 368-382 | `DcRunningNumbers` |
| `SalesService/MfgInvService.cs` | inline write (manual branch) | 442-462 | `InvoiceAutoRunningNumbers` |
| `SalesService/MfgInvService.cs` | **compensating decrement** on delete | 973-986 | `InvoiceAutoRunningNumbers` |
| `SalesService/ExpInvService.cs` | **compensating decrement** on delete | 247-259 | `InvoiceAutoRunningNumbers` |
| `SalesService/ExpInvService.cs` | inline write (manual branch) | 1200-1220 | `InvoiceAutoRunningNumbers` |
| `LabourServices/LabourInvoiceService.cs` | inline write (manual branch) | 264-284 | `InvoiceAutoRunningNumbers` |
| `LabourServices/LabourInvoiceService.cs` | **compensating decrement** on delete | 636-648 | `InvoiceAutoRunningNumbers` |
| `LabourServices/LabourDcOutgoingService.cs` | inline write (manual branch) | 2717-2738 | `DcRunningNumbers` |
| `LabourServices/LabourDcOutgoingService.cs` | inline write (manual branch) | 4523-4544 | `DcRunningNumbers` |
| `LabourServices/LabourDcOutgoingService.cs` | **compensating decrement** on delete | 596-609 | `DcRunningNumbers` |
| `LabourServices/LabourDcOutgoingService.cs` | **compensating decrement** on delete, **extra `> 1` guard** | 5177-5190 | `DcRunningNumbers` |
| `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs` | **compensating decrement** on delete | 222-235 | `DcRunningNumbers` |
| `OutSourcingService/SubContractDcOutService/SubConDcOutService.cs` | **compensating decrement** on delete, **extra `> 1` guard** | 1583-1596 | `DcRunningNumbers` |

*Confirmed (completeness check, 2026-08-20 attempt 2).* The table above is the **whole** write
census: `grep -rn "DcRunningNumbers\|InvoiceAutoRunningNumbers" BusinessLayer/ --include=*.cs`
filtered to `GetQueryable|CreateAsync|UpdateAsync` returns 26 lines, and every one falls inside
a row above. Two related call sites complete the picture and remove an apparent contradiction
with §3.4:

- `SubConDcOutService.cs:921` — `entity.DcNo = await _commonService.GenerateAutoRunningNoAsync("SUBCONDCOUT", entity.Suffix);`,
  directly under the commented-out Mechanism A call at `:920`. This is the SUBCONDCOUT
  allocation site §3.4 implies but does not name.
- `LabourDcOutgoingService.cs:2715` — the `else if (labourDcVM.IsManualDcNo ?? true)` that opens
  the inline write at `:2717-2738`, directly under the commented-out Mechanism A call at
  `:2708`.

*Confirmed negative result:* SUBCONDCOUT has **no** manual-branch inline write — the only
`DcRunningNumbers` writes in `SubConDcOutService.cs` are the two decrements at `:234` and
`:1595`. So the manual-number hazard in (c) below does **not** apply to that series.

*Confirmed.* The two allocation-table repositories are **empty shells** — constructor only,
no methods: `Repository/DcRunningNoRep/DcRunningNumberRepository.cs:15-29` and
`Repository/InvoiceAutoRunnNo/InvoiceAutoRunningNumberRepository.cs:13-27`. All allocation
logic lives in the services.

Four behaviours in this mechanism are load-bearing for any remedy.

**(a) `GenerateAutoRunningNoAsync` swallows its exception and returns `null`.**
*Confirmed*, `CommonService.cs:1957-1961`. So does the invoice allocator —
`CommonService.cs:2195-2199`. **A failed allocation therefore yields a `null` document number
rather than an error.** The caller (`MfgDcService.cs:844`) assigns it straight to
`entity.DcNo` and continues to the duplicate check at `:848`, which does not fire on `null`.
Under an HTTP API this converts a transient database fault into a persisted document with no
number. Recorded in R-12.

**(b) The delete path decrements `LastNumber` — deliberate gap-avoidance.**
*Confirmed*, `MfgDcService.cs:368-382`: the row for `("MFGDC", dc.Suffix)` is read, and
**only when `runningRow.LastNumber == oldDcNo`** (`:377`) is it set to `oldDcNo - 1`
(`:379`). The same shape exists at `MfgInvService.cs:982-985`, `ExpInvService.cs:256-259`
and `LabourInvoiceService.cs:645-648`.

**Re-verified 2026-08-20 — there are eight such blocks in six services, not four.** The
census above missed `LabourDcOutgoingService.cs:596-609` and `:5177-5190` (both
`"LABOURDCOUTGOING"`) and `SubConDcOutService.cs:222-235` and `:1583-1596` (both
`"SUBCONDCOUT"`). So **every one of the six auto-allocated document types decrements on
delete** — the behaviour is universal to Mechanism C, not a quirk of four services. The task
file names only the first site, and so does R-12.

**Six of the eight are unguarded; two carry an extra `> 1` clause.** *Confirmed 2026-08-20
(guard census corrected on attempt 2; the first correction said seven and one).* Reproducible
command and its full output:

```
$ grep -rn "runningRow.LastNumber ==" V.SMART/V.SMART.Shared/BusinessLayer/ --include=*.cs
LabourServices/LabourDcOutgoingService.cs:605:   if (runningRow.LastNumber == oldDcNo)
LabourServices/LabourDcOutgoingService.cs:5186:  if (runningRow.LastNumber == oldDcNo && runningRow.LastNumber > 1)
LabourServices/LabourInvoiceService.cs:645:      if (runningRow.LastNumber == oldInvNo)
OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:231:   if (runningRow.LastNumber == oldDcNo)
OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:1592:  if (runningRow.LastNumber == oldDcNo && runningRow.LastNumber > 1)
SalesService/ExpInvService.cs:256:               if (runningRow.LastNumber == oldExpInvNo)
SalesService/MfgDcService.cs:377:                if (runningRow.LastNumber == oldDcNo )
SalesService/MfgInvService.cs:982:               if (runningRow.LastNumber == oldInvNo )
```

`LabourDcOutgoingService.cs:5186` **and** `SubConDcOutService.cs:1592` are identical lines and
both guard `&& runningRow.LastNumber > 1`. The other **six** omit the clause and will therefore
write `LastNumber = 0` when the first document of a financial year is deleted. Whether a stored
`0` is handled correctly on the next allocation is **Unknown**: the allocator's initialiser is
`1` (`CommonService.cs:1860`), and no branch expects to read `0` back. Raised with the other
M2-B12-02 questions in §10.

**This rules out a plain `CREATE SEQUENCE`**, which cannot be decremented. Flagged for
[`M2-B12-03`](../execution/tasks/M2-B12-03.md) as a hard constraint on the remedy — and the
constraint binds **all six** document types, which materially widens the remedy's scope.

**(c) The manual-number path can move the high-water mark *backwards*.**
*Confirmed*, `MfgDcService.cs:815-841`. `UpsertDCAsync` branches on
`mfgDcVM.IsManualDcNo ?? true` — note that the default when the flag is **null is MANUAL**.
The manual branch writes `runningRow.LastNumber = Convert.ToInt64(entity.DcNo)` (`:836`)
**unconditionally**, so a user typing a *lower* number lowers the mark and the next automatic
allocation reissues numbers already used. The duplicate check runs **after** that write
(`:848`) and refuses with a bare `throw new Exception("Duplicate DC Number found.")`
(`:849-852`) — which under M2-A06's error mapping surfaces as a `500`, not a `409`.
The same `IsManual…` branch exists at `MfgInvService.cs:440`, `ExpInvService.cs:1198` and
`LabourInvoiceService.cs:262`.

**(d) The `Company` discriminator is read with no `WHERE` clause.** *Confirmed*,
`CommonService.cs:1855-1858` and `:2088-2091`:
`_unitOfWork.Companies.GetQueryable().Select(c => c.BookTypeDc).FirstOrDefaultAsync()`.
It takes whichever `Company` row the provider returns first. Safe **only** if a tenant
database holds exactly one `Company` row — a data question, **Unknown** from source, raised
as **Q-39** and handed to [`M2-B12-02`](../execution/tasks/M2-B12-02.md).

### 3.4 Dead code — 7 of the 38 Mechanism A sites have no live caller

*Confirmed.* Method: for each of the 28 distinct raw-SQL method names, every reference across
`V.SMART/` outside `Repository/` was listed and commented-out lines excluded. Five names
returned **zero** live callers; each was then read individually and every remaining reference
confirmed to be a commented-out line:

| Method | Repositories affected | Commented-out callers |
|---|---|---|
| `GetLastDcNoAsync` | `MfgDcRepository`, `LabourDcOutgoingRepository`, `SubConDcOutRepository` (**3 sites**) | `MfgDcService.cs:812`, `LabourDcOutgoingService.cs:2708`, `SubConDcOutService.cs:920`, `CommonService.cs:1680,1688,1697` |
| `GetLastInvNoAsync` | `MfgInvRepository` | `MfgInvService.cs:436`, `CommonService.cs:1772` |
| `GetLastExpInvNoAsync` | `ExpInvRepository` | `ExpInvService.cs:1194`, `CommonService.cs:1790` |
| `GetLastLabInvoiceNoAsync` | `LabInvRepository` | `LabourInvoiceService.cs:258`, `CommonService.cs:1781` |
| `GetLastReqNoAsync` | `StockRequestIssueRepository` | none of any kind |

These are exactly the DC and invoice families — the series that moved from Mechanism A to
Mechanism C and left the old method behind. **M2-B12-03's remedy surface is 31 live sites,
not 38.** This document marks them dead rather than proposing to fix them.

*Caveat (Inferred, high confidence).* The liveness measurement is a `git grep` over
`V.SMART/` (all four projects). It would miss a call made by reflection, or a Razor call
written so that the method name does not appear literally. No such mechanism was observed;
the risk is low but not zero.

---

## 4. Format catalogue

### 4.1 The general shape — number and suffix are stored separately

**Confirmed.** For every `Suffix`-scoped series the allocator returns a **bare integer
string** (`"12"`) which is stored in the number column, and the financial-year suffix is
stored **in its own column**. The user-visible document number is the **concatenation**,
performed at display / report / payload time:

- `MfgDcService.cs:2083` — `DocNo = dc.DcNo + dc.Suffix`
- `MfgInvService.cs:2057` — `DocNo = dc.InvNo + dc.Suffix`
- `LabourDcOutgoingService.cs:4482`, `SubConDcOutService.cs:2778` — same shape
- `LabourInvoiceService.cs:755` — `["DcNo"] = $"{r.DcNo}{r.Suffix}"`
- `SubConGRNService.cs:852,1053,1122` — `RefDcNo = $"{x.SubConDcOut.DcNo}{x.SubConDcOut.Suffix}"`

With `FinancialYearHelper` producing `"/2025-26"` (§5), the resulting user-visible string is:

> **Worked example.** `DcNo = "12"`, `Suffix = "/2025-26"` → **`12/2025-26`**.

This one pattern covers **every** Mechanism A series and the DC/invoice Mechanism C series.
Substituting the number column gives the shape for each: `QuoteNo` → `12/2025-26`,
`PONo` → `12/2025-26`, `GRNNo` → `12/2025-26`, `SCNNo` → `12/2025-26`, and so on. There is
**no zero-padding and no alphabetic prefix** anywhere in this family — the allocators return
`nextNumber.ToString()` or `$"{runningRow.LastNumber}"` verbatim
(`CommonService.cs:1955`, `:2191`), and every Mechanism A repository ends the same way — e.g.
`DebitNoteRepository.cs:51`, `return nextNumber.ToString();`.

### 4.2 Series that deviate from the general shape

| Series | Shape | Assembled at | Worked example | Notes |
|---|---|---|---|---|
| **Non-returnable labour DC** | `NR` + integer, **no suffix, no year scope** | `LabourDcOutgoingRepository.GetNextNrDcNoAsync:59-82` | `NR7` | Selected by `DcNo LIKE 'NR%'` and `NonReturnDc = 1`; the previous number is parsed with `lastNr.Substring(2)`. The `suffix` parameter is accepted and **never used**. |
| **Month-wise production issue** | integer, scoped by department **and** month | `ProductionIssueAssyRepository.GetMonthWiseProductionIssueNumberAsync:49-100` | `4` (within `DepartmentCode='AD'`, `MonthCode='08'`, `Suffix='/2025-26'`) | The scope key includes `deptCode`, defaulting to the **literal `"AD"`** (`:52`) when the current user has no `Staff.DepartmentCode` (`:53-64`), and `monthCode = today.Month.ToString("D2")` (`:67`). **The number a user gets depends on who is logged in** — an API concern, since the API resolves identity differently from Blazor. |
| **Tool-crib return** | integer, but the reader tolerates an embedded `/suffix` | `ToolCribReturnRepository.GetLastReturnNoAsync:29-57` | `12` | `ORDER BY TRY_CAST(LEFT(TCReturnNo, CHARINDEX('/', TCReturnNo + '/') - 1) AS INT) DESC` (`:38`) and `lastNumberStr.Split('/')[0]` (`:45`) — defensive parsing implying some stored `TCReturnNo` values **do** embed the suffix. Whether any actually do is **Unknown** from source (data question → M2-B12-02). |
| **Staff loan** | integer, **no scope of any kind** | `StaffLoanRepository.GetLastLoanNoAsync:31-47` | `41` | Never rolls over at year end. |
| **Attendance** | integer identity + 1 | `AttendanceRepository.GetLastIdAsync:41-48` | `882` | Not a document number in the statutory sense; listed for completeness. |

### 4.3 Sale-order number — the one series with a stored prefix and padding

**Confirmed**, `MfgPoService.GetNextSaleOrderNoAsync:1623-1660`.

1. Scope is **`x.PoTypeId == poTypeId` only** (`:1627`) — **no `Suffix`, no year**. This
   series **never rolls over at year end**, unlike every other.
2. The previous `SaleOrderNo` is matched with `Regex.Match(lastOrderNo, @"(\d+)$")` (`:1638`);
   the prefix is `lastOrderNo.Substring(0, match.Index)` and the padding width is
   `new string('0', match.Value.Length)` (`:1644-1645`). **Both prefix and width are inherited
   from the previous row**, not configured anywhere.
3. Fallbacks, in order: `PoType.SeriesNo` (`:1649`, `:1653`, read at `:1663-1670`), then the
   literal `"SO-0001"`.

> **Worked examples.** Previous `SaleOrderNo = "SO-0007"` → next **`SO-0008`**. Previous
> `"ACME/0099"` → next **`ACME/0100`**.
>
> **Unknown.** The prefix and width for a `PoType` that has no rows yet come from
> `PoType.SeriesNo`, which is **stored data not present in this repository**. Marked Unknown
> rather than guessed.

4. **It fails open.** *Confirmed*, `:1656-1659`: the `catch` block **also returns
   `"SO-0001"`**. A transient database fault therefore silently yields a document number that
   very probably already exists, rather than an error. This is a sibling of the
   null-on-failure defect in §3.3(a), was not previously recorded anywhere, and is added to
   R-12.

### 4.4 Order-acceptance (OA) number — two independent series, two shapes

**Confirmed.** `MfgPoService` / `PerformaInvService` return `$"{nextNumber}{suffix}"`
(`MfgPoService.cs:1723` / `PerformaInvService.cs:1096`), while `ContractReviewService`
returns `$"{nextNumber}/{suffix}"` (`ContractReviewService.cs:785`).

Since the suffix supplied by the caller **already begins with `/`** —
`ContractReviewCheckListUpsert.razor:629` passes
`FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now)`, and that method returns
`$"/{year}-…"` (`FinancialYearHelper.cs:16`) — the two shapes are:

| Series | Source table | Expression | Worked example |
|---|---|---|---|
| Sales-order OA | `MfgPo.OANo` | `$"{nextNumber}{suffix}"` | **`12/2025-26`** |
| Contract-review OA | `ContractReview.OANo` | `$"{nextNumber}/{suffix}"` | **`12//2025-26`** — *Inferred, high confidence*: a **double slash** |

The double slash is *Inferred* rather than Confirmed because confirming it requires reading a
stored value, which is a database question. It is handed to
[`M2-B12-02`](../execution/tasks/M2-B12-02.md) as a one-line `SELECT`. If it is real it is a
**format** difference on a customer-facing document, and the compliance warning at the head of
this document applies: M2-B12-03 must **preserve** it, not tidy it.

### 4.5 What is Unknown in this catalogue, and deliberately so

Parts of the shape that depend on **stored data not in the repository** are marked Unknown
rather than guessed:

- `Company.BookTypeDc` / `Company.BookTypeInvoice` — decide which series-sharing branch
  (§6) a tenant exercises, and therefore whether MFGDC and LABOURDCOUTGOING share one
  ascending series or run independently.
- `PoType.SeriesNo` — the sale-order prefix and padding width for a fresh `PoType`.
- `Staff.DepartmentCode` — the scope key of the month-wise production issue number.
- Whether any stored `TCReturnNo` actually embeds a suffix.

**This is not an incomplete deliverable.** These are M2-B12-02's questions by design.

---

## 5. Financial-year rules — two implementations, one boundary, two shapes, one winner

### 5.1 The two implementations

**Implementation 1 — `FinancialYearHelper.GetFinancialYearSuffix(DateTime)`**
(*Confirmed*, `V.SMART/V.SMART.Shared/Services/FinancialYearHelper.cs:11-17`; the whole file
is 19 lines):

```csharp
int year = date.Month >= 4 ? date.Year : date.Year - 1;  // April = financial year start
int nextYear = year + 1;
return $"/{year}-{nextYear.ToString().Substring(2)}";
```

Output shape: **`/{yyyy}-{yy}`** — e.g. `/2025-26`, with a **leading slash**.

**Implementation 2 — the local in `CommonService`** (*Confirmed*,
`CommonService.cs:1849-1851`, repeated verbatim at `:1969-1971`):

```csharp
DateTime now = DateTime.Now;
int currentYear = now.Month > 3 ? now.Year : now.Year - 1;
string financialYear = $"{currentYear % 100:D2}-{(currentYear + 1) % 100:D2}";
```

Output shape: **`{yy}-{yy}`** — e.g. `25-26`, with **no leading slash**.

### 5.2 Do they agree on the boundary?

**Yes.** *Confirmed by inspection.* `date.Month >= 4` and `now.Month > 3` are the identical
predicate over integers: the financial year starts on **1 April** in both. There is no
boundary divergence to fix.

### 5.3 Do their output shapes differ?

**Yes.** `/2025-26` versus `25-26` — a different leading character *and* a different year
width. If both reached storage, two suffix vocabularies would coexist in one database and
every `WHERE Suffix = {0}` scope would silently split.

### 5.4 Which one actually reaches a stored `Suffix`? — the question answered

**`FinancialYearHelper` produces every stored `Suffix`. The `CommonService` implementation is
dead code and reaches nothing.** *Confirmed*, by three independent observations:

1. **The `CommonService` variable is never read.** `git grep -n financialYear` over
   `CommonService.cs` returns **exactly two lines** — `:1851` and `:1971`, both *assignments*.
   There is no third line, so `financialYear` is assigned and discarded in both methods. The
   two invoice allocators (`:2078`, `:2204`) do not compute it at all.
2. **The allocators use the `suffix` *parameter*, not the local.** `GenerateAutoRunningNoAsync`
   scopes every read and write by the `suffix` argument (`:1865`, `:1872`, `:1879`, `:1913`,
   `:1920`) and returns `$"{runningRow.LastNumber}"` (`:1955`) — the local never appears.
3. **The parameter is supplied by `FinancialYearHelper`, from Razor `@code`.**
   `GetFinancialYearSuffix` has **77 occurrences across 63 files**: **53 files under
   `Pages/`**, 9 files under `BusinessLayer/`, 1 under `Services/`. Worked examples:
   `MfgDcUpsert.razor:1447`, `ContractReviewCheckListUpsert.razor:629`, `Payments.razor:1333`,
   `Receipts.razor:1335`, `AdvanceAdjustment.razor:1302`, `FinalInspectionUpsert.razor:918`,
   `AppointmentLetterUpsert.razor:534`, `OfferLetterUpsert.razor:500` — each of the form
   `<VM>.Suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);`.

**Therefore the stored `Suffix` is always `/{yyyy}-{yy}`, and the divergence is latent, not
active.** *Nonetheless*, R-12's instruction that **M2-B12-03 must not "unify" them** stands:
the dead local is harmless, but any refactor that accidentally routed the `{yy}-{yy}` shape
into a stored `Suffix` would change a statutory document's appearance.

### 5.5 The migration consequence — the scope key lives in `@code`

**This is the highest-value migration finding in this investigation.** The financial-year
suffix — the scope key of the *entire* numbering system — is computed **in Razor `@code`, in
53 files**, and passed *into* the services as a parameter. No service derives it. Per
[`CLAUDE.md`](../../../CLAUDE.md), logic trapped in `@code` is **extracted into server-side
services** before any Angular screen replaces it; it is never reimplemented in TypeScript.
Every Angular document screen therefore needs the suffix resolved **server-side** — which is
also the only way the server can stay authoritative for document numbering.

---

## 6. Series-sharing rules — `BookTypeDc` **and** `BookTypeInvoice`

The task file names only `Company.BookTypeDc`. There is a **second discriminator**,
`Company.BookTypeInvoice`, driving the invoice allocator — and **its branch values are the
mirror image of the DC allocator's**. Anyone who assumes the two agree gets them exactly
backwards.

*Confirmed*, `CommonService.cs:1855-1858` (`BookTypeDc`, a `byte`) and `:2088-2091`
(`BookTypeInvoice`, a `byte`).

The ten rules below are registered in
[KB-030](../business-rules/business-rule-inventory.md) as **BR-DOC-001 … BR-DOC-010**.

### 6.1 DC series (`GenerateAutoRunningNoAsync`, `dcType` in {MFGDC, LABOURDCOUTGOING, SUBCONDCOUT})

Three candidate high-water marks are read first — `mfg` (`:1862-1867`), `lab` (`:1869-1874`),
`subcon` (`:1876-1881`), each the `DcRunningNumbers.LastNumber` for that `DcType` + `Suffix`.

| Rule | Condition | Effect | Evidence |
|---|---|---|---|
| **BR-DOC-001** | `BookTypeDc == 1` | **Separate series.** Each `dcType` takes its **own** mark — `startNumber = mfg` / `lab` / `subcon`, **with no `+1`** | `CommonService.cs:1883-1894` |
| **BR-DOC-002** | `BookTypeDc == 2` and `dcType` in {MFGDC, LABOURDCOUTGOING} | **Shared series** for those two: `startNumber = Math.Max(mfg, lab) + 1` | `:1895-1898` |
| **BR-DOC-003** | `BookTypeDc == 2` and `dcType == SUBCONDCOUT` | Independent: `startNumber = subcon + 1` | `:1899-1902` |
| **BR-DOC-004** | `BookTypeDc == 3` | **Fully shared**: `startNumber = Math.Max(Math.Max(mfg, lab), subcon) + 1` | `:1903-1907` |
| **BR-DOC-005** | `BookTypeDc <= 0` **and the row already exists** | Per-type increment: `startNumber = mfg`/`lab`/`subcon` `+ 1` for the matching `dcType` | `:1928-1944` |

If no `DcRunningNumbers` row exists for (`dcType`, `Suffix`), one is **created** with
`LastNumber = startNumber` (`:1915-1925`); the `<= 0` fallback is only reached on the
existing-row path.

> **Defect or design? — Unknown, raised as Q-38.** BR-DOC-001 assigns `startNumber` **without
> `+1`** and then writes it straight back (`:1947`) and returns it (`:1955`). Under
> `BookTypeDc == 1` the allocator therefore appears to return the **same number on every
> call**. The `+1` exists on every other branch. The source states the behaviour but not the
> intent — it may be an intended "manual entry sets the mark" design, or a duplicate-number
> defect. **Not resolvable from source.** M2-B12-02 can decide it empirically: duplicate
> `(DcType, Suffix, number)` rows concentrated in tenants whose `Company.BookTypeDc = 1`
> would settle it.

### 6.2 Invoice series (`GenerateInvoiceAutoRunningNoAsync`, `invType` in {MFGINV, LABINV, EXPINV})

Same shape, reading `mfg` (`:2097-2102`), `lab` (`:2104-2109`), `expinv` (`:2111-2116`) from
`InvoiceAutoRunningNumbers`. **Note the inverted values.**

| Rule | Condition | Effect | Evidence |
|---|---|---|---|
| **BR-DOC-006** | `BookTypeInvoice == 2` | **Separate series** — `startNumber = mfg` / `lab` / `expinv`, **no `+1`** (the DC allocator uses value **1** for this) | `CommonService.cs:2119-2130` |
| **BR-DOC-007** | `BookTypeInvoice == 1` and `invType` in {MFGINV, LABINV} | **Shared**: `Math.Max(mfg, lab) + 1` (the DC allocator uses value **2** for this) | `:2131-2134` |
| **BR-DOC-008** | `BookTypeInvoice == 1` and `invType == EXPINV` | Independent: `expinv + 1` | `:2135-2138` |
| **BR-DOC-009** | `BookTypeInvoice == 3` | **Fully shared**: `Math.Max(Math.Max(mfg, lab), expinv) + 1` | `:2139-2143` |
| **BR-DOC-010** | `BookTypeInvoice <= 0` **and the row already exists** | Per-type increment `+ 1` | `:2164-2177` |

**Any remedy must reproduce every one of these ten branches.** Which branch a real tenant
exercises is **Unknown** from source — it depends on `Company.BookTypeDc` /
`Company.BookTypeInvoice` values, which are data. That is M2-B12-02's census.

---

## 7. Application-level duplicate checks and their scoping

*Confirmed.* `git grep --untracked -c "IsDuplicate.*Async" -- V.SMART/` returns **81
occurrences across 59 files** (interfaces, services and Razor call sites) — **not** the 62/41
the task file states; the task figure is stale. **23** of the 81 are service implementations.
Scope is expressed in the parameter list and the `AnyAsync` predicate, and it is **not
uniform** — which matters directly to §9: an unqualified `(Number, Suffix)` unique index would
reject data the application currently accepts.

| Service (under `BusinessLayer/BusinessService/`) | Method | Line | Scope beyond the number |
|---|---|---|---|
| `SalesService/MfgDcService.cs` | `IsDuplicateDcNoAsync` | 771 | `Suffix` **+ `CustId`** |
| `SalesService/MfgInvService.cs` | `IsDuplicateMfgInvoiceNoAsync` | 396 | `Suffix` + `CustId` |
| `SalesService/ExpInvService.cs` | `IsDuplicateExportNoAsync` | 1153 | `Suffix` + `CustId` |
| `SalesService/MfgPoService.cs` | `IsDuplicatePoAsync` | 964 | `Suffix` + `custId` |
| `SalesService/PerformaInvService.cs` | `IsDuplicatePoAsync` | 998 | `Suffix` + `custId` |
| `SalesService/EnquirySalesService.cs` | `IsDuplicateEnquiryAsync` | 537 | `Suffix` + `custId` |
| `SalesService/ContractReviewService.cs` | `IsDuplicateContractReviewNoAsync` | 660 | `poId` — **no `Suffix`**; and it checks `ContNo`, not `OANo` |
| `SalesService/EnqiryFeasibility/EnqiryFeasibilityService.cs` | `IsDuplicateEnquiryFeasibilityNoAsync` | 500 | `custId` — **no `Suffix`** |
| `LabourServices/LabourDcOutgoingService.cs` | `IsDuplicateDcNoAsync` | 2648 | `CustId` + `suffix` |
| `LabourServices/LabourGRNService.cs` | `IsDuplicateDcNoAsync` | 1119 | `CustId` + `suffix` |
| `OutSourcingService/PurchOrSubConPoService/PuchPoService.cs` | `IsDuplicatePoAsync` | 961 | `suffix` + `vendorcode` + **`RevesionNo`** *(sic)* |
| `OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs` | `IsDuplicateQuoteAsync` | 251 | `suffix` + `VendorCode` |
| `OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs` | `IsDuplicateDcNoAsync` | 930 | `vendorCode` + `suffix` |
| `OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs` | `IsDuplicateInvNoAsync` | 946 | `vendorCode` + `suffix` |
| `OutSourcingService/Purchase_Invoice_Service/PurchaseInvoiceService.cs` | `IsDuplicateInvoiceAsync` | 1026 | `suffix` + `VendorCode` |
| `OutSourcingService/SubContractInvoiceService/SubConInvService.cs` | `IsDuplicateInvoiceAsync` | 747 | `suffix` + `VendorCode` |
| `AccountsService/PaymentsService.cs` | `IsDuplicatePaymentsAsync` | 1503 | **none** — number only |
| `AccountsService/ReceiptsService.cs` | `IsDuplicatePaymentsAsync` | 1533 | **none** — number only |
| `LeadService/LeadService.cs` | `IsDuplicateLeadsNameAsync` | 153 | n/a — not a document number |
| `MasterService/AccountsService/BankService.cs` | `IsDuplicateAccountNoAsync` | 74 | n/a — master data |
| `MasterService/InventoryService/CategoryService.cs` | `IsDuplicateCategoryNameAsync` | 54 | n/a — master data |
| `MasterService/InventoryService/RawMaterialService.cs` | `IsDuplicateRawMaterialAsync` | 196 | n/a — master data |
| `SettingsService/ProdLogSettingsService.cs` | `IsDuplicateProdTypeNameAsync` | 43 | n/a — settings |

Worked example — `MfgDcService.IsDuplicateDcNoAsync:778-783`:

```csharp
return await _unitOfWork.MfgDcs.GetQueryable()
    .AnyAsync(x => x.DcNo == DcNo && x.Suffix == suffix && x.CustId == CustId
                && (currentDcId == null || x.DcId != currentDcId));
```

**Confirmed (negative result).** Many series that *do* allocate numbers have **no** duplicate
check at all — among them `LabInv` (labour invoice), `MfgQuote`, every Production series,
every Planning series, every Inventory/Stock series, `DebitNote`, `CreditNote`,
`StoreInterTrans`, `ToolCribIssue`/`ToolCribReturn`, `MaterialIssNote`, `SCNGen`,
`StockIssueRequest`, `EnquiryPurchase`, `MaterialReq` and `StaffLoan`. Nothing at any layer
prevents a duplicate for those. Reflected per-row in §9.

**Confirmed.** Every check runs **after** the allocation, in the same request, on the same
non-serialized connection — so it narrows the race window; it does not close it (§8).

---

## 8. Concurrency analysis — why `UPDLOCK, ROWLOCK` is *not* protection

**This section exists so that no reviewer of
[`M2-B12-03`](../execution/tasks/M2-B12-03.md) can argue the code is already protected.**

**`UPDLOCK, ROWLOCK` without `HOLDLOCK` and without an enclosing transaction does not prevent
the race.** Three independent reasons, all Confirmed from source:

1. **It is a row lock, not a range lock.** `UPDLOCK` takes an update lock on the row(s)
   actually **read**. It cannot lock a row that does not exist yet, so it cannot block the
   **`INSERT`** a competing session is about to perform. Two sessions reading `TOP 1 …` see
   the same current maximum, both compute `max + 1`, and both insert it. When no row
   qualifies at all, they lock **nothing**.
2. **Outside an explicit transaction the lock is released at statement end.** Every Mechanism
   A repository method issues exactly one statement and returns
   (e.g. `MfgDcRepository.cs:29-41`); the lock is gone before the caller has computed `+1`,
   let alone inserted.
3. **Where an explicit transaction *does* exist, it is READ COMMITTED.** This is the sharpest
   point and it cuts in both directions. `MfgDcService.UpsertDCAsync:802` genuinely wraps the
   allocation and the insert in one transaction — but `UnitOfWork.BeginTransactionAsync`
   (`Repository/UnitOfWork.cs:798-801`) calls `_db.Database.BeginTransactionAsync()` with **no
   `IsolationLevel`**, so the default applies. Under READ COMMITTED a phantom insert by
   another session is **permitted**. The read→insert gap is transactional and still
   unprotected.

**The hint is close to decorative — and it has already misled a reader.** *Confirmed*,
`V.SMART/V.SMART.Shared/Repository/OutSourcingRepository/DebitNote_Repository/DebitNoteRepository.cs:32`:

```csharp
// Safely fetch the latest QuoteNo with locking to prevent concurrency issues
```

That comment is wrong on three counts: the lock does not make it safe; the method is not
fetching a `QuoteNo` (it fetches `DebitNo`, `:35-41`); and the comment was evidently copied
from the quote repository. It is direct evidence that a reader has already concluded from the
hint that the problem was handled.

**Mechanism C is worse than A, not better.** *Confirmed.* `GenerateAutoRunningNoAsync` reads
three `DcRunningNumbers` rows with **plain EF and no hint whatsoever** (`:1862-1881`),
computes `startNumber` in C#, then writes (`:1947`) and `SaveAsync()`s (`:1953`). This is a
textbook lost update: two concurrent callers read the same `LastNumber`, both compute the same
successor, and the second write silently overwrites the first with the identical value.
**An engineer told only to "replace the ~20 `SELECT TOP 1` calls" would leave this untouched
— and this is where the worst race lives.**

**What would actually close it** (for M2-B12-03 to choose between; not decided here): range
locking across read+insert (`HOLDLOCK` / `SERIALIZABLE`), an application lock
(`sp_getapplock`), an atomic `UPDATE … SET LastNumber = LastNumber + 1 … OUTPUT`, or a
database sequence. **A stronger row hint is not one of the options.**

**Constraints any remedy must satisfy** — each Confirmed above:

1. The **decrement on delete** (§3.3(b), **eight blocks in six services**, re-verified
   2026-08-20) rules out a plain `CREATE SEQUENCE`. It binds every auto-allocated type.
2. `CustId` / `VendorCode` scoping (§7) rules out an unqualified `(Number, Suffix)` unique
   index.
3. The **manual-number** path (§3.3(c)) must keep working — including its ability to move the
   mark, unless the owner decides otherwise.
4. All **ten** `BookType*` branches (§6) must be reproduced.
5. **Formats must be byte-preserved** (§4), including the sale-order prefix/padding
   inheritance and the contract-review double slash.

**Why now.** Blazor Server's low concurrency is the only thing currently masking this. That is
a property of the **host**, not of the code. *Confirmed (negative result), 2026-08-20:*
`V.SMART.Api/Controllers/` contains only `AuthController.cs` and `CurrencyController.cs`, and
a grep of `V.SMART.Api/` for `runningno|GetLast.*NoAsync|Suffix|DcNo|InvNo` returns **zero**
matches. **No document-creating endpoint exists yet** — which is exactly why this work is
cheap now and expensive later.

---

## 9. Handoff table — `(table, number column, scope columns)` → M2-B12-02

**This is the machine-readable deliverable.** One row per document series.
[`M2-B12-02`](../execution/tasks/M2-B12-02.md) turns each row directly into a SQL query
against a live tenant database (database-per-tenant,
[KB-014](../architecture/multi-tenancy.md) — so **no tenant column appears in any scope**;
isolation is by connection).

`Nullable?` is read from the EF entity property under `<Nullable>enable</Nullable>`
(`V.SMART.Shared.csproj:5`): `string?` → nullable, `string` → NOT NULL by convention.
**Unique in EF model?** is `Y` only for a `HasIndex(...).IsUnique()` in
`ApplicationDbContext.cs`.

| Table | Number column | Scope columns | Nullable? | Unique in EF model? | Application-level duplicate check |
|---|---|---|---|---|---|
| `MfgDc` | `DcNo` | `Suffix`, `CustId` | N | N | `SalesService/MfgDcService.cs:771-790` |
| `MfgInv` | `InvNo` | `Suffix`, `CustId` | N | N | `SalesService/MfgInvService.cs:396` |
| `ExpInv` | `ExpInvNo` | `Suffix`, `CustId` | N | N | `SalesService/ExpInvService.cs:1153` |
| `LabInv` | `LabInvNo` | `Suffix` | N | N | **none** |
| `MfgQuote` | `QuoteNo` | `Suffix` | N | **Y** — `ApplicationDbContext.cs:579-582` | **none** |
| `PerformaInv` | `InvNo` | `Suffix` | N | N | `SalesService/PerformaInvService.cs:998` (`IsDuplicatePoAsync`) |
| `MfgPo` | `PONo` | `Suffix`, `custId` | N | N | `SalesService/MfgPoService.cs:964` |
| `MfgPo` | `SaleOrderNo` | **`PoTypeId` only — no `Suffix`** | **Y** | N | **none** |
| `MfgPo` | `OANo` | `Suffix` | **Y** | N | **none** |
| `ContractReview` | `OANo` | `Suffix` *(matched by `EndsWith`)* | **Y** | N | **none** — `ContractReviewService.cs:660` checks `ContNo`, not `OANo` |
| `EnquirySales` | `EnquiryNo` | `Suffix`, `custId` | N | N | `SalesService/EnquirySalesService.cs:537` |
| `LabourDcOutgoing` | `DcNo` | `Suffix`, `NonReturnDc = 0`, `CustId` | N | N | `LabourServices/LabourDcOutgoingService.cs:2648` |
| `LabourDcOutgoing` | `DcNo` (`NR` series) | `NonReturnDc = 1`, `DcNo LIKE 'NR%'` — **no `Suffix`** | N | N | **none** |
| `LabourGRN` | `GRNNo` | `Suffix` | N | N | **none** on `GRNNo` — `LabourGRNService.cs:1119` checks the referenced `DcNo` |
| `LabourSCN` | `SCNNo` | `Suffix` | N | N | **none** |
| `SubConDcOut` | `DcNo` | `Suffix` | N | N | **none** |
| `SubConGRN` | `GRNNo` | `Suffix` | N | N | **none** |
| `SubConSCN` | `SCNNo` | `Suffix` | N | N | **none** |
| `SubConInv` | `InvNo` | `Suffix`, `VendorCode` | N | N | `SubContractInvoiceService/SubConInvService.cs:747` |
| `PurchPo` | `PONo` | `Suffix`, `vendorcode`, `RevesionNo` | N | N | `PurchOrSubConPoService/PuchPoService.cs:961` |
| `PurchaseGRN` | `GRNNo` | `Suffix`, `vendorCode` | N | N | `PurchaseGRN_Service/PurchaseGRNService.cs:930`, `:946` |
| `PurchaseSCN` | `SCNNo` | `Suffix` | N | N | **none** |
| `PurchaseInvoice` | `InvNo` | `Suffix`, `VendorCode` | N | N | `Purchase_Invoice_Service/PurchaseInvoiceService.cs:1026` |
| `PurchaseQuote` | `QuoteNo` | `Suffix`, `VendorCode` | N | N | `PurchOrSubConQuoteService/PurchaseQuoteService.cs:251` |
| `EnquiryPurchase` | `EnquiryNo` | `Suffix` | N | N | **none** |
| `MaterialReq` | `MReqNo` | `Suffix` | N | N | **none** |
| `DebitNote` | `DebitNo` | `Suffix` | **Y** | N | **none** |
| `CreditNote` | `CreditNo` | `Suffix` | **Y** | N | **none** |
| `RouteCard` | `RCNo` | `Suffix` | N | N | **none** |
| `RouteCardRelease` | `RcReleaseNo` | `Suffix` | N | N | **none** |
| `JobOrder` | `JobNo` | `Suffix` | N | N | **none** |
| `Estimate` | `EstiamateNo` *(sic)* | `Suffix` | N | N | **none** |
| `ProductionLog` | `LogNo` | `Suffix` | N | N | **none** |
| `ProductionIssueAssy` | `IssueNo` | `Suffix` | N | N | **none** |
| `ProductionIssueAssy` | `IssueNo` (month-wise series) | `DepartmentCode`, `MonthCode`, `Suffix` | N | N | **none** |
| `ProductionReturnAssy` | `ReturnNo` | `Suffix` | N | N | **none** |
| `ProductionSCNAssy` | `SCNNo` | `Suffix` | N | N | **none** |
| `ProductionIssueComp` | `IssueNo` | `Suffix` | N | N | **none** |
| `ProductionReturnComp` | `ReturnNo` | `Suffix` | N | N | **none** |
| `ProductionSCNComp` | `SCNNo` | `Suffix` | N | N | **none** |
| `MaterialIssNote` | `IssueNo` | `Suffix` | N | N | **none** |
| `SCNGen` | `SCNGenNo` | `Suffix` | N | N | **none** |
| `StockIssueRequest` | `IssueNo` *(allocator `GetLastReqNoAsync` is dead — §3.4)* | `Suffix` | N | N | **none** |
| `StoreInterTrans` | `ISTNo` | `Suffix` | N | N | **none** |
| `ToolCribIssue` | `TCIssueNo` | `Suffix` | N | N | **none** |
| `ToolCribReturns` | `TCReturnNo` | `Suffix` | N | N | **none** |
| `StaffLoan` | `LoanNo` | **none at all** | N | N | **none** |
| `Payments` | `PaymentNo` | none | — | N | `AccountsService/PaymentsService.cs:1503` |
| `Receipts` | `PaymentNo` | none | — | N | `AccountsService/ReceiptsService.cs:1533` |
| `DcRunningNumbers` *(allocation table)* | `LastNumber` | `DcType`, `Suffix` | n/a (`long`) | **N — no unique index on (`DcType`, `Suffix`)** | n/a |
| `InvoiceAutoRunningNumbers` *(allocation table)* | `LastNumber` | `InvoiceType`, `Suffix` | n/a (`long`) | **N — no unique index on (`InvoiceType`, `Suffix`)** | n/a |

**Two rows deserve to be read twice.** The **allocation tables themselves carry no unique key
on their logical key** in the EF model, so `GenerateAutoRunningNoAsync`'s
`FirstOrDefaultAsync(x => x.DcType == dcType && x.Suffix == suffix)` (`:1909-1913`) can
silently pick one of several duplicate rows. And **the large majority of the ~50 series have
no protection at any layer** — neither a constraint in the EF model nor an application check.

**Note for M2-B12-02.** `Suffix` values are stored **with a leading slash** (`/2025-26`, §5),
so a query written as `WHERE Suffix = '2025-26'` returns nothing. The `Payments` /
`Receipts` `PaymentNo` nullability was not read from source and is left `—`.

---

## 10. Open questions raised, and negative results recorded

### 10.1 Questions raised (see [KB-004](../open-questions.md))

| Id | Question | Why it cannot be answered here |
|---|---|---|
| **Q-10** *(pre-existing, stays open)* | Do the document-number columns carry unique constraints in the **live** tenant databases? | Requires database access — M2-B12-02. This document supplies §9 as its input. |
| **Q-37** *(new)* | Do document numbers cross into e-Invoice / e-Way payloads in a **shape-sensitive** way? | The coupling is Confirmed — `EWayDatabaseService.cs:216,227,239,251` matches records by `DcNo + Suffix` / `InvNo + Suffix`. Whether a downstream government API *parses* that shape is INV-015's question (Scheduled, Phase 4.5), and this task is **forbidden** from running it. Recorded, not investigated. |
| **Q-38** *(new)* | Is the "separate series" branch that omits the `+1` (`CommonService.cs:1883-1894`, `:2119-2128`) intended design or a duplicate-number defect? | The source states the behaviour, not the intent. Needs the owner, or M2-B12-02's duplicate census correlated against `Company.BookTypeDc`. |
| **Q-39** *(new)* | Can a tenant database hold **more than one `Company` row**? | If so, the unscoped `Companies…FirstOrDefaultAsync()` discriminator reads (`CommonService.cs:1855-1858`, `:2088-2091`) return an arbitrary company's setting. One `COUNT(*)` in M2-B12-02 settles it. |
| **Q-40** *(new; guard census corrected on attempt 2)* | When the **first** document of a financial year is deleted, **six** of the eight decrement blocks write `LastNumber = 0`. Is `0` handled correctly, and is the other **two** blocks' extra `> 1` guard the intended behaviour or the accident? | Exactly two guard `&& runningRow.LastNumber > 1` — `LabourDcOutgoingService.cs:5186` and `SubConDcOutService.cs:1592`, identical lines; the other six omit it (§3.3(b)). Which is correct is not recoverable from source. Owner decision, or M2-B12-02 finding stored `LastNumber = 0` rows. **M2-B12-03 must not normalise the eight without answering it.** |

Additional **Unknown**s are recorded in §4.5 rather than as separate questions, because they
are all resolved by the same M2-B12-02 census: `Company.BookTypeDc` / `BookTypeInvoice`
values, `PoType.SeriesNo`, `Staff.DepartmentCode`, and whether any stored `TCReturnNo` embeds
a suffix.

### 10.2 Negative results — Confirmed, 2026-08-20

Each of these rules out "it must be handled somewhere else", which is why they are recorded.

1. **No serializable transaction, no application lock, no database sequence.** Zero matches
   across `V.SMART/` for `HOLDLOCK`, `sp_getapplock`, `IsolationLevel`, `Serializable`,
   `CREATE SEQUENCE` and `HasSequence`.
2. **One document-number unique index in the whole EF model.** `ApplicationDbContext.cs`
   contains exactly **three** `IsUnique()` calls: `:582` (`MfgQuote(QuoteNo, Suffix)` — the
   only document number), `:595` (`AssmblyDef(AssmblyID, ItemId)`) and `:618`
   (`AssemblyDefLabour(AssmblyID, ItemId)`).
3. **No tenant column in any numbering query.** Consistent with database-per-tenant
   ([KB-014](../architecture/multi-tenancy.md)): isolation is by connection, not by
   predicate. Every scope column found is a *business* scope. **M2-B12-02 must not add a
   `TenantId` to any §9 query.**
4. **`V.SMART.Api` exposes nothing in this area.** Controllers are `AuthController.cs` and
   `CurrencyController.cs` only; grepping `V.SMART.Api/` for
   `runningno|GetLast.*NoAsync|Suffix|DcNo|InvNo` returns zero. Nothing to extend, nothing to
   break — and the window in which a fix is cheap.
5. **The allocation-table repositories contain no logic.**
   `DcRunningNumberRepository.cs:15-29` and `InvoiceAutoRunningNumberRepository.cs:13-27` are
   constructor-only. Do not look for the rules there.
6. **`PerformaInvService.GetNextOANumberAsync` has no caller**, and **7 of the 38 Mechanism A
   sites have no live caller** (§3.4).
7. **The `CommonService` financial-year local is never read** — `git grep -n financialYear`
   over that file returns exactly two lines, both assignments (§5.4).

---

## Related documents

- [KB-060 R-12](../risks/technical-debt-register.md) — the risk this document corrects and
  substantiates. **Classification remains `Inferred (high confidence)`**: reading code proves
  a race is *possible*, not that one has *occurred*. Only M2-B12-02's duplicate census can
  upgrade it.
- [KB-030](../business-rules/business-rule-inventory.md) — BR-DOC-001 … BR-DOC-010.
- [KB-004](../open-questions.md) — Q-10, Q-37, Q-38, Q-39, Q-40.
- [KB-003](../investigation-registry.md) — INV-012 (closed by this document); INV-015
  (Scheduled, deliberately not run).
- [KB-014](../architecture/multi-tenancy.md), [KB-011](../architecture/backend-architecture.md)
  — reused for tenancy and for the repository / `UnitOfWork` shape; not re-derived.
