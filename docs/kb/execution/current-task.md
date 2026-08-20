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
last_verified: 2026-08-20
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-C00` — rewrite KB-050 frontend architecture for Angular

**Task file:** [`tasks/M2-C00.md`](tasks/M2-C00.md). **Status:** `Ready`, attempt 0 of 3, no branch yet.

### The frontend framework changed on 2026-08-20 — read this first

[**ADR-007**](../decisions/ADR-007-angular-stack.md) selects **Angular + PrimeNG** and supersedes
`ADR-003`, which chose React. Owner decision: his background is C# and WPF with no frontend
experience, and while the runner writes the screens, **he** reviews and maintains them.

**The finding that reopened it:** ADR-003 never evaluated Angular at all — every rationale it
recorded was a choice *within* React. It was an assumption that acquired the authority of a
decision by being written into an ADR.

**What this means for anyone opening a frontend task:** all 26 `M2-C*`/`M2-D*` task files carry a
**⛔ STOP banner**. They were deliberately *not* rewritten — that is ~25,000 lines of spec with
1,300+ React references, and rewriting it against a KB-050 that does not yet describe Angular
would just produce a second draft to throw away. **If you select a banner'd task, stop and
report.** Re-specifying is an owner-level change, not something to infer mid-implementation.

### Why `M2-C00`, now

**It gates the entire `M2-C` tree — 20 tasks.** KB-050 is the primary input every one of them
cites, and it still describes a React application. Nothing else in the frontend can be specified
honestly until it is rewritten. No other `Ready` candidate comes close on downstream unblocking:
`M2-B12-01` releases one task, `M0-01-03`/`M2-B04`/`M2-B09` release none.

It is **Documentation** type — no code, no build, no test run.

### What it does

Rewrite KB-050 for Angular, starting from the section-by-section map already added to that
document (which sections are dead, which still bind). Rewrite the stack, project structure,
data-fetching, auth flow, permission rendering and **error handling** — that last one predates
`M2-A06`, so the real `application/problem+json` contract in `ApiProblems.cs` is now the source.
Keep the design constraints, the document-editor pattern, workflow commands, performance and
accessibility sections: none was ever a React decision. Re-specify `M2-C01` for Angular in the
same change, since it is next and its banner otherwise blocks it.

### Do not

Write code. Re-decide ADR-007's stack — implement it as recorded; if it is wrong, say so and stop.
Re-specify the rest of the `M2-C` tree — only `M2-C01` is in scope. Delete the React app — that is
the re-scoped `M2-C01`'s call.

---

## Waiting for your review — one unmerged branch

| Task | Branch | State |
|---|---|---|
| **`M2-A01-03`** | `migration/M2-A01-03-rights-cache` | Validated `PASS`, `Needs Review`. Per-request rights caching, tenant-scoped TTL. **Releases `M2-A02`, `M2-A07`, `M2-A08` when merged** — the critical path runs through it |

`M2-A01-03` is the highest-value merge available: it is the only thing standing between the
current state and three released `M2-A` tasks.

## Ready and unclaimed after `M2-C00`

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-B12-01` | INV-012 document numbering | 2 d | Documentation-only; releases `M2-B12-02` |
| `M2-B09` | Reference-data endpoints + caching | 3 d | P1, released by `M2-B02` |
| `M2-B04` | Decouple `IApprovalService` | 1 wk | Zero tracked dependents |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | Carried M0 debt; **no longer hardware-blocked** (footnote ²¹) |
| `M2-C01` | Angular scaffold | 3 d | Blocked on `M2-C00` |

**A session may now run more than one of these.** Changed 2026-08-20: the standing rule is
*"pick the next task that can actually be done"*, with a five-part test in
[`CLAUDE.md`](../../../CLAUDE.md) § Standing constraints. **One task, one branch, cut from
`master`** is unchanged, as is *never merge, never push*.

---

## Carried forward — still relevant (from `M2-A01-02`, merged `ed559ad`)

- **`M2-A01-02` is `Completed` and merged** (`ed559ad`, 2026-08-20) — implemented on
  `migration/M2-A01-02-require-screen-right` (`9a6b3c2`), validated `PASS` on attempt 1 of 3,
  0 escalations. **The D-5/R-40 contradiction was verified as genuinely not-hit at review**, not
  taken on report: `grep` of `V.SMART.Api/Authorization/` for `UserId == 1` / `IsAdmin` /
  `Administrator` / `bypass` / `.Role` returns **zero matches**, and KB-105's D-5 still reads
  *"No `Administrator` bypass. None. Anywhere."* verbatim — the spec was extended, not softened.
  Full record:
  [`tasks/M2-A01-02.md` § Execution Record (2026-08-20)](tasks/M2-A01-02.md#execution-record-2026-08-20)
  and `task-tracker.md` footnote ²⁵. **`M2-A01-03` is now `Ready`.** `M2-A02`,
  `M2-A03`, `M2-A04`, `M2-A07` and `M2-A08` remain `Blocked` behind it.
- **`V.SMART/V.SMART.Api/Authorization/` now exists**, all ten types KB-105 §2 specifies, with
  `Right`, `[RequireScreen]`, `[RequireRight]`, `[NoScreenRight]`, `IUserRightsProvider` (no
  cache), `ScreenRightAuthorizationFilter`, `ScreenRightSet`, `ScreenCatalogue`, and
  `ScreenRightStartupValidator`, registered in `Program.cs`. **No controller is annotated** —
  `M2-A02`'s job. R-03 (KB-060) stays open with that noted.
- **⚠ THE FILTER IS OPT-IN, NOT DENY-BY-DEFAULT — `M2-A02` must close this.** `D-4` is only
  partly implemented: an authenticated action on a controller carrying **no** `[RequireScreen]`
  at all is **allowed through**, at request time and at startup. The reasoning is sound —
  enforcing it now would have made the host refuse to start over `CurrencyController`'s five
  unannotated endpoints, which this task was forbidden to change — and the *half*-annotated
  directions (T-11, T-12) **are** enforced, as is D-6's catalogue check. But the gap is the
  opposite of what "deny by default" implies, so it is stated here rather than left in a
  footnote: **today, an unannotated controller is unprotected.** Tracked against R-03 (KB-060);
  `M2-A02` closes it in the same change that annotates the first controller.
- **A latent, deployment-conditional DI-eagerness finding**, not a regression today, recorded
  for `M2-A02` to watch: the globally registered filter constructs `IUserRightsProvider` (and
  therefore the tenant `DbContext`) via DI on every request reaching MVC's pipeline, even on
  unannotated actions. See `KB-060` R-03 close-out addendum and `task-tracker.md` footnote
  ²⁵ for the detail and why it is safe today.
- **Q-27** (duplicate `(UserId, ScreenId)` `UserRight` rows in a live tenant database) remains
  **Unknown** — `INV-037`'s amendment confirms the 152-screen catalogue matches exactly, but
  the duplicate-row question was not queried in this session's reachable dev tenant.
- **R-40 / D-5 contradiction** (`UserId == 1` auto-granted all 152 rights by
  `Login.razor:345-349`, vs. KB-105 D-5 "no Administrator bypass") was **not hit** by
  `M2-A01-02` — the filter correctly denies a `UserId == 1` caller with zero `UserRight` rows
  (`T13` in the filter's test suite), because the bypass lives in the Blazor login path, not
  in `RightsHelper`/the new filter. Still unresolved for `M2-A02`: an API-only administrator
  will hold zero rows unless `Q-28` (login never calls `SyncRightsForUserAsync` on the API
  path) is settled first. Both remain open questions for `M2-A02`, not this task.

