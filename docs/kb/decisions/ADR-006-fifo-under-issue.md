---
doc_id: ADR-006
title: FIFO stock issue silent under-allocation (R-07) — preserve behaviour, add visibility
module: decisions
status: accepted
confidence: high
last_verified: 2026-08-27
dependencies: [KB-004, KB-060, KB-081]
---

# ADR-006 — FIFO stock issue silent under-allocation (R-07 / Q-01)

**Status:** Accepted · **Date decided:** 2026-08-19 · **Date recorded:** 2026-08-27

> This document is the formal record `M0-11` was scoped to produce. The decision itself was
> made by the repository owner on 2026-08-19, in `open-questions.md` (Q-01), five days before
> this task's own `last_verified` date — the task's original brief assumed the decision was
> still pending. What follows is that already-made decision written up against the measured
> baseline (`M0-13`'s characterisation tests), not a fresh deliberation.

## The question

> **Is the silent stock under-issue (R-07 / BR-STK-002) a bug or relied-upon behaviour?**
> `TrackStockUsageAsync` errors only when *no* batch exists; if batches exist but total
> balance is short, it allocates what it can and returns success, leaving the ledger
> unbalanced.

Asked in [`open-questions.md` Q-01](../open-questions.md), answered by **the repository
owner**, 2026-08-19, in his own words: *"preserve but surface we can plan it after the
Milestone-2 is done."*

## Current behaviour (Confirmed, pinned by M0-13)

Source: `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/StockManagerService.cs`,
re-verified unchanged against the working tree on 2026-08-27 — line numbers match the
`M0-13`/`R-07` citations exactly, no drift since pinning.

| # | Statement | Evidence |
|---|---|---|
| 1 | A `StockIssue` row is created and saved (lines ~153–155) **before** `TrackStockUsageAsync` is called — so the row exists regardless of whether allocation later succeeds, partially succeeds, or throws. | `StockManagerService.cs:153-155`; the throw path in test 2 below only fires *after* this save has already happened. |
| 2 | If **no** batch has `BalQty > 0`, `TrackStockUsageAsync` throws `InvalidOperationException("No available stock to issue.")` (line ~210). | `S13_R07_IssueOrUpdateStock_WhenNoBatchHasBalance_ThrowsNoAvailableStockToIssue` |
| 3 | If batches exist but their total balance is **less** than the requested quantity, the allocation loop (lines ~211–233) consumes every batch it can, then exits normally with `remainingQty > 0` — no exception, and the issue is recorded as fully successful. | `S14_R07_IssueOrUpdateStock_WhenBatchesExistButTotalBalanceIsShort_SilentlyUnderAllocatesAndDoesNotThrow` — drift asserted numerically: `IssueQty 100 − Σ UsedQty 30 == 70m` |
| 4 | The same silent under-allocation happens on the **re-issue / update** path, not only on first create. | `S15_R07_IssueOrUpdateStock_WhenReIssueIncreasesQuantityBeyondAvailableStock_SilentlyUnderAllocatesOnTheUpdatePathToo` — drift `95m` |
| 5 | The boundary is sharp, not proportional: issuing against **zero** batches throws in full; issuing the same quantity against **one** unit of stock succeeds and silently drops the remaining 99%. | `S16_R07_IssuingOneHundred_ThrowsAgainstZeroStock_ButSilentlyDriftsByNinetyNineAgainstOneUnit` |
| 6 | The root cause is structural: there is **no check of `remainingQty > 0` after the allocation loop closes** (between `:231` and `SaveAsync()` at `:233`) that would convert a short allocation into a refusal or a flagged partial success. | Re-verified by direct reading, `StockManagerService.cs:209-233`, 2026-08-27 — this is what statements 3–5 depend on; no dedicated test names this alone, because `S14`/`S15` passing *at all* is the direct consequence of its absence. |

**No change since `M0-13` (2026-08-19).** All four tests (`S13`–`S16`) still describe the
live code path; none were touched by this task, per its own constraint (`git diff --stat`
shows zero files under `tests/`, verified below).

## Drift quantification

**Not run — Unknown, pending DBA/production access.** No production tenant database
connection exists in this project's execution environment (same limitation recorded for
`Q-02`, `Q-03`, and `Q-25`'s "needs production tenant-database access nobody on this project
has"). The characterisation tests quantify drift on synthetic fixtures (`70m` and `95m` in
two scenarios) but say nothing about real tenant data.

The read-only query below answers it, once someone with tenant-database access runs it. It
is derived directly from the entity definitions — `StockIssue.IssueQty`
(`Data/Inventory(Stock)/StockIssue.cs:37`, the requested quantity, saved before allocation
per statement 1) versus the sum of `StockIssueTrack.UsedQty`
(`Data/Inventory(Stock)/StockIssueTrack.cs:30`, what was actually allocated) grouped by
`IssueId`. Table names are EF Core defaults — `StockIssue` and `StockIssueTrack` — confirmed
by `grep` against `ApplicationDbContext.cs`: no `.ToTable(...)` override exists for either
entity.

```sql
-- R-07 / ADR-006 drift quantification. Read-only. Run per tenant database.
-- Every StockIssue row where less was tracked than was requested.
SELECT
    si.IssueId,
    si.IssueDate,
    si.ItemId,
    si.StoreId,
    si.IssueQty                                  AS RequestedQty,
    ISNULL(SUM(sit.UsedQty), 0)                  AS AllocatedQty,
    si.IssueQty - ISNULL(SUM(sit.UsedQty), 0)    AS DriftQty,
    CASE WHEN ISNULL(SUM(sit.UsedQty), 0) = 0
         THEN 'Zero-allocation orphan (statement 1 — row saved, allocation never landed)'
         ELSE 'Silent partial under-allocation (statements 3-5)'
    END                                           AS DriftCategory
FROM StockIssue si
LEFT JOIN StockIssueTrack sit ON sit.IssueId = si.IssueId
GROUP BY si.IssueId, si.IssueDate, si.ItemId, si.StoreId, si.IssueQty
HAVING si.IssueQty > ISNULL(SUM(sit.UsedQty), 0)
ORDER BY DriftQty DESC;
```

**Who must run this:** whoever has direct SQL access to each tenant production database —
nobody on this project's execution side does (same gap as `Q-02`/`Q-03`). Flagged here rather
than guessed at.

## Reliance evidence (searched 2026-08-27; hits and misses both recorded)

The task spec requires searching for, and reporting both hits and misses on: a config flag
governing short issues; a UI warning about insufficient stock; a report reconciling
`StockIssue` against `StockIssueTrack`; a code comment acknowledging the behaviour.

| Searched for | Result |
|---|---|
| Config flag (`AllowOverIssue`, `AllowUnderIssue`, `PermitShortIssue`, `OverIssue`) anywhere in `.cs` | **Miss.** Zero hits — no such flag exists; the behaviour is unconditional. |
| Report reconciling `StockIssue` against `StockIssueTrack` in `Pages/` | **Miss.** Zero hits — no reconciliation UI exists. |
| Developer comment acknowledging the drift near the allocation loop | **Miss.** `StockManagerService.cs` has no comment on the subject beyond the `#region FIFO Stock Tracking` header (`:176`) — the behaviour is undocumented in code. |
| UI warning about insufficient stock in `Pages/` | **Hit (Confirmed), but a distinct, partial mechanism — not evidence the drift itself is acted upon.** `ProductionLogUpsert.razor:1822-1877`, `ValidateAndAdjustQtyAsync`, compares the requested input quantity against `ProductionLogService.GetAvailableStockByItemIdAndRcAndScreenAsync` (`ProductionLogService.cs:119-137`, which sums `StockAdd.BalQty` — the same balance field `TrackStockUsageAsync`'s loop consumes) and, if short, **clamps the input and warns** ("Insufficient RM Stock. Input Qty auto-adjusted to {allowedInputQty}.") **before any call reaches `StockManagerService`.** |

**Why the one hit does not change the drift analysis.** This guard operates on a `SUM(BalQty)`
snapshot taken when the field changes, not atomically with the eventual issue call — a
concurrent issue between the check and the submit can still exhaust the stock this check
saw as sufficient. It also sits in exactly one screen (Daily Production Log); the other
issue call sites this project has already touched for unrelated reasons (`ToolCribReturnService`,
`LabourSCNService`, `PurchaseSCNService`, `SubConSCNService`, `ProductionSCNAssyService`,
`ProductionSCNCompService` — see `M2-B05`) were not found to have an equivalent pre-check, and
were not exhaustively re-audited for one here (out of this task's scope). So: real evidence
that the *product* anticipates insufficient-stock situations and tries to warn the user in at
least one screen — but not evidence of reliance on the silent drift itself, and not a
guarantee the drift cannot still occur even in that one screen.

**Net finding: no evidence, positive or negative, that any tenant relies on the drift.**
This matches Q-01's own recorded rationale — "no evidence exists either way about tenant
reliance on the drift — that was `Unknown` and not determinable from source" — and this
search does not change that. It adds one data point (a partial UI mitigation exists in one
screen) without resolving the underlying unknown.

## Options considered

### Option A — Tighten

Add the missing post-loop check (statement 6): if `remainingQty > 0` after the allocation
loop, refuse the issue (throw, or return a typed failure) instead of silently succeeding.

**Consequences:**
- **Existing data:** no retroactive effect — this only changes behaviour for issues made
  *after* the change. Historical drift (quantified by the SQL query above, once run) stays
  in the ledger regardless.
- **Back-dated entry:** this is the option's real risk. If stock for an already-recorded
  issue is entered *later* (a back-dated `StockAdd`), and the workflow depends on an issue
  being allowed to go through and be topped up afterward, tightening would start refusing
  transactions that succeed today. Whether this workflow exists is itself part of the
  "reliance" unknown above — not determinable from source.
- **The API:** the API (per `M2` work already in flight) reproduces `StockManagerService`
  as-is; tightening here would need to land in the shared service so both Blazor and the API
  inherit it identically, per `R-07`'s own stated action ("fix in the service so both UIs
  benefit").
- **Migration:** requires updating `S14`, `S15`, `S16` in the same commit — they currently
  assert the *drift itself* as their expected outcome, so tightening flips all three from
  passing to correctly red until rewritten to assert the new refusal.

### Option B — Preserve, but surface *(chosen)*

Keep the allocation behaviour exactly as measured — a short issue still succeeds and still
allocates whatever is available. Change only what happens to the shortfall: instead of being
silent, it is **returned to the caller and surfaced in the UI/API response** (e.g. an issue
that requested 100 and could only allocate 30 reports back "issued 30 of 100 requested" as
part of a successful response, rather than reporting plain, undifferentiated success).

This is Option B *with visibility*, addressing the task's own requirement that Option B
"addresses visibility, not merely leave it as it is."

**Consequences:**
- **Existing data:** none — no behavioural change to allocation itself, so no existing
  transaction is affected differently than before.
- **Back-dated entry:** unaffected — nothing about the accept/refuse decision changes, so
  the legitimate back-dated-topup workflow (if it exists) keeps working exactly as it does
  today.
- **The API:** must carry the shortfall in its response contract for the issue endpoint(s) —
  a concrete, scoped piece of API design work, not yet done (see "what happens next" below).
- **Migration:** `S13`–`S16` need **no** behavioural change and stay green as-is (they assert
  the allocation outcome, not the caller-visible response shape) — but should gain an
  assertion that the shortfall value is now correctly reported once the surfacing work lands,
  so the visibility guarantee itself is pinned, not just the allocation math.

### Variants noted, not promoted

- **Partial-refusal hybrid** (refuse if the shortfall exceeds some threshold, otherwise
  allocate-and-surface): not evaluated — no business rule establishes any threshold, and
  inventing one would violate the standing "never invent business rules" constraint.
  Mentioned only because it is a real point in the design space; it is not evaluated further.
- **Reservation/hold model** (reserve stock at request time, refuse only if reservation
  fails): a materially larger change than either option above — touches concurrency
  behaviour, not just the failure/response path — out of scope for a decision brief and not
  something the owner was asked to weigh.

## Recommendation (as originally prepared, before the owner's answer)

The brief this task was built to produce would have recommended **Option B**, for the reason
the owner's own rationale independently arrived at: tightening risks breaking a workflow this
project cannot rule out (back-dated entry) based on **Unknown** reliance evidence in either
direction, while preserving-with-visibility fixes the actual complaint — silence — without
that risk.

**The strongest argument for Option A, stated fairly:** a ledger that can silently drift is a
correctness defect, not a feature, and "preserve but surface" still leaves every existing
drifted transaction in the ledger unexplained after the fact — visibility at issue-time helps
future transactions, but does nothing for the historical gap the drift-quantification query
above may reveal. If that query, once run, shows material real-world drift, the case for
eventually tightening (with a proper data-migration/reconciliation pass, not attempted here)
gets stronger than this brief can currently argue.

## Decision

| Field | Value |
|---|---|
| **Decision** | **Option B — Preserve the allocation behaviour, add visibility into the shortfall.** |
| **Decided by** | Repository owner (Kumar) |
| **Date** | 2026-08-19 |
| **Rationale (owner's words)** | *"preserve but surface we can plan it after the Milestone-2 is done"* |
| **Recorded rationale (`Q-01`)** | Tightening risks blocking legitimate back-dated entry where stock is recorded after the issue; no evidence exists either way about tenant reliance on the drift. Preserving keeps every existing workflow working; surfacing stops the drift being invisible. |
| **Scope split** | The *decision* closes G0 criterion 7, now, via this document. The *implementation* of surfacing is **explicitly deferred until after Milestone 2** — no task id exists for it yet. |

## What happens next

- **This task (`M0-11`) closes** with this document as its deliverable. It does not implement
  surfacing — that is out of scope by the owner's own scope split above.
- **A new task, not yet created**, is needed after Milestone 2 to implement the surfacing:
  it must (a) return the shortfall (`RequestedQty − AllocatedQty`) from the issue
  operation(s) in both `StockManagerService`'s callers and the eventual API contract, and
  (b) decide what to do about the **orphan `StockIssue` row** noted in statement 1 — a
  refused issue (zero batches) still writes a full-quantity row today, so "refused" is not
  clean even under Option A, and Option B doesn't touch this either; it stays exactly as
  measured until that future task addresses it.
- **`R-07` stays open** in the technical-debt register — this decision does not resolve the
  drift, it decides to keep it and stop hiding it. It closes only once the deferred
  surfacing task lands.
- **`S13`–`S16` are not modified by this task** and continue to pin today's allocation
  behaviour exactly. They will need a new assertion (not a rewrite) when the surfacing task
  lands, to pin the caller-visible shortfall value as well.
- **If the drift-quantification query, once run, shows material real production drift**, that
  is new information the owner did not have on 2026-08-19 and may warrant revisiting this
  decision — this document does not preclude that; it records what was decided with the
  information available at the time.
