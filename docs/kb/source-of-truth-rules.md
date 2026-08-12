---
doc_id: KB-002
title: Source-of-Truth Rules
module: meta
status: active
confidence: n/a
last_verified: 2026-08-12
---

# Source-of-Truth Rules

## 1. Evidence classification

Every factual statement in this knowledge base carries one of these tags:

| Tag | Meaning | Required evidence |
|---|---|---|
| **Confirmed** | Traced directly in current source code | `path/to/file.cs:LINE` or an exact identifier |
| **Inferred** | Reasoned from code structure but not stated anywhere | The code that supports the inference, plus the reasoning step |
| **Unknown** | Not established; needs investigation | Recorded in [`open-questions.md`](open-questions.md) with an INV id |

Never write an inference in a way that reads as a confirmed fact.

## 2. Authority order

When sources disagree, this order decides:

1. **Current source code** — always authoritative.
2. **Database schema / migrations** — authoritative for storage, but note that
   `Migrations/` may lag or contain superseded snapshots.
3. **This knowledge base** — authoritative for interpretation, until code contradicts it.
4. **Pre-existing prose docs** — `docs/ARCHITECTURE.md`,
   `docs/FRONTEND_MIGRATION_ANGULAR_REACT.md`, `docs/ZOHO_UI_REDESIGN_PLAN.md`,
   `docs/UPSKILLING_ROADMAP.md`, `.github/copilot-instructions.md`.
   **Treat as hypotheses, not facts.**

### Known conflicts already found between prose docs and code

| Claim | Source of claim | Reality | Evidence |
|---|---|---|---|
| "`ApplicationDbContext` — all transactional entities **+ Identity**" | `docs/ARCHITECTURE.md` §3 | There is **no ASP.NET Identity schema**. `User` is a plain custom entity; only `IPasswordHasher<User>` is borrowed from Identity | `Data/Master/Admin_Module/User.cs`; `Data/ApplicationDbContext.cs` has no `IdentityDbContext` base |
| "`MasterDbContext` — Master/configuration data context" | `docs/ARCHITECTURE.md` §3 | It contains exactly **one** `DbSet`: `Tenants` | `Data/MasterDbContext.cs` (9 lines) |
| "`Existing Store Procedures/` — legacy SQL Server SPs" implying completeness | `docs/ARCHITECTURE.md` §2 | Only **13** of the **94** procedures referenced in code are present | `Existing Store Procedures/StoredProcedures/` vs code scan |
| Doc is a finished architecture description | reading `docs/ARCHITECTURE.md` | It is a **template with `[TODO: ANALYZE]` placeholders** | `docs/ARCHITECTURE.md` header |

These are recorded, not corrected in place — the old docs are left as-is and superseded
by this knowledge base.

### Conflicts found between **this knowledge base** and code

The rule cuts both ways. When a KB document is contradicted by the repository, the KB is
corrected in place, the retraction is recorded, and `last_verified` is bumped.

| Claim | Source | Reality | Evidence | Corrected |
|---|---|---|---|---|
| Build output (`dist/`, `.angular/cache/`, `.vs/`, `*.csproj.user`) is **committed**; "remove from history" | [KB-060](risks/technical-debt-register.md) R-14, marked *Confirmed* | **Zero** such paths are tracked. All are correctly ignored. The entry conflated *present on disk* with *tracked by git*. The real problem is the inverse: `V.SMART.Api/`, `docs/`, `frontend/`, `.github/` and the build's own `.sln` are **untracked** | `git ls-files` = 2,162 paths, 0 matches for each pattern; `frontend/vsmart-erp/.gitignore:4,10,32`; `.gitignore:9,37` | 2026-08-12, INV-029 |
| "81 of 94 stored procedures have no DDL" | [KB-060](risks/technical-debt-register.md) R-04, [KB-001](00-executive-summary.md), [ADR-005](decisions/ADR-005-reporting-and-printing.md) | The gap is **82**. Only 12 of the 13 `.sql` files map to a called name: `Sp_Print_PurchaseOrder.sql` is dead DDL, while the live `Sp_Print_PurchasePo` has none | `Sp_Print_PurchaseOrder.sql:1` vs `PurchasePoDetails.razor:306`, `PurchPOUpsert.razor:4596`, `Authorization.razor:723` | 2026-08-12, INV-030 |
| "~40 `CanDelete…Async` methods to audit" | [KB-060](risks/technical-debt-register.md) R-08 | **63** implementations under `BusinessLayer/`, and **three are not `Async`-suffixed** — so an audit scoped by the `Async` suffix silently misses them | `PurchaseQuoteService.cs:864`, `SubConDcOutService.cs:2262`, `ProductionLogService.cs:192` | 2026-08-12 |
| Line ranges for the calculation engine and the FIFO defect | [KB-030](business-rules/business-rule-inventory.md), [KB-060](risks/technical-debt-register.md), [KB-011](architecture/backend-architecture.md) | **Every cited range was off by 2–6 lines**, and `CalculationService.cs:12-118` pointed past the end of a 117-line file. The defect itself is confirmed and now pinned exactly: guard `:209-210`, loop `:212-231`, `SaveAsync` `:233`, no check between | re-read of `CalculationService.cs`, `StockManagerService.cs` | 2026-08-12 |
| Numbering queries have "no lock, no `UPDLOCK`, no serializable transaction" | [KB-060](risks/technical-debt-register.md) R-12 | **All 36** such repository files already use `WITH (UPDLOCK, ROWLOCK)`; exactly one site lacks a hint. The race is still real, but because `ROWLOCK` is not a **range** lock and the lock is released at statement end outside a transaction — so the remedy is range locking or an app lock, **not** a stronger row hint. Also "~20 repositories" → 38 sites / 36 files, plus two further mechanisms | `MfgDcRepository.cs:31-36`; the unhinted site at `ProductionIssueAssyRepository.cs:73-81` | 2026-08-12 |
| "Services take/return ViewModels, not entities" | [KB-041](api/api-readiness-assessment.md) | Holds for most, **not all**. 21 of 139 service interfaces expose no ViewModel at any signature; `BankVM`/`StateVM`/`CurrencyTodayVM`/`ProjectTypeMasterVM` do not exist. Generalised from two sampled services | `IBankService.cs:13-15`, `ITermsAndConditionsService.cs:13-16` | 2026-08-12 |
| `MfgPOUpsert.razor` `@code` method line numbers (`AskToShortCloseAsync:979`, `CancelPO:1381`, …) | [KB-030](business-rules/business-rule-inventory.md), planning notes | **Every one was ~2,001 too low** — they are offsets *within* the `@code` block, which starts at `:2002`, not absolute file lines. Absolute: `AskToShortCloseAsync:2980`, `Cancel:3039`, `OnItemCancelChanged:3113`, `HandleModalConfirmation:3234`, `ShortClosePo:3284`, `CancelItem:3314`, `CancelPO:3382` | `grep -n` on the file; `@code {` at `:2002` | 2026-08-12 |
| INV-009's name-extraction command reproduces 94 | [KB-003](investigation-registry.md) INV-009 | Unscoped, it now returns **111** — it matches the `.sql` files and this KB's own prose. **The knowledge base has begun contaminating its own evidence.** The scoped form returns 94 | `grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor --exclude-dir=obj --exclude-dir=bin V.SMART` | 2026-08-12 |

**Why R-14 mattered.** Acting on it as written would have triggered a destructive
git-history rewrite — colliding with M0-05 — to remove files that were never committed. A
*Confirmed* rating is a claim that someone traced it; this one had not been traced.

**Rules now enforced in [KB-083](execution/prompt-template.md) as a result:**

1. Verify before citing, **even when the source is our own knowledge base**. Every one of
   these was found by a task author re-checking a fact they had been handed.
2. Never mark a claim *Confirmed* on inference from a directory listing or a filename.
3. **Re-verify line numbers immediately before citing them.** Ranges drift; a cited range
   that overruns the file is the clearest signal the citation was never re-read. Prefer
   citing a *declaration line* plus a symbol name (`TrackStockUsageAsync`, declared `:177`)
   over a range — declarations move, but the pairing stays checkable.
4. Record the count *and* the command that produced it, scoped so the knowledge base cannot
   match its own prose.

None of these findings changed a conclusion in the analysis — the FIFO defect, the
authorization gap and the stored-procedure gap are all still real, and two got worse. What
changed is that the evidence now survives being checked.

## 3. Separation of as-is and to-be

| Directory | Contains |
|---|---|
| `architecture/`, `modules/`, `business-rules/`, `api/api-overview.md` | **Existing system only.** No proposals. |
| `frontend-new/`, `migration/`, `api/api-readiness-assessment.md` | **Proposals only.** Every proposal states what it depends on from the as-is docs. |
| `decisions/` | Decisions taken, with context and consequences. |

A document must never contain both a description of current behaviour and a proposal for
changed behaviour without an explicit heading separating them.

## 4. Change-control rules for the codebase

1. Do not rewrite the backend because the frontend is being replaced.
2. Preserve existing business logic unless there is a documented reason (an ADR) to
   change it.
3. Keep existing API contracts (`api/api-overview.md`) where practical.
4. Identify breaking changes explicitly **before** implementing them.
5. Before modifying code, state what will change and why.
6. Every business rule recorded here must have traceable source evidence.

## 5. Staleness

A document is **stale** when any of its `source_files` has changed materially since
`last_verified`. Re-verify and bump `last_verified`; if findings changed, note the delta
in the document and update the registry row.
