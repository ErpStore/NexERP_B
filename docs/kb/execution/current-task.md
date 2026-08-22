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
last_verified: 2026-08-22
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ M2-C04-01 — Design tokens, theme, light/dark

**Task file:** [`tasks/M2-C04-01.md`](tasks/M2-C04-01.md) — the token layer of
[KB-051](../frontend-new/design-system.md) as CSS custom properties (semantic colour,
typography, spacing, radius, elevation, motion, breakpoints, density), authored as **two
first-class palettes** (light and dark, not one filtered into the other), plus a root
`ThemeService` (Angular signals, `light | dark | system` preference), a theme-toggle
component, the PrimeNG theme-preset reconciliation, and an automated contrast test over every
token pair used for text or UI boundaries.

**Why this one.** `M2-C12-05` closed this session (`Needs Review`, independently validated
`PASS`) and re-specified the last of the 25 formerly-`⛔`-banner files — the frontend tree is
reachable again for the first time since ADR-007. `M2-C04-01` and `M2-C10` are the only two
tasks this newly releases as genuinely selectable: both `depends_on: [M2-C01]` only
(`Completed`/merged), both carry `status: Ready` in their own re-specified task files, neither
has a sibling branch, and neither is gated by an unanswered open question that actually blocks
execution. `M2-C04-01` wins the rank: it unblocks two direct children (`M2-C04-02`,
`M2-C04-03`) against `M2-C10`'s one (`M2-C07`, itself further gated on `M2-C05-01` and
**Q-71**), and it sits on the ancestry of the project's stated critical path
(`M2-C04-01 → M2-C04-02 → M2-C05-01 → M2-C05-03 → M2-D01 → …`,
[`dependency-graph.md`](dependency-graph.md) § *Project critical path*) while `M2-C10` feeds
only the off-path `M2-C07`. Full reasoning: `task-tracker.md` footnote ⁴⁷,
`tasks/M2-C12-05.md` § Execution Record (2026-08-22) — Close-out.

### Five-part "can actually be done" check

1. Hard prerequisite `M2-C01` — `Completed` and merged to `master` (`2dd4e53`, 2026-08-21).
   **Met.**
2. Not a `Product Decision`. **Met** — `task_type: Frontend`.
3. Not blocked on an unanswered open question. **Carries a caveat, judged non-blocking.**
   `M2-C04-01` carries **Q-68** (whether resetting its status to `Ready` after its earlier
   React implementation was deleted from disk is what the owner intends). Q-68's own "Impact
   if unresolved" column reads *"Nothing technically"* — it governs how the tracker row should
   be worded, not whether the work is gated. The task file itself already carries `status:
   Ready` as the applied (conservative) answer. If the owner later rules the other way on
   Q-68, that corrects bookkeeping, not this task's output.
4. Task file not superseded/stale. **Met** — no ⛔ banner, re-specified for Angular by
   `M2-C12-01` (merged), `last_verified: 2026-08-22`.
5. No sibling branch open on the same files. **Met** — `git branch --no-merged master` (checked
   2026-08-22 during `M2-C12-05`'s close-out) lists no `M2-C04-01` branch; three unrelated
   sibling worktrees exist (`wt-M0-10`, `wt-M2-A08`, `wt-M2-B01`), none touching
   `frontend/nexgen-web/`, `M2-C04-01.md` or its `source_files`.

### Read before starting

- [`tasks/M2-C04-01.md`](tasks/M2-C04-01.md) in full — it is dense: the re-specification note
  at the top explains what carried over from the discarded React implementation (the eight
  WCAG contrast corrections and the 12/18 type scale, now recorded as *shipped* values in
  [KB-051 §Colour](../frontend-new/design-system.md#colour)) and what did not (the file paths —
  `frontend/nexgen-web/src/styles/tokens.css` and `src/app/core/theme/`, not the deleted React
  tree's paths).
- **Q-33** — `UserThemePreference.cs:20` holds a single `bool IsDarkMode` (default `false`) and
  **cannot represent `system`**, but KB-051 asks for a `system` default. This task must record
  the schema/spec mismatch, not quietly resolve it either direction.
- **Q-68** — see check 3 above. Do not treat it as licence to skip the work; it is a
  bookkeeping question about how the tracker describes what you are about to do.
- KB-051 §Colour for the already-measured contrast ratios this task must reproduce, not
  re-litigate.
- `ADR-007-angular-stack.md` for the PrimeNG-preset-from-CSS-tokens question (**Q-67**, partly
  answered — the reconciliation mechanism is this task's to establish and record which of the
  two routes worked).

### Session status

Selected at `M2-C12-05`'s close-out (2026-08-22, Select phase only). **Not yet dispatched.** A
future session picks this up at Investigate/Implement per [`workflow.md`](workflow.md)
(KB-088) and [`autonomous-runner.md`](autonomous-runner.md) (KB-091). `M2-C04-01`'s frontmatter
does not state `complexity`/`risk` explicitly — classify per KB-091 §4 at dispatch
(`task_type: Frontend` → base MEDIUM; `source_files` cites three `V.SMART/` files read-only for
behaviour reference, no schema/auth/tenancy/numbering/calculation surface — check the raise/HIGH
triggers against the actual `Implementation Requirements` section before routing).
