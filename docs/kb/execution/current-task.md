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

## ▶ No active task — the pool is empty pending human review

**`M2-C01`** (Angular workspace + TypeScript strict + lint + test + CI) was implemented and
independently validated `PASS` on `migration/M2-C01-angular-workspace` (`67410b0`,
2026-08-21), attempt 1 of 3, 0 escalations. It closed **`Needs Review`**, not `Completed` —
only the repository owner may set a task `Completed` (KB-088 § Who may set COMPLETED), and
this branch is not merged. Full record:
[`tasks/M2-C01.md` § Execution Record (2026-08-21)](tasks/M2-C01.md#execution-record-2026-08-21),
tracker footnote ⁴⁰.

**This releases nothing.** `M2-C04-01`, `M2-C10`, `M2-C02` and the rest of the `M2-C` tree all
list `M2-C01` as a Hard prerequisite and need it `Completed` **and merged**
([KB-082](dependency-graph.md#ready-task-selection-rule) step 1) — `Needs Review` does not
satisfy that, the same rule already applied to `M2-C00` under tracker footnote ³⁸.

**No task in the tracker currently passes the five-part "can actually be done" test**
(`CLAUDE.md` § Standing constraints). The three remaining `Ready` rows are each excluded for a
recorded reason:

- **`M0-06`** (Remove the seeded default Administrator credential, P1) — `Ready`, but
  `migration/M0-06-remove-default-admin` already exists, so part 5 of the test (no sibling
  branch open on the same files) excludes it.
- **`M0-11`** (Product decision — silent FIFO under-issue, Q-01) — a **`Product Decision`**,
  owner-only, never self-selectable.
- **`M2-A02`** (Apply row-scope to `CurrencyController` + denial tests, P0) — dependency-clear,
  but gated on the unanswered **Q-28** (API-only administrators hold zero `UserRight` rows)
  *and* on **R-65** (two phantom screen names in `ScreenCatalogue.cs` that would silently deny
  every request forever if either were annotated).

**A run resumes the moment a human reviews and merges one of the `Needs Review` branches.**
`M2-C01` is the one to prioritise — it gates the entire `M2-C` frontend tree (`M2-C02`
through `M2-D01` and beyond) and nothing downstream of it can start until it lands on
`master`. The other unmerged `Needs Review`/validated branches, oldest first: `M0-01-03`
(needs a named operator for runbook §7, not a technical blocker), `M2-B04`, `M2-A08` (two
competing branches — the owner must pick one), `M2-B09`, `M2-B06`, `M2-B11`.

### Two branches deliberately left unmerged (unrelated to review)

- **`migration/M2-A08-row-level-scoping`** — a duplicate of the separately-merged `M2-A08`
  work now on `master` (`380c805`, from `migration/M2-A08-row-scope-and-account-gates`). Its
  `UserRepository.cs` change is *functionally identical* (same predicate, different comment
  prose) and it carries no validated `PASS`. Safe to delete; kept pending the owner's word.
- **`migration/M2-B12-01-inv-012-numbering`** — `Blocked`, verdict `FAIL`, escalation budget
  exhausted.

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
