---
doc_id: KB-082
title: Task Dependency Graph, Critical Path and Parallel Execution Plan
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: proposal
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-080, KB-081, KB-088]
---

# Task Dependency Graph

**Dependency classes**

| Class | Meaning | Consequence of ignoring it |
|---|---|---|
| **Hard** | B cannot start until A completes | B fails outright |
| **Soft** | B is materially easier or safer after A | rework, not failure |
| **Information** | B needs an answer A produces | B proceeds on a guess |
| **Testing** | B's verification depends on A | B ships unverified |
| **Deployment** | B cannot reach an environment until A | B cannot be validated in situ |

---

## M0 — Stabilise

```
                          ┌──────────────────────────────────────────┐
  M0-01-01 ──► M0-01-02 ──┤──► M0-01-03 ─────────────────────────────┤
  (inventory)  (script)   └──► M0-02 (drift, Q-14) ──────────────────┤
                                                                     │
  M0-04 (rotate) ──────────────┐                                     │
                               ├──► M0-05 (purge history) ───────────┤
  M0-03-01 ─► M0-03-02 ─► M0-03-03 ═► [M0-03] ─────────────────┘     │
      │                                                              ├─► G0
      └──► M0-14 (DetailedErrors)                                    │
                                                                     │
  M0-00 ──► M0-08 ──► M0-07 ──► M0-12-01 ═► [M0-12]                  │
    │                    │          ├──► M0-12-02 ──────────────────┤
    └──► M0-15 ──────────┘          ├──► M0-13 ──► M0-11 (Q-01) ────┤
                                    ├──► M0-09 ──► M0-10            │
                                    └──► M0-06 ─────────────────────┘
```

`═►` = child rolls up into its parent. `[M0-03]` and `[M0-12]` are containers, never worked
directly.

| Edge | Class | Why |
|---|---|---|
| M0-00 → M0-08 → M0-07 | Hard | all three rewrite repository-wide state; concurrent execution guarantees conflicts |
| M0-00 → M0-15 | Hard | the build baseline is meaningless measured against a dirty tree |
| M0-15 + M0-08 → M0-07 | Hard | CI needs the warning baseline and the corrected tracked-file set |
| M0-07 → M0-12-01 | Hard | the test project is wired into CI at creation, not retrofitted |
| M0-12-01 → M0-12-02 / M0-13 / M0-09 / M0-06 | Hard | nothing can assert without a test project |
| M0-13 → M0-11 | **Hard** | the decision must be taken with current behaviour already pinned, or the change is invisible |
| M0-09 → M0-10 | Soft | the audit uses the fix as its reference pattern |
| M0-03 + M0-04 → M0-05 | **Hard** | purging before rotation leaves live credentials in every existing clone; purging before externalisation loses the working configuration |
| M0-03-01 → M0-14 | Hard | **same file** — `V.SMART/V.SMART.Web/appsettings.json` |
| M0-01-02 → M0-01-03 / M0-02 | Hard | nothing to deploy or diff until the procedures are captured |
| M0-04 → Q-19 | Information | repository visibility must be settled before rotation is planned |

---

## M0 → M2 gate crossing

```
  M0-01-03 ─┐
  M0-05 ────┤
  M0-07 ────┼──► ★ G0 ★ ──┬──► M2-B07 ──► M2-A01-01 ──► M2-A01-02 ──► M2-A01-03
  M0-12-02 ─┤             │                                  │
  M0-13 ────┤             ├──► M2-A06                        ├──► M2-A02 ──► M2-A03
  M0-11 ────┘             │                                  ├──► M2-A07
                          └──► M2-C01                        ├──► M2-A08
                                                             └──► M2-B08
```

**G0 is a hard gate for all of M2.** Not process ceremony: without the stored procedures no
environment is reproducible, and without characterisation tests there is no way to show the
API preserves behaviour.

---

## M2 — Foundation

```
BACKEND STREAM
  M2-B07 ──┬──► M2-B04 (decouple Pages refs)
           ├──► M2-B01 (/api/v1)
           ├──► M2-B05 (ScreenCodes)
           └──► M2-B12-01 ──► M2-B12-02 ──► M2-B12-03 ═► [M2-B12]

  M2-A06 ──┬──► M2-B02 ──► M2-B03 ──► M2-B10 ──────────────┐
           ├──► M2-B06                                     │
           ├──► M2-B09  (also needs M2-B02)                │
           └──► M2-B11                                     │
                                                           │
  M2-A01-03 ──┬──► M2-A02 ──► M2-A03  ────────────────────┤
              ├──► M2-A07 ──────────────┐                  │
              ├──► M2-A08               │                  │
              └──► M2-B08 ──────────┐   │                  │
  M2-A01-02 ──► M2-A04 ──► M2-A05   │   │                  │
                    │              │   │                  │
FRONTEND STREAM     └──────────────┼───┤                  │
  M2-C01 ──┬──► M2-C10 ────────┐   │   │                  │
           ├──► M2-C11         │   │   │                  │
           ├──► M2-C04-01 ──┬──┼───┼───┼── M2-C04-02 ──┐  │
           │                └──┼───┼───┼── M2-C04-03   │  │
           └──► M2-C02 ◄───────┴───┘   │       │       │  │
                    │                   │       │       │  │
                    └──► M2-C03 ◄───────┘       │       │  │
                                                ▼       │  │
                              M2-B02 ──────► M2-C05-01 ◄┘  │
                                              │            │
                              ┌───────────────┼────────────┤
                              ▼               ▼            │
                        M2-C05-02       M2-C05-03          │
                        M2-C06                             │
                        M2-C07 (also ◄ M2-C10)             │
                           │                               │
                           ▼                               │
                     M2-C08-01 ──┬──► M2-C08-02            │
                                 └──► M2-C08-03            │
                        M2-C09 ◄── M2-B08                  │
                                                           │
VERTICAL SLICE                                             │
  M2-C05-03 + M2-A02 + M2-B10 ─────────────────────────────┴──► M2-D01
                                                                  │
                                    M2-D02-01 ──► M2-D02-02 ──► M2-D02-03
                                                                  │
                                                              M2-D03 ──► ★ G2 ★
```

| Edge | Class | Why |
|---|---|---|
| M2-B07 → every controller task | **Hard** | `V.SMART.Api/Program.cs` registers only `ICurrencyService`; any second controller fails at runtime on DI resolution |
| M2-A06 → M2-B02 → M2-B03 → M2-B10 | **Hard** | error contract → paging contract → template → generated client. Generating a client from an unsettled contract invalidates the whole chain |
| M2-A01-03 → M2-A02 → M2-A03 | Hard | one security surface, built once and then proven |
| M2-A04 → M2-A05 | Hard | both change the token shape and `JwtTokenService` |
| M2-A07 → M2-C02 | Hard | the permission store is populated from `GET /api/v1/me` |
| M2-A08 → M2-D01 | **Hard** | if `StateCodesCsv` scoping is real, the first list endpoint leaks data without it |
| M2-C05-01 → M2-C06 / M2-C07 / M2-C09 | Hard | the grid underpins picker, line grid and report page |
| M2-C10 → M2-C07 | Hard | editable quantity/amount cells must not use float arithmetic |
| M2-C07 → M2-C08-01 | Hard | the document editor embeds the line grid |
| M2-B08 → M2-C09 | Hard | the report page needs report endpoints to call |
| M2-B12-03 → first document-create endpoint | **Hard** | an API raises concurrency well above Blazor Server's; unguarded numbering produces duplicates |
| M2-D02-01 → M2-D02-03 | **Hard** | extract-before-rebuild. Building the screen first is how logic silently lands in TypeScript |

### Same-file conflicts — never parallelise

| Tasks | Shared surface |
|---|---|
| M2-A05, M2-B01 | `V.SMART/V.SMART.Api/Program.cs` — routing and middleware |
| M2-A04, M2-A05 | `JwtTokenService` and token shape |
| M2-A06, M2-B11 | the middleware pipeline |
| M2-C02, M2-C03 | the permission store |
| M0-03-01, M0-14 | `V.SMART/V.SMART.Web/appsettings.json` |
| M0-06, M0-13 | `ApplicationDbContext` seed data |

---

## M3 / M4 — within a wave

Every wave is a chain with two parallel tails:

```
 <W>-01 ──► <W>-02 ──► <W>-03 ──► <W>-04 ──► <W>-05 ──► <W>-06 ──┬──► <W>-07
 (rules)   (triage)  (extract)  (verify)  (contract) (controller)│
                                                                 └──► <W>-08 ──► <W>-09
                                                                          │
                                                            <W>-10 ◄──────┘
                                                              │
                                                         <W>-11 ──► <W>-12 ──► <W>-13 ──► <W>-14
```

`<W>-03 → <W>-04 → <W>-08` is the **hard** ordering that enforces extract-before-rebuild.
`<W>-07` (API tests) and `<W>-08` (React screens) genuinely parallelise once `<W>-06` lands.

### Wave ordering

```
G2 ──► M3-1 ──► M3-2 ──► M3-3 ──┬──► M3-4
                                └──► M3-5 ──► M3-9 ──► ★ G3 ★
       M3-6, M3-7, M3-8 ─────────────────────────────────┘  (parallel throughout)

G3 ──► M4-2 ──► M4-1 ──► M4-3 ──► M4-4 ──► M4-5 ──► M4-6 ──► M4-7 ──► ★ G4 ★
       M4-8, M4-9, M4-10, M4-11 ────────────────────────────────────────┘
```

| Edge | Class | Why |
|---|---|---|
| M3-1 → M3-2 | Hard | Item and BOM master screens use Accounts/General master pickers |
| M3-3 → M3-5 | Soft | the permission matrix should be administrable before the first document module |
| M2-C08 → M3-5 | **Hard** | Sales Order is the `DocumentEditor` reference implementation |
| M3-5 → M3-9 | Hard | M3-9 re-baselines M4 from measured extraction cost |
| M0-11 applied → M4-2-03 | **Hard** | stock behaviour must be settled before Inventory migrates |
| M4-2 → M4-1 | **Hard** | Purchase SCN writes stock; Inventory hardening must land first |
| M4-4 → M4-5 | Hard | despatch/invoice documents consume production output |
| M3-8 → every `<W>-12` | Hard | no wave can ship without feature flags |

---

## Project critical path

```
M0-00 → M0-08 → M0-07 → M0-12-01 → M0-13 → M0-11 → ★G0★
      → M2-B07 → M2-A01-01 → M2-A01-02 → M2-A01-03 → M2-A02 → M2-A03
      → M2-C05-01 → M2-C05-03 → M2-D01 → M2-D02-01 → M2-D02-02 → M2-D02-03 → M2-D03 → ★G2★
      → M3-1 → M3-2 → M3-3 → M3-5 → M3-9 → ★G3★
      → M4-2 → M4-1 → M4-3 → M4-4 → M4-5 → M4-6 → M4-7 → ★G4★
      → M5 sweep → ★G5★ → M6-01 → M6-03 → M6-04 → M6-07 → ★G6★
```

**Contended for the M0 critical path:** `M0-01-01 → M0-01-02 → M0-01-03`. It becomes the
binding constraint the moment DBA access takes more than a few days — it is the longest-lead
item in the project's first month and the only one that cannot be accelerated by engineering
effort. Escalate it on day one.

**Where the schedule is actually decided:** `M4-7` (Labour Work — a 6,112-LOC service and a
6,528-LOC page) is the largest single item, and every M4 estimate is provisional until
`M3-9`. The plan's honest position is that the M4 range is unreliable until Sales Order
(M3-5) provides the first real measurement of `@code` extraction cost.

---

## Parallel execution plan

### Which tasks can run in parallel

| Window | Streams |
|---|---|
| **M0** | (a) `M0-01-*` — DBA work, touches only `db/stored-procedures/`; (b) `M0-04` — ops, outside the repo; (c) `M0-00 → M0-08 → M0-07` — repo hygiene; (d) after M0-12-01: `M0-12-02`, `M0-13`, `M0-09`, `M0-06` |
| **M2** | (a) backend M2-A/M2-B; (b) frontend M2-C — genuinely independent until `M2-C02` needs `M2-A04`+`M2-A07`. `M2-C01`, `M2-C04-*`, `M2-C10`, `M2-C11` need no backend at all |
| **M3** | `M3-6` (reports, read-only) and `M3-8` (feature flags) parallelise with every wave. Within a wave, `-01`/`-02` overlap the previous wave's `-08`…`-14` |
| **M4** | `M4-11` (reports) throughout; `M4-8`/`M4-9`/`M4-10` are weakly coupled to the manufacturing spine and can run alongside `M4-5`…`M4-8` with a separate pair |
| **M5** | M5-01…M5-06 are continuous by construction; M5-07/M5-08/M5-10 are the discrete sweep |

### Which must remain sequential

Everything marked **Hard** above. In particular, and against intuition:

- `M0-03` → `M0-05`. Purge and rotate look independent; they are not.
- `M2-A05` and `M2-B01`. Different concerns, same file.
- `<W>-03` → `<W>-08` in every wave. Extraction before UI is the project's central rule.

### Realistic team shape

| Phase | Backend | Frontend | QA | Other |
|---|---|---|---|---|
| M0 | 1–2 | 0 | 0 | 1 DBA (critical), 1 ops (rotation) |
| M2 | 2–3 | 2–3 | 1 | — |
| M3 / M4 | 2–3 | 2–3 | 1 | product owner for wave sign-off |

M0 does **not** need the frontend team. If frontend engineers are idle during M0, the honest
options are `M2-C01`/`M2-C04-*` prototyping against mocks — not starting module work.

---

## Ready-task selection rule

Applied at the end of every task to choose the next `current-task.md`
([KB-088 §7](workflow.md#7-selecting-the-next-task)). It is deterministic so that two
independent sessions reach the same answer — and so that "what's next" never depends on
someone remembering a conversation.

**1. Build the candidate set** from [KB-081](task-tracker.md). A task is a candidate when:

- every **Hard** prerequisite above is genuinely `COMPLETED` — not `REVIEW`. A prerequisite
  that is committed but unmerged still blocks a merge-dependent successor; check the tracker's
  footnotes, which record exactly this distinction for M0-03, M0-08 and M0-01-03;
- every **Information** dependency has an actual answer — an unanswered `Q-nn` means the task
  proceeds on a guess, which is worse than waiting;
- it is not a parent container (`M0-03`, `M0-12`, `M2-B12`, `M2-C04`, `M2-C05`, `M2-C08`,
  `M2-D02` are never worked directly — only their children);
- it is not blocked on a human step nobody has scheduled. Such a task is `BLOCKED` with a
  named owner, and **surfacing it to that owner is itself the useful action**.

**2. Remove same-file conflicts.** Drop any candidate sharing a surface with in-flight work,
per *Same-file conflicts — never parallelise* above. Two sessions editing `Program.cs` or
`appsettings.json` in parallel produce a merge, not progress.

**3. Rank**, in order:

1. **P0 before P1 before P2.**
2. **Most downstream unblocking** — count tasks whose `depends_on` names it. `M2-B07` blocks
   every controller task; nothing else in M2-B comes close.
3. **On the critical path** (§ *Project critical path*) before off it.
4. **Longest lead time first** where a dependency is external. `M0-01-*` needs DBA access and
   cannot be accelerated by engineering effort — it is the binding constraint the moment that
   access takes more than a few days.
5. **Smaller estimate** as the final tie-break, to keep review batches small.

**4. If two candidates are equally ranked and genuinely independent, say so and let the owner
choose.** Do not pick silently — parallel capacity is a staffing question, and the honest
answer ("these two can run in parallel, they touch nothing in common") is more useful than an
arbitrary pick.

**5. Write the winner into [`current-task.md`](current-task.md) and stop.** Do not implement
it.

### Worked example

At 2026-08-16 the tracker lists `M0-15` and `M0-02` as `Ready`. Both are candidates. Neither
shares a file with the other. `M0-15` is P0, `M0-02` is P1 → **M0-15** wins at rank step 1,
without needing the later steps. It is also on the critical path via `M0-07`, and `M0-02`
depends on DBA access that no one has scheduled — so `M0-02` is arguably `BLOCKED` on a human
rather than `Ready`, which is a tracker correction worth making when someone next touches it.
