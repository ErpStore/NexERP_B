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
last_verified: 2026-08-21
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ No active task — the blocker is now the specifications, not the merge queue

**`M2-C01` was merged to `master` (`2dd4e53`, `--no-ff`) on owner instruction 2026-08-21 and is
`Completed`.** It merged with zero conflicts. Re-verified on merged `master`: `npm run typecheck`
exit 0; `npm run lint` "All files pass linting"; `npm run test:ci` **6 passed / 2 files**
(Vitest 4.1.11); `npm run build` **436.85 kB raw / 104.20 kB transfer**. No .NET source was
touched, so every `V.SMART.*` baseline is unaffected. Nothing was pushed.

### The finding that matters: 25 superseded specifications

`M2-C01`'s merge clears the last *dependency* on the frontend tree — and unblocks **nothing**,
because every task behind it still specifies React:

```
M2-C02  M2-C03  M2-C04  M2-C04-01  M2-C04-02  M2-C04-03
M2-C05  M2-C05-01  M2-C05-02  M2-C05-03  M2-C06  M2-C07
M2-C08  M2-C08-01  M2-C08-02  M2-C08-03  M2-C09  M2-C10  M2-C11
M2-D01  M2-D02  M2-D02-01  M2-D02-02  M2-D02-03  M2-D03
```

Each carries `⛔ STOP — this specification is superseded. Do not implement it as written.`
[`CLAUDE.md`](../../../CLAUDE.md) makes that a **stop-and-report**, never a licence to infer the
missing specification. So the dependency graph now says these are ready and the task files say
they are not — and the task files win.

**`M2-C00` is the precedent for how this gets fixed.** It rewrote KB-050 for Angular *and*
re-specified `M2-C01` in the same change, which is the only reason `M2-C01` was selectable. The
equivalent work for the remaining 25 **does not exist as a task** and needs the owner to
authorise it — as one re-specification task, or several.

### Everything else, and why it is excluded

- **`M2-A02`** — P0, dependency-clear, gated on the unanswered **Q-28** *and* on **R-65**, whose
  two phantom screen names would silently deny every request forever if either were annotated.
- **`M2-A04`** — reads `Blocked` although its only listed prerequisite `M2-A01-02` is `Completed`
  and merged. **The blocking reason is recorded nowhere.** Needs an owner ruling. It also gates
  `M2-C02`, so it matters more than its row suggests.
- **`M0-06`** — `Ready`, P1, but `migration/M0-06-remove-default-admin` already exists (part 5).
- **`M0-11`** — a **`Product Decision`**: owner-only, never self-selectable.
- **`M2-B05`** — `Blocked`; premise falsified at Investigate, awaiting re-specification onto **R-66**.
- **`M2-B12-01`** — `Blocked`, verdict `FAIL`, escalation budget exhausted.
- **`M0-01-03`** — merged, still `Needs Review`, awaiting a **named operator** for runbook §7.

### Branches deliberately left unmerged

- **`migration/M2-A08-row-level-scoping`** — duplicate of the merged `M2-A08`; functionally
  identical `UserRepository.cs` change, no validated `PASS`. Safe to delete.
- **`migration/M2-B12-01-inv-012-numbering`** — `Blocked`, `FAIL`, budget exhausted.

---
## Standing blockers worth reading before picking anything up

- **R-65** (`ScreenCatalogue.cs` — two phantom screen names) blocks `M2-A02`. Owner **Vivek**.
  [`technical-debt-register.md`](../risks/technical-debt-register.md).
- **Q-28** (API-only administrators hold zero `UserRight` rows) also blocks `M2-A02`.
  [`open-questions.md`](../open-questions.md).
- **`M0-01-03`** needs a **named operator** to run runbook §7 (start `V.SMART.Web`, log in, run
  one report, print one document) and sign the drill log — an accountability requirement, not a
  technical one. The two throwaway drill databases are left in place for this. See
  [`tasks/M0-01-03.md`](tasks/M0-01-03.md).
- **`M2-A08`** has two competing branches; the owner needs to pick one before either merges
  (one, from `migration/M2-A08-row-scope-and-account-gates`, is already merged to `master`).
- **`M2-C11`** is gated on **Q-38** (what `M2-C11` is for under ADR-007) — unanswered.
- **`M2-A04`** reads `Blocked` although its only listed prerequisite `M2-A01-02` is `Completed`
  and merged. The blocking reason is not recorded anywhere; needs an owner ruling.
- **Three sibling worktrees may still be live** (`wt-M0-10`, `wt-M2-A08`, `wt-M2-B01`) —
  `git worktree list` belongs in Select alongside the tracker, which cannot see them.

## Also true right now

- **R-51** *(new, M2-C01, 2026-08-21)* — `primeng@22.1.0` ships client-side licence-banner
  enforcement (`showInvalidLicenseBanner()`, closed-mode shadow root) and no licence key is
  configured. Owner decision tracked as **Q-66**. Cheapest to resolve now, while only one
  placeholder Angular screen exists — before the app shell (`M2-C03`) and ~150 later screens
  build on PrimeNG. [`technical-debt-register.md`](../risks/technical-debt-register.md).
- **Q-66** *(new, M2-C01)* — does PrimeNG 22 require a paid licence, and has one been bought?
  [`open-questions.md`](../open-questions.md).
- **R-67** — `SaveCorresFileAsync` (`WebFileUploadService.cs:100-104`) writes a zero-byte file
  and reports success; every Blazor correspondence/drawing upload has been landing empty. Found
  by M2-B06, deliberately left unfixed (out of scope), survivable only because
  `Correspondence.Image` holds a second copy.
- **Q-16** now has a storage half (M2-B06) and an observability half (M2-B11): uploaded files
  and the log sink both currently live on local disk/filesystem with no durability guarantee
  under an unknown deployment topology.
