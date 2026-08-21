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

## ▶ No active task — but the pool is no longer empty

**The merge queue was cleared on owner instruction 2026-08-21.** Eight branches were merged to
`master` with `--no-ff`: `M2-B11` (`955620a`), `M0-01-03` (`1aa1106`), `M0-10` (`843a04e`),
`M2-A07` (`80c209b`), `M2-C00` (`0da6a35`), `M2-B04` (`f054c75`), `M2-A08` (`380c805`) and
`M2-B09` (`501b12d`). Seven are `Completed`; `M0-01-03` stays `Needs Review` because merging a
runbook does not supply the **named operator** its §7 requires.

**Verified on `master` after the last merge**, not assumed: `V.SMART.Api` 0 errors / **6693**
warnings and `V.SMART.Web` 0 / **6697** — both exact baselines; `tests/V.SMART.Api.Tests`
**312 passed / 0 failed**; `tests/V.SMART.Shared.Tests` **90 passed / 1 skipped**, the skip being
`M0-10`'s deliberate characterisation test for the one surviving R-08 defect (un-skipped by
`M0-10a`). Nothing was pushed.

### The next task a runner should select: `M2-C01`

**`M2-C01`** — Angular CLI + TS strict + lint + test + CI, P0, 3 d, gate **G2**,
`depends_on: [M2-C00]`. Its Hard prerequisite is now `Completed` **and merged**, which is what
part 1 of the five-part test actually requires. It is not a `Product Decision`, is not gated on
an unanswered question, its task file carries no ⛔ banner, and no sibling branch is open on it.
Task file: [`tasks/M2-C01.md`](tasks/M2-C01.md).

> ⚠ **Re-specify before implementing.** `M2-C01` was merged once already, on 2026-08-19, as a
> **React/Vite** scaffold (`12f172f`), before [`ADR-007`](../decisions/ADR-007-angular-stack.md)
> selected Angular on 2026-08-20. Footnote ²⁶ re-scoped the row for Angular; the *code* that
> closed it the first time does not apply. Read `ADR-007` and `M2-C00`'s rewritten
> [KB-050](../frontend-new/react-architecture.md) before writing anything.

### Everything else, and why it is still excluded

- **`M2-C11`** — dependency cleared by the same merge, now gated on **Q-38** (what `M2-C11` is
  for under ADR-007). Q-38 was `M2-C00`'s `Q-37`, renumbered at merge because `M2-A07` had
  already claimed that id.
- **`M2-C02`** — needs `M2-C01` and `M2-A04`; neither is `Completed`.
- **`M2-A02`** — P0 and dependency-clear, but gated on the unanswered **Q-28** *and* on **R-65**,
  whose two phantom screen names would silently deny every request forever if either were
  annotated.
- **`M2-A04`** — reads `Blocked` although its only listed prerequisite `M2-A01-02` is `Completed`
  and merged. **The blocking reason is not recorded anywhere**; this needs an owner ruling before
  the row can be trusted either way.
- **`M0-06`** — `Ready`, P1, but `migration/M0-06-remove-default-admin` already exists, so part 5
  of the test excludes it.
- **`M0-11`** — a **`Product Decision`**: owner-only, never self-selectable.
- **`M2-B05`** — `Blocked`; premise falsified at Investigate, awaiting re-specification onto **R-66**.
- **`M2-B12-01`** — `Blocked`, verdict `FAIL`, escalation budget exhausted.
- **`M0-01-03`** — merged, still `Needs Review`, awaiting a named operator for runbook §7.

### Two branches deliberately left unmerged

- **`migration/M2-A08-row-level-scoping`** — a duplicate of the merged `M2-A08`. Its
  `UserRepository.cs` change is *functionally identical* to the merged one (same predicate,
  different comment prose) and it carries no validated `PASS`. Safe to delete; kept pending the
  owner's word.
- **`migration/M2-B12-01-inv-012-numbering`** — `Blocked`, `FAIL`, budget exhausted.

---
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
- **`M2-A08`** has two competing branches; the owner needs to pick one before either merges.
- **Three sibling worktrees may still be live** (`wt-M0-10`, `wt-M2-A08`, `wt-M2-B01`) —
  `git worktree list` belongs in Select alongside the tracker, which cannot see them.

## Also true right now

- **R-67** — `SaveCorresFileAsync` (`WebFileUploadService.cs:100-104`) writes a zero-byte file
  and reports success; every Blazor correspondence/drawing upload has been landing empty. Found
  by M2-B06, deliberately left unfixed (out of scope), survivable only because
  `Correspondence.Image` holds a second copy.
- **Q-16** now has a storage half (M2-B06) and an observability half (M2-B11): uploaded files
  and the log sink both currently live on local disk/filesystem with no durability guarantee
  under an unknown deployment topology.
