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
last_verified: 2026-08-28
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a _pointer plus the minimum
> needed to start_, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Selected: `M2-C08-01` — Document editor shell (header + lines + totals + commands)

Full spec: [`tasks/M2-C08-01.md`](tasks/M2-C08-01.md). **Run State: `BLOCKED`, 2026-08-28.**
Not a fresh selection — implemented on `migration/M2-C08-01-document-editor-layout` (tip
`c5c1b77`), independently validated, diagnosed, and closed out, all in the same task's
lifecycle. **Do not re-dispatch an implementer** — there is no code defect to fix, only an
owner decision pending. **Do not select a new task from this pointer** — this session's
instruction was to record the outcome only.

**What happened, briefly** (full detail: [`tasks/M2-C08-01.md`](tasks/M2-C08-01.md)'s
Execution Record, [`task-tracker.md`](task-tracker.md) footnote ¹²⁰,
[`failure-log.md`](failure-log.md) §§ "M2-C08-01 · attempt 1 · independent validation" and
"· diagnosis"): the shell, config contract and both siblings' slots were implemented and
independently validated against every acceptance criterion. **Every criterion passed except
one** — `npm run format:check` fails, but on two files (`eslint.config.js`,
`no-float-money.spec.ts`) that are byte-identical to `master` and already unformatted there
(`1c93bb3`), outside this task's authorised file surface. `master`'s own blocking CI
Format-check job is red for the same reason. One small in-scope defect the validator flagged
(a wrong `doc_id` citation, `INV-062` → `INV-065`) was fixed and committed (`c5c1b77`).

**Blocked on an owner decision, recorded as [`Q-105`](../open-questions.md).** Three options,
none an execution session's to choose: (a) a standalone `npm run format` hygiene branch merged
to `master` — recommended, clears the gate for every unmerged branch at once, including
`M2-D01`; (b) an explicit R-82 exception scoping the criterion to files this task actually
touched; (c) folding the two-file reformat into this branch as an out-of-scope commit. **Named
owner: Vivek** (repository owner) — per CLAUDE.md only he may authorise a merge to `master` or
an acceptance-criterion exception. Attempts used: 1 of 3. Escalations: 0 — KB-091 §8 treats
this stop as a successful outcome; a retry cannot change an owner-scope decision.

**Downstream not released.** `M2-C08-02`/`M2-C08-03` still need `M2-C08-01` `Completed` **and
merged**, not merely implemented — both stay `Blocked` until `Q-105` resolves and the branch
merges.

**What a resuming session should do:** if the owner has answered `Q-105`, act on the chosen
option, then re-validate `npm run format:check` alone before touching anything else — every
other criterion is already proven met and does not need re-running unless the merge/rebase
changes something. Do not re-run the layout survey (INV-065, already recorded) or
re-investigate the implementation.

---

## Superseded pointer, retained for lineage — `M2-D01` implemented, **`Completed`** but unmerged (2026-08-28, branch tip `0762e6b`)

> **Updated 2026-08-28:** the owner set `M2-D01` `Completed` in conversation ("mark M2-D01 as
> completed") — [`task-tracker.md`](task-tracker.md) footnote ¹¹⁹, which is also the entry
> this file and [`runner-state.md`](runner-state.md) already cited before it existed. **The
> branch is still unmerged**, so `M2-D02`/`M2-D02-01` stay blocked: rule 1 of the five-part
> test needs `Completed` *and merged*. The `Needs Review` wording below is the record as
> written at the time and is left intact.

Full spec: [`tasks/M2-D01.md`](tasks/M2-D01.md). **Implemented 2026-08-28, branch
`migration/M2-D01-currency-end-to-end`, left `Needs Review`.** Resumed from the prior
`Blocked` attempt (footnote ⁷⁸) — rebased onto current `master` (merged in, keeping the
branch's own history) before any feature code was written, since `M2-C02`, the specific
blocker that stopped that attempt, is now `Completed` and merged.

**What was built, briefly** (full detail: [`task-tracker.md`](task-tracker.md) footnote
¹¹⁹, [`tasks/M2-D01.md`](tasks/M2-D01.md)'s Close-out): the first vertical slice — full CRUD
against the real `/api/v1/currencies` contract, the KB-052/KB-053 drawer-vs-routes conflict
resolved by a route-addressable drawer, permission-gated throughout, server-paged grid with
Excel export. 706/706 unit tests + 4/4 e2e, both builds clean.

**Real findings, not anticipated by the task file:** R-80 — `deleteCurrency()`'s generated
client loses a 409's `title` because its `responseType` (set for the empty `204` success
body) also governs how the error body parses, reported rather than special-cased locally; a
genuine architectural tension between KB-050's pre-`DataGridQueryState` service shape and
the mechanism that now exists, resolved and documented in KB-050's own new Slice review
section; several measured Playwright/PrimeNG interaction quirks, worked around and
documented in place.

**Disclosed, not silently done:** the delete-refusal message is not byte-identical today
(R-80); the rights-less-caller 403 is proven client-side only (no live backend in this
environment); the Blazor screens were not manually re-verified, same reason. **Superseded
above** — `M2-C08-01` selected next.

---

## Superseded pointer, retained for lineage — `M2-D01` selected, not yet dispatched (2026-08-27, master tip `5f83a88`)

Full spec: [`tasks/M2-D01.md`](tasks/M2-D01.md). Owner reviewed
[`tasks/M2-C03.md`](tasks/M2-C03.md)'s Close-out in-conversation and set it `Completed`
("yes mark it as completed"), which cleared the one remaining gap in `M2-D01`'s own
"transitively required" table (footnote ¹¹⁷'s `M2-C03` row) — `M2-A05`, the table's other
gap, had already cleared the pass before. Both `M2-D01` and `M2-C08-01` moved `Blocked` →
`Ready` on the same decision; ranked per [`dependency-graph.md`](dependency-graph.md) §
*Selecting the next task*: both `P0` (tie), `M2-D01` wins on downstream unblocking
(`M2-D02`, `M2-D02-01` both name it directly; nothing names `M2-C08-01`) and independently
sits on the project critical path (`dependency-graph.md:212`), unlike `M2-C08-01`. Full
detail: [`task-tracker.md`](task-tracker.md) footnote ¹¹⁸. **Superseded above** — dispatched
and implemented in the same continuation.

`M2-C08-01` stays `Ready` and is the next candidate after `M2-D01`, unless a higher-priority
row appears first.

---

## Superseded pointer, retained for lineage — no task selected, 2026-08-27 select-only pass, master tip `9187abb`

**Triggered by `M2-A05`/`M2-A11` closing**, per the standing instruction to pick the next
dispatchable task when one closes. Exactly one tracker row read `Ready`: `M2-D01` —
`depends_on` cleared, but its own stricter "transitively required" table still named
`M2-C03`, `Needs Review`, not `Completed`. Not dispatched; row corrected to `Blocked`. Full
detail: [`task-tracker.md`](task-tracker.md) footnote ¹¹⁷. **Superseded above** — the owner
reviewed `M2-C03`'s Close-out and set it `Completed` in the same conversation, clearing this
gap; see the current `## Selected` section.

---

## Superseded pointer, retained for lineage — `M2-A05`/`M2-A11` `Completed`, merged (2026-08-27, master tip `9187abb`)

Full spec: [`tasks/M2-A05.md`](tasks/M2-A05.md). **Implemented, 2026-08-27, branch
`migration/M2-A05-tenant-resolution-cors` (stacked on the unmerged `M2-A11` re-specification
this task depends on), left `Needs Review`.** Dispatched directly off the pass below: Q-16
was explicitly deferred (owner asked directly, answered "I don't know"), which cleared
`M2-A05`'s own Prerequisites; the owner then instructed implementation directly ("go ahead").

**What was built, briefly** (full detail: [`task-tracker.md`](task-tracker.md) footnote
¹¹⁴, [`tasks/M2-A05.md`](tasks/M2-A05.md)'s Close-out): tenant bound at
login/refresh/logout, exactly ADR-002 §5's `{ tenant, username, password }`; the real
architectural fix this task turned out to need — `AuthController` no longer
constructor-injects the tenant-scoped services (`IUnitOfWork`/`IRefreshTokenService`/
`IUserRightService`), resolving them from `IServiceProvider` only after tenant binding,
since ASP.NET Core builds a controller's constructor dependencies before it model-binds the
request body; real per-environment CORS, empty/fails-closed by default; the API's
dev-tenant-pinning `tenant.json` deleted; the Angular client regenerated and the login
form/`TokenStore`/`AuthService` extended to collect and resend the tenant. 619/619 +
685/685 tests, both `dotnet build`s clean, `npm run build`/`e2e` clean.

**Real findings surfaced during implementation, not anticipated by either version of the
task file:** the DI-ordering mechanism above; `Refresh`/`Logout` needed the same `tenant`
field `Login`'s spec named, since `IRefreshTokenService` is equally tenant-scoped; the
pre-existing host-based fallback for those two endpoints never actually worked for a
cross-origin SPA, despite its own doc comments describing it as live.

**Disclosed, not silently done:** Q-16 stays deferred, so only the CORS mechanism ships,
no real origins; no live-backend e2e (this environment has no populated
`Jwt:Secret`/`ConnectionStrings:MasterDb`); `AllowCredentials: false` decided but unexercised.

**Merged to `master` `0d313d5` 2026-08-27** on owner instruction ("merge"), carrying
`M2-A11`'s stacked commits in with it — clean merge, no conflicts. Post-merge
re-verification reproduced every number above on the merged tip. Full detail:
[`task-tracker.md`](task-tracker.md) footnote ¹¹⁵.

**Marked `Completed` 2026-08-27 on explicit owner instruction** ("okay mark it as
completed", applied to both `M2-A05` and `M2-A11`). Full detail:
[`task-tracker.md`](task-tracker.md) footnote ¹¹⁶.

---

## Superseded pointer, retained for lineage — `M2-A11` implemented, `Needs Review`, unmerged (2026-08-27)

Full spec: [`tasks/M2-A11.md`](tasks/M2-A11.md). **Implemented, 2026-08-27, branch
`migration/M2-A11-respec-M2-A05`, left `Needs Review`.** Following on directly from the
select-only pass below (kept for lineage), which found `M2-A05` mechanically dependency-ready
but never re-specified for Angular — this is the one documentation-only, ungated action that
pass surfaced as genuinely doable without an owner decision: unlike Q-16 (deployment topology)
or `M2-C03`'s `Completed` sign-off, fixing a stale task specification needs no one's approval
to attempt, only care in the doing.

**What changed, briefly** (full detail: [`task-tracker.md`](task-tracker.md) footnote ¹¹²,
[`tasks/M2-A11.md`](tasks/M2-A11.md)'s Execution Record): every "React app"/"React SPA"
phrase corrected; the false claim that `M2-C02` "implements the tenant picker" corrected
against what `M2-C02` actually built (it deliberately does not, by design, per its own doc
comments); every route corrected from `/api/auth/…` to `/api/v1/auth/…` now that `M2-B01` has
landed; `M2-A06`'s already-landed `ProblemDetails` conversion reflected instead of described
as pending; `## React Changes` renamed to `## Frontend Changes` and rewritten; a missing
`## Completion Conditions` section added; the stale `## Fresh-Session Execution Prompt` block
removed.

**This does not make `M2-A05` dispatchable.** `M2-A05.md`'s own Prerequisites still require
Q-16 answered or explicitly deferred with a recorded reason — that has not happened. The
tracker row for `M2-A05` is corrected from a bare, unexplained `Blocked` to name this
precisely, so whoever picks it up next knows exactly what is still needed: an owner/ops
answer to Q-16, or an explicit "ship the mechanism only" deferral.

---

## Superseded pointer, retained for lineage — no task selected, 2026-08-27 select-only pass, master tip `f235aeb`

**Every candidate checked past its tracker `Ready` tag failed a real check.** Full detail:
[`task-tracker.md`](task-tracker.md) footnotes ¹¹⁰–¹¹¹, [`runner-state.md`](runner-state.md)
(KB-093).

- **`M2-C08-01`** read `Ready` on the tracker but its own frontmatter `depends_on` (corrected
  by `Q-102`) names `M2-C03` as Hard, and `M2-C03` is `Needs Review`, not `Completed`.
  Corrected to `Blocked` on the tracker.
- **`M2-D01`** genuinely clears rule 1 (all seven Hard deps `Completed` and merged), but its
  own task file's "transitively required" table — which it explicitly says to check, "because
  each is a silent blocker" — names **`M2-A05`**, which is `Blocked`/`Not Started`. Verified
  concretely, not just by tag: `Program.cs:166-169` hardcodes CORS to
  `http://localhost:4200`; the Angular e2e dev server runs at a different origin,
  `127.0.0.1:4300`. A real live-backend login/e2e pass — which `M2-D01` requires — is
  genuinely blocked, and fixing CORS is outside `M2-D01`'s own declared scope.
- **`M2-A05`** was checked next and mechanically clears the five-part test, but was **not**
  dispatched: its task file still says "the React app" throughout and carries a whole _React
  Changes_ section — stale in the sense CLAUDE.md warns about explicitly, never re-specified
  for Angular the way `M2-C03`/`M2-D01`/`M2-C08-01` were. It also names an unanswered blocking
  question (**Q-16** — deployment topology / CORS origins) with no entry in
  `open-questions.md`, and touches `TenantProvider.cs`, shared by two **live** hosts.

**Requires human decision** (see `runner-state.md` for the full list): review `M2-C03`'s
Close-out and set `Completed` or send back changes; answer or explicitly defer Q-16; and
someone should re-specify `M2-A05` for Angular before it is dispatched.

---

## Superseded pointer, retained for lineage — `M2-C03` merged, `Needs Review` (2026-08-27, master tip `73e91e8`)

Full spec: [`tasks/M2-C03.md`](tasks/M2-C03.md). **Implemented and merged to `master`
(`73e91e8`), 2026-08-27 — status `Needs Review`, not `Completed` (owner sign-off pending).**
Branch `migration/M2-C03-app-shell`. See [`tasks/M2-C03.md`](tasks/M2-C03.md)'s Close-out and
Merge sections, and [`task-tracker.md`](task-tracker.md) footnotes ¹⁰⁸–¹⁰⁹, for the full
build, verification, and post-merge re-verification record. **Correction:** the claim
originally made here — "nothing currently names `M2-C03` in any task's `depends_on`" — was
wrong; see the pass above. Per [KB-088](workflow.md#who-may-set-completed), the next step is
the owner's — review the Close-out and either confirm `Completed` or ask for changes.

**Owner override, 2026-08-27: `M2-C03` explicitly chosen over the ranking below.** This pass's
own five-part test and downstream-unblocking ranking (kept below for the record) put `M2-D01`
ahead of `M2-C03`. Presented with both `Ready` options, the owner picked `M2-C03` directly
("Pick M2-C03") — an explicit, in-conversation instruction that overrides the ranking
heuristic, which exists to pick _a_ defensible option absent one, not to bind the owner's own
choice. `M2-C03` independently clears every part of the five-part test on its own merits (see
below); the owner's reason for preferring it over `M2-D01` was not asked for and is not
assumed here. `M2-D01` remains `Ready` and unstarted, its own pre-existing unmerged branch
(`migration/M2-D01-currency-end-to-end`) still unresolved — both carried forward for whoever
picks up that task next.

### How this pass found it, 2026-08-27, master tip `3a30401`

`M2-C02` (the prior selection below) was implemented, merged (`6298732`) and marked
`Completed` this session, both on explicit owner instruction ("Yeah merged", then separately
"Yes complete it"; see [`task-tracker.md`](task-tracker.md) footnotes ¹⁰⁶–¹⁰⁷). That clears
rule 1 of the five-part test for two direct dependents at once:

- **`M2-C03`** — Hard prerequisites `M2-C02`, `M2-C04-01` both `Completed` and merged.
- **`M2-D01`** — all seven Hard prerequisites (`M2-C05-03`, `M2-A02`, `M2-B10`, `M2-C02`,
  `M2-A07`, `M2-A06`, `M2-B01`) `Completed` and merged.

Both clear the rest of the five-part test (neither is `Product Decision`; `open-questions.md`
has no hit for either; neither task file carries a ⛔ banner — `M2-D01`'s was re-specified for
Angular by `M2-C12-05`, `M2-C03`'s by `M2-C12-02`).

**Ranked `M2-D01` ahead of `M2-C03`** on step 2 of the Ready-task-selection rule (most
downstream unblocking): `M2-D01` directly gates `M2-D02-01` (`task-tracker.md:182`) and,
transitively, the rest of the `M2-D02*`/`M2-D03` chain — the actual document-editor/Sales
Order spine this migration is building toward. **`M2-C03` currently gates nothing in any
task's `depends_on`** — grepped `task-tracker.md` for every row naming `M2-C03` as a
dependency and found none, confirming the earlier finding at `task-tracker.md:3286` ("`M2-C03`
was cited as blocking `M2-D01` but appears in no Dependencies table") still holds. `M2-C03` is
not excluded — it stays the next candidate after `M2-D01`, just ranked second, and nothing
currently needs it to unblock.

**One thing to check before dispatching, not yet done by this pass:** `M2-D01` was already
dispatched once before, on `migration/M2-D01-currency-end-to-end`, and stopped `Blocked` on
arrival when its `depends_on` was found incomplete (footnote ⁷⁸). That branch still exists,
unmerged. Before cutting a fresh branch per the "one task, one branch" rule, the next session
picking this up should check `git log migration/M2-D01-currency-end-to-end` for how much real
implementation (if any) that branch holds versus just the investigation that found the
`depends_on` gap — reusing it may be more honest than abandoning real work, but re-cutting
fresh is also legitimate if it never got past investigation. Not resolved here because this
pass's instruction was to close `M2-C02`, not to dispatch the next task.

**Downstream not yet released:** `M2-D02-01` still needs `M2-D01` itself `Completed`, not
`Ready`.

---

## Superseded pointer, retained for lineage — `M2-C02` closed (2026-08-27, master tip `3a30401`)

Full spec: [`tasks/M2-C02.md`](tasks/M2-C02.md). **Superseded — implemented, merged
(`6298732`) and marked `Completed`** on owner instruction, both 2026-08-27. Original selection
narrative below, retained for lineage only:

### How this pass found it, 2026-08-27, master tip `e96e333`

`M2-A04` and `M2-C07` were both merged this session (`72a5758`) and then marked `Completed`
on explicit owner instruction ("looks fine ... move to next task"; see
[`task-tracker.md`](task-tracker.md) footnote ¹⁰⁴). That genuinely clears rule 1 of the
five-part test (`Completed` **and** merged, not merely `Ready`/`Needs Review`) for two direct
dependents at once:

- **`M2-C02`** — Hard prerequisites `M2-C01`, `M2-A04`, `M2-A07` are all now `Completed`.
- **`M2-C08-01`** — Hard prerequisite `M2-C07` is now `Completed`. (`M2-C08` itself, the
  parent, stays `Not Started` — parents are never worked directly, so never `Ready`.)

Both cleared the rest of the five-part test (neither is a `Product Decision`; neither is
blocked on an unanswered `Q-nn`; neither task file carries a ⛔ banner; no sibling branch is
open on either's files). **Ranked `M2-C02` ahead of `M2-C08-01`** on step 2 of the
Ready-task-selection rule (most downstream unblocking): `M2-C02` gates `M2-C03`, `M2-D01`
and transitively the whole `M2-D02*`/`M2-D03` chain, plus it was already named as the real
blocker behind `M2-C05-02`'s own close-out — a materially larger release than `M2-C08-01`,
which only gates its two `M2-C08-02`/`-03` siblings. Also on the critical path (KB-082): the
document-editor/M3-5 Sales Order spine needs the auth slice before it needs the document
shell. `M2-C08-01` remains the next candidate after `M2-C02` closes, not excluded — just
ranked second.

**Downstream not yet released:** `M2-C03` still needs `M2-C02` itself `Completed`, not
`Ready`; same for `M2-D01`. `M2-C08-02`/`-03` need `M2-C08-01` `Completed`, not `Ready`.

---

## Superseded pointer, retained for lineage — no task selected (2026-08-26, master tip `ab9a348`)

**This pointer is stale as of this session and has been corrected.** The prior text below
("Selected: `M2-C05-02`") described a pass at master tip `2281740`. Master has since advanced
through `4a8b61a` (that `M2-C05-02` selection recorded), `c0341db` (dispatch found `M2-C05-02`
itself `Blocked` — its corrected `depends_on` names `M2-C02`, which is `Blocked`, plus two
implementation-time findings: the needed endpoint pair does not exist and no real fixture
capture was possible), `5f23c51` (a select-only pass confirming nothing clears the five-part
test), and `ab9a348` (owner-instructed Q-102 fix: added missing Hard `depends_on` entries to
33 task files; the commit's own message confirms "No task changed status" and every task that
gained a new not-`Completed` dependency was already `Blocked`).

This session re-verified rather than trusted that chain: `git status --porcelain --branch` on
`master` is clean; `task-tracker.md` still shows exactly two `**Ready**` rows, unchanged —
`M0-06` (line 84) and `M0-11` (line 86) — and both still fail the five-part "can actually be
done" test:

1. **`M0-06`** — fails part 5: `git log --oneline master..migration/M0-06-remove-default-admin`
   still shows the branch's unmerged tip `5c9b34c`, closed `Blocked` on `Q-25`/`Q-26`.
2. **`M0-11`** — fails part 2: `task_type: Product Decision` (`Q-01`), owner-only.

No other tracker row reads `Ready`. **`nextTaskId` is empty.** See
[`runner-state.md`](runner-state.md) (KB-093) for the full stop record and the standing human
decisions that would unblock the tree (`M2-C02` unblock, `Q-25`/`Q-26`, `Q-01`, and the
`M0-04`/`M0-06` branch merge-or-reject decisions).

--- prior pointer, superseded, retained for lineage ---

## Selected: `M2-C05-02` — Column preferences + persistence

Full spec: [`tasks/M2-C05-02.md`](tasks/M2-C05-02.md). **Superseded — dispatched and closed
`Blocked`** (`c0341db`; its corrected `depends_on` names `M2-C02`, itself `Blocked`). Do not
re-dispatch until `M2-C02` completes and merges. Original selection narrative below, retained
for lineage only:

### How this pass found it (select-only, this session)

Starting point: `master` tip `2281740`, tree clean (`git status --porcelain` empty,
`git branch --show-current` = `master`). This is two commits past the pointer inherited from
the prior pass (`39a9e11`, which had selected `M2-D01`): `e978c12` recorded that selection, and
`2281740` ("Correct `M2-D01`'s `depends_on` on master — 3 of 7 Hard deps were listed") is on
master.

The inherited `current-task.md` pointer ("`M2-D01`, attempt 0, not yet dispatched") was stale.
`git log --all` shows `M2-D01` was in fact dispatched, on its own branch
(`migration/M2-D01-currency-end-to-end`), and stopped on arrival: its own _Dependencies_ table
(`tasks/M2-D01.md:244-250`) declares seven Hard rows, but `depends_on` had listed only three
(`M2-C05-03`, `M2-A02`, `M2-B10`), all `Completed`/merged, which is why the prior pass's
five-part test passed it. The other four — `M2-C02`, `M2-A07`, `M2-A06`, `M2-B01` — were never
checked; three are `Completed` but **`M2-C02` is `Blocked`**, and it supplies
`PermissionService`, `requireScreen()` and `*appHasRight`, verified absent on disk
(`frontend/nexgen-web/src/app/core/auth/`, `core/http/`, `layout/shell/` each hold only a
`.gitkeep`). That branch closed `Blocked` (`99885fe`, footnotes ⁷⁸/⁷⁹) but is **unmerged**, so
master's `task-tracker.md` row for `M2-D01` still read `Ready` until this pass. Master's own
`2281740` independently confirms the same finding by correcting `depends_on` to all seven Hard
rows — with the corrected list, `M2-D01` now fails part 1 of the five-part test
(`M2-C02` not `Completed`), so it is excluded without needing to merge the close-out branch.

Re-ran the five-part test against every row `task-tracker.md` marks `Ready`:

1. **`M0-06`** — fails part 5: sibling branch `migration/M0-06-remove-default-admin` is already
   open (unmerged), and itself closed `Blocked` on `Q-25`/`Q-26` (`5c9b34c`).
2. **`M0-11`** — fails part 2: `Product Decision` (`Q-01`), owner-only.
3. **`M2-D01`** — fails part 1: corrected `depends_on` now names `M2-C02`, which is `Blocked`.
4. **`M2-C05-02`** — clears all five parts:
   1. Sole Hard prerequisite `M2-C05-01` is `Completed` and merged
      (`task-tracker.md:164`; `bf2b4cd` is on master's first-parent line).
   2. `task_type: Frontend`, not `Product Decision`.
   3. Grepped `open-questions.md` for `M2-C05-02` — no hit.
   4. `tasks/M2-C05-02.md` carries no ⛔ banner — re-specified for Angular by `M2-C12-03` on
      2026-08-22, banner removed in that same change.
   5. Checked `git diff --stat master...<branch> -- <column-preference source files>` for
      every currently unmerged branch (`migration/M0-04-credential-rotation-runbook`,
      `migration/M0-06-remove-default-admin`, `migration/M2-B12-01-inv-012-numbering`,
      `migration/M2-B12-02-verify-unique-constraints`, `migration/M2-C06-record-picker-dialog`,
      `migration/M2-C10-decimal-handling`, `migration/M2-D01-currency-end-to-end`,
      `integration/2026-08-25-session-merges`) — no hit in any. `M2-C05-03`, the branch that
      previously conflicted on the same files, is already merged (ancestor of master `HEAD`).

`M2-C05-02` is the only row that clears all five parts this pass and is this pass's selection.

### Classification (KB-091 §4 — task file carries no explicit `complexity`/`risk` override)

- **Base**: `task_type: Frontend` → MEDIUM.
- **Raises** (only one applies):
  - `estimate: 3 d` ≥ 3 d — yes.
  - `depends_on`/reverse count ≥ 3 — no (one dependency, `M2-C05-01`).
  - `business_rules` non-empty — no (`[]`).
  - `source_files` spans 2+ of the four .NET projects — no, all under `V.SMART/V.SMART.Shared/`.
  - touches auth/tenancy/document numbering/calculation logic — no.
  - `risk` HIGH — no.
- **Complexity: HIGH** (MEDIUM + 1 raise).
- **Risk: MEDIUM** (default — not Security/Product Decision, no schema change, no
  secrets/`Program.cs`/`appsettings*`, `business_rules: []`; does not change what a live Blazor
  user observes — `ColumnMenu.razor`/the Blazor path is untouched and reference-only, and both
  existing persistence mechanisms stay byte-compatible per the task file's own framing).
- **Routing** (KB-091 §5.1, complexity HIGH): Investigate, Implement and Validate all route to
  `opus`.

### Safety / human-decision check

Not a safety stop: tree clean, `master` tip verified at `2281740`, branch to be cut fresh from
`master`. Not `requiresHuman`: no DBA/credential/environment need disclosed by
`tasks/M2-C05-02.md`, not a `Product Decision`, no pending architecture decision
(`ADR-007` already governs the stack).

### Carried forward — still true, untouched by this pass

- **`M2-D01`** now correctly excluded on master (`depends_on` fixed by `2281740`); its actual
  root blocker is `M0-04` (credential rotation, owner-only) → `M2-A04` → `M2-C02` → `M2-D01`.
  Do not re-dispatch until `M2-C02` is `Completed` and merged.
- **`M0-04`** (credential rotation runbook) closed `Blocked` on a separate, **unmerged** branch
  (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, since no
  human with production access participated. A merge decision, not a selection one.
- **`M0-06`** (fails part 5, unmerged `Blocked` branch) and **`M0-11`** (fails part 2, `Product
Decision`, owner-only) remain excluded, unchanged from every prior pass.
- **`M2-A03`** — closed **`Completed`** 2026-08-26: the owner added the required status check
  ("Restore, build and gate analyzer warnings", the `ci.yml` `build` job) to `master` branch
  protection in session. See tracker footnote ⁸².
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded — untouched by this pass.
- `Q-71, Q-81, Q-82, Q-83, Q-84, Q-91, Q-92, Q-93, Q-97` and `R-43, R-76, R-77, R-78, R-79`
  are untouched by this pass (`Q-97`, per the `M2-D01` branch's close-out, records the gap that
  the five-part test walks only `depends_on` and not a task file's own narrative Dependencies
  table — worth checking when it lands on master).
