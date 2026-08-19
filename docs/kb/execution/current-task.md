---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-19
dependencies: [KB-081, KB-082, KB-088]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task: **M0-06** — Remove the seeded default Administrator credential

Full spec: [`tasks/M0-06.md`](tasks/M0-06.md). Type Security, P1, estimate 1 d, Gate G0.
Depends on `M0-12-01` (Hard, `Completed`) and is ordered after `M0-13` (Hard ordering
constraint — both risk touching `ApplicationDbContext.cs` seed data; `M0-13` is now
`Completed` and merged, `3f6dfa8`, so this ordering constraint is satisfied).

**Do NOT implement it yet.** This entry only selects it as the next dependency-ready task; a
future session executes it.

### Why this task, not another

Selection rule: [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).

`M0-09` (fix the two unreachable delete guards, R-08) closed this session at
`Needs Review` (validated `PASS`, implemented on `migration/M0-09-delete-guard-fix`,
`8e3b19d`) — it is **not** a candidate again until reviewed and merged. That leaves `M0-06`
as the only task the `M0-12-01` merge released that is both dependency-ready and not
blocked on a human step: `M0-12-02` and `M0-13` are `Completed`, `M0-09` is `Needs Review`,
and `M0-06` alone remains genuinely `Ready`. No tie-break was needed.

`M0-10` (audit all `CanDelete…Async` guards, INV-025) is **not** ready — it names `M0-09` as
a Hard prerequisite and the selection rule requires that prerequisite to be genuinely
`Completed`, not `Needs Review`. It stays `Blocked` until `M0-09`'s branch is reviewed and
merged.

### What M0-06 does, in brief

`ApplicationDbContext.OnModelCreating` seeds a hard-coded `Administrator` user with a fixed
PBKDF2 hash into **every tenant database** (`ApplicationDbContext.cs:1136-1148`, per R-09 /
KB-060). The task removes that seed from source and replaces it with a bootstrap path that
does not leave a known password in source control — **without locking out any tenant that
has no other administrator account**. Read the full task file before starting: it requires
three deliverables (seed removed, a documented/executed bootstrap or migration path, and an
explicit statement of tenant impact), not just a code deletion.

**Ordering constraint, already satisfied but re-verify at start:** confirm
`ApplicationDbContext.cs`'s current seed section still matches the cited line range before
touching it — `M0-13`'s merge may have shifted nearby lines even though it targeted
`StockManagerService.cs`, not this file.

### What M0-09 left for a future session, carried forward here in case it is relevant

Not part of `M0-06`'s scope, but recorded so nothing is lost: an **unreported instance of
the same compute-one/test-another guard defect** exists at `MfgPoService.cs:613-615`
(`CanSalesOrderItemCancelCheckAsync` — `hasCR` computed, `hasRc` tested), found by the
`M0-09` validator, not fixed, and not `M0-06`'s concern. It is recorded under R-08 in
[`technical-debt-register.md`](risks/technical-debt-register.md) (KB-060) and as a scope
note on `INV-025` in [`investigation-registry.md`](investigation-registry.md) (KB-003), for
`M0-10` to pick up once it runs.

## Most recently closed: `M0-09` — Fix the two unreachable delete guards (R-08)

**`Completed` and merged (`47b2d2e`, 2026-08-19)** on the owner's in-conversation instruction.
Re-verified on `master` after the merge: `dotnet test` **79 passed, 0 failed**;
`dotnet build V.SMART.Api --no-incremental` **0 errors, 6,694 warnings** (baseline 6,695).

> **This released `M0-10`, which is now `Ready` — and it is no longer a speculative sweep.**
> `M0-09` fixed two compute-one/test-another guards; its validator found a **third, unreported
> instance of the identical defect** at `MfgPoService.cs:613-615`
> (`CanSalesOrderItemCancelCheckAsync` computes `hasCR`, tests `hasRc`), correctly left unfixed
> as out of scope. The bug class is confirmed wider than anyone had catalogued, and `M0-10` is
> the audit that finds the rest. Two tasks are now `Ready`: **`M0-06`** (P1, 1 d) and
> **`M0-10`** (P1, 2 d).

Pre-merge record follows. Implemented on `migration/M0-09-delete-guard-fix`
(`8e3b19d`), attempt 1 of 3, 0 escalations. Validator verdict **`PASS`**, `scopeOk: true`,
`failureCategory: none`, every acceptance criterion `MET` — independently re-derived,
including the validator reproducing the pre-fix red state itself in a separate detached
worktree. Two identifier changes only (`MfgPoService.cs:504,525`), no `Message` string or
guard order touched. Suite 73 → 79, all green, run twice. `dotnet build V.SMART.Api`
(CI form): 0 errors, 6,693 warnings (at the 6,695 baseline). KB-030, KB-060, KB-080,
KB-003 all updated in the implementation commit; this close-out additionally recorded a
validator-found lead (`MfgPoService.cs:613-615`) that the implementation commit did not
surface. Full record:
[`tasks/M0-09.md` § Execution Record (2026-08-19)](tasks/M0-09.md#execution-record-2026-08-19),
[`task-tracker.md`](task-tracker.md) footnote 15.

**Not `Completed`** — awaiting the repository owner's review and merge, same standing
convention as every other `PASS`-validated task this milestone
([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)). **Unblocks nothing
yet**: `M0-10` stays `Blocked` until this branch is merged.

## Other open blockers, unaffected by this change

- **`Needs Review`** — implemented, validated, committed on its own branch, awaiting a
  human review-and-merge/sign-off step that no autonomous session may perform on its own
  authority ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)):
  `M0-01-03`, `M0-09`.
- **`Blocked` on an unscheduled human**, not on any task: `M0-04` (unidentified owner —
  tracker footnote 4).
- **`Blocked`, transitively:** `M0-11` (Q-01 product decision, released by `M0-13`'s merge
  but not runner-selectable — needs the owner, not a task), `M0-10` (behind `M0-09`'s
  merge), `M0-05` (behind `M0-04`).
- **A parent container**, never worked directly: `M0-01`, `M0-12`.

Full detail on why each is blocked and who the candidate owner is:
[`runner-state.md`](runner-state.md) (KB-093) § *Blocked on* / *Owner to unblock ...* rows,
and [`task-tracker.md`](task-tracker.md) (KB-081) footnotes 1, 4, 13, 15.

> **This does not open M2.** Gate G0 still has zero of seven exit criteria ticked.
> `M0-01-03`'s rebuild drill, `M0-07`'s CI branch-protection criterion and `M0-04`'s
> credential rotation remain human-owned and unchanged by this session. Even once `M0-06`
> and `M0-09`/`M0-10` land, G0 still needs those three human steps.
