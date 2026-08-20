---
doc_id: ADR-007
title: Angular frontend stack selection — supersedes ADR-003
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-015, KB-050, KB-051, ADR-003]
---

# ADR-007 — Angular frontend stack

**Status:** Accepted · **Date:** 2026-08-20 · **Supersedes:** [ADR-003](ADR-003-react-stack.md)

## Context

The context of [ADR-003](ADR-003-react-stack.md) is unchanged and is not restated here: ~140
target screens, ~65 of them dense document editors (header form + editable line grid +
upstream-document picker + server-computed totals), full-time keyboard-first operators, all
state is server state, a 152-screen × 5-right permission matrix gating navigation, and money
and stock calculations that must never be computed on the client.

**What changed is not the requirements. It is who maintains the result.**

### The finding that reopened the decision

ADR-003 selected React on 2026-08-12. Read for its reasoning, **it never evaluated Angular at
all.** Every rationale it records is a choice *within* React — Vite over Next.js, TanStack Query
over Redux, Mantine over MUI/Ant/shadcn. Angular appears exactly twice in the document: a note
that the pilot's `localStorage` JWT is XSS-exposed, and *"the Angular pilot is archived, not
converted."*

So "React over Angular" was never a recorded decision with reasons attached. It was an
assumption that acquired the authority of one by being written down. Per
[KB-002](../source-of-truth-rules.md), that is an **Inferred** claim presented as **Confirmed**,
and the correct response is to make the decision properly rather than defend the artefact.

### The decisive fact, stated plainly

**The repository owner's experience is C# and WPF. He has not worked on frontend.** The
autonomous runner writes the screens; the owner reviews them, and maintains them for the life of
the product — including at 2 a.m. in three years when a document screen misbehaves in
production.

"Easier" therefore does not mean easier to write. It means **easier for a C#/WPF developer to
read, review and debug.** On that measure the two frameworks are not close.

| WPF / C# concept | Angular | React |
|---|---|---|
| XAML markup | Component template — declarative, binding syntax | JSX — JavaScript with markup embedded |
| ViewModel + `INotifyPropertyChanged` | Component class + signals / RxJS | Hooks, closures, dependency arrays |
| MVVM | Component + service split is the same separation | No prescribed pattern |
| Constructor dependency injection | Constructor DI, near-identical semantics | None — prop drilling or context |
| `ICommand` | Methods on the component class | Callback props |
| `IDataErrorInfo` / binding validation | Reactive Forms validators | React Hook Form + Zod, assembled |
| Strongly-typed C# | TypeScript — same language designer | TypeScript, ecosystem often fights it |

A C# developer opening an Angular component finds a class with injected services, typed
properties and a template bound to them. That is a View and a ViewModel. The same developer
opening a React component finds a function calling `useState`, `useEffect` with a dependency
array and `useMemo` — a functional-reactive model whose failure modes (stale closures, effects
firing twice, render loops) are precisely the kind that are miserable to diagnose without
fluency.

### The second reason: assembled stack versus batteries included

ADR-003's stack is **thirteen independently versioned libraries** — Vite, React Router, TanStack
Query, Zustand, React Hook Form, Zod, Mantine, TanStack Table, TanStack Virtual, Recharts, Axios,
decimal.js, react-i18next. Every integration seam between them is owned by this project, and
every one has its own release cadence and breaking changes.

Angular ships routing, forms, HTTP, DI and i18n as first-party, versioned together, upgraded with
one `ng update`. For a single maintainer new to frontend, that is the difference between one
thing to learn and thirteen.

### The third reason: it already exists

`frontend/vsmart-erp/` is an Angular 19.2 + PrimeNG 19.1 pilot with 40 tracked files and 9
components, including an auth service, a route guard and an HTTP interceptor — a real head start
on `M2-C02`. ADR-003 planned to archive it. **It becomes the baseline instead.**

## Decision

**Angular replaces React as the frontend framework for the V.SMART / NexGen ERP SPA.**

| Concern | Choice | Change from ADR-003 |
|---|---|---|
| Framework | **Angular**, standalone components, `strict` TypeScript | replaces React 19 |
| Exact major version | **Verified at scaffold time on the workstation**, as `M2-C01` did for Node. The pilot is on 19.2; do not assume, run `ng version` and record it | new |
| Build | **Angular CLI** (esbuild) | replaces Vite 6 |
| Routing | **Angular Router**, functional guards | replaces React Router v7 |
| Server state | **Typed Angular services over `HttpClient`**, explicit refetch | replaces TanStack Query — see below |
| Client state | **Angular signals** in services | replaces Zustand |
| Forms | **Typed Reactive Forms** | replaces React Hook Form |
| Validation | Angular validators, shapes **generated from OpenAPI** | Zod dropped, OpenAPI generation kept |
| UI library | **PrimeNG** — one library, never mixed | replaces Mantine 7 |
| Styling | **CSS-variable design tokens**, component styles | **unchanged** |
| Tables | **PrimeNG Table**; `LineItemGrid` re-evaluated, see below | replaces headless TanStack Table |
| Charts | PrimeNG Charts | replaces Recharts |
| HTTP | `HttpClient` + interceptors, **generated OpenAPI client** | Axios dropped, generation kept |
| Money display arithmetic | **decimal.js** | **unchanged** — framework-agnostic |
| i18n | **Runtime-switchable** (`ngx-translate` or equivalent), from day one | replaces react-i18next |
| Testing | **Jest or Vitest + Angular Testing Library + Playwright** | replaces Vitest + RTL + MSW |

### Key rationales

**Angular services over a query library.** TanStack Query's Angular adapter is still marked
experimental, and its value here is caching and invalidation that a small typed service layer can
provide explicitly. For a maintainer new to frontend, an explicit `refresh()` on a service is
easier to reason about than cache-key invalidation semantics. Revisit if list staleness becomes a
real complaint — not before.

**Signals over RxJS for component state.** Signals are closer to `INotifyPropertyChanged` than
observables are, and they are the direction Angular itself is moving. RxJS stays where it belongs:
HTTP and event streams.

**PrimeNG over building headless.** This is a **deliberate reversal of ADR-003's headless-table
decision**, and the reasoning that made it right there is what makes it wrong here. ADR-003 argued
that no off-the-shelf grid gives the required density, keyboard model and server-driven paging
without a fight — true, and a team with deep frontend skill should own that code. **A single
maintainer new to frontend should not.** Owning thousands of lines of grid internals means owning
every bug in them. PrimeNG's table covers `DataGrid`; if its editable-row model cannot deliver the
keyboard-first line-item entry `M2-C07` requires, **AG Grid is the fallback and its licence cost is
cheaper than the maintenance cost** — that evaluation is `M2-C07`'s to make and record, not this
ADR's to pre-empt.

**Testing away from Karma.** The pilot ships Karma + Jasmine; Karma is deprecated. The scaffold
task picks Jest or Vitest and records why.

**Runtime i18n, not compile-time.** Angular's built-in i18n produces one build per locale. An ERP
whose users switch language in-session needs runtime switching.

### Carried over from ADR-003 unchanged

These were never React decisions and survive intact:

- **Server-authoritative everything** — validation, calculations, permissions, document numbering.
  No ERP business logic in TypeScript.
- **Generated API client from OpenAPI**, so frontend and backend cannot drift silently.
- **CSS-variable design tokens**, including the eight WCAG contrast corrections and the 12 px
  workhorse type scale ([KB-051](../architecture/design-system-proposal.md), owner decision
  2026-08-20). `tokens.css` ports **almost verbatim** — it is plain CSS.
- **decimal.js** for display arithmetic.
- **One component library, never mixed.** Mixing is what makes the current MudBlazor + Bootstrap
  UI incoherent (R-22). PrimeNG is the single library; PrimeFlex is its own utility layer, not a
  second component library.

### Explicitly not inherited from the pilot

**The pilot stores its JWT in `localStorage`, which is XSS-exposed** — flagged by ADR-003 and true.
Adopting the pilot as a baseline does **not** adopt that. `M2-C02` decides the token storage model
against [ADR-004](ADR-004-server-side-authorization.md) and must not copy the pilot's approach by
default.

The pilot also hardcodes `http://localhost:5144` in **both** `environment.ts` and
`environment.prod.ts` — its production build points at localhost. That is a defect to remove, not
a pattern to keep.

## Consequences

**Positive.** The maintainer can read, review and debug the result. The stack is one framework
plus a component library instead of thirteen libraries and their seams. An existing pilot with
working auth is the starting point rather than landfill. PrimeNG removes the largest in-house
build from the critical path.

**Negative — stated plainly, because a reversal is the easiest thing to write up favourably.**
Two completed, merged, independently verified tasks are discarded: `M2-C01` (React scaffold) and
`M2-C04-01` (design tokens). Roughly twelve `M2-C` task specifications need rewriting. That is
**1–2 weeks of re-specification, and it is real waste, not a rounding error.**

Two things reduce it, and neither should be used to pretend the cost is zero:

- `tokens.css` is plain CSS custom properties and ports nearly unchanged, carrying the WCAG
  corrections and the type-scale decision with it.
- `M2-C01` was not wasted in one respect: it proved the frontend CI pipeline, which ran green on a
  hosted runner and is now a known-good shape to re-point at Angular.

**Why now rather than later.** Two of twenty `M2-C` tasks are complete. `M2-C05` (`DataGrid`),
`M2-C07` (`LineItemGrid`) and `M2-C08` (`DocumentEditor`) — the bulk of the frontend, 6–7 weeks by
ADR-003's own estimate — are all still ahead. Switching after those land would discard 8–10 weeks
instead of two. **The cost of this decision only grows, and it grows fastest in the next month.**

**Neutral — and this is the load-bearing point.** **No backend work is affected.** Of the seven
`M2` tasks completed, five are backend — `M2-A01-01`, `M2-A01-02`, `M2-A06`, `M2-B07`, `M2-B02` —
and none knows what framework calls it. The API's error contract, authorization filter, paging
contract and DI seam are framework-neutral by design, which is exactly the property that makes
this decision affordable. `M2-B10` (OpenAPI → TypeScript client) serves Angular as well as React.

## What this ADR does not decide

- **The exact Angular major version** — `M2-C01` verifies and records it.
- **Whether PrimeNG's table suffices for `M2-C07`'s keyboard-first line-item entry**, or AG Grid is
  required — `M2-C07` evaluates and records.
- **Token storage for the JWT** — `M2-C02`, against ADR-004.
- **Whether `frontend/nexgen-web/` is deleted or left dormant** — `M2-C01`'s re-scope decides, but
  the default is deletion in the same change that scaffolds Angular: two frontend applications in
  one repository, only one of them built, is how a stale tree starts.
