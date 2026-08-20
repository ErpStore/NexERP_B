---
doc_id: ADR-003
title: React frontend stack selection
module: decisions
status: superseded
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-015, KB-050, KB-051]
---

# ADR-003 — React frontend stack

**Status:** ~~Accepted~~ **SUPERSEDED by [ADR-007](ADR-007-angular-stack.md), 2026-08-20** · **Date:** 2026-08-12

> ## Superseded — read this before using anything below
>
> **The framework choice in this document is no longer in force.** [ADR-007](ADR-007-angular-stack.md)
> selects **Angular + PrimeNG**. Nothing below should be implemented as written.
>
> **Why it was reopened, recorded because it is the more useful half of the lesson:** read for its
> reasoning, **this document never evaluated Angular at all.** Every rationale it records is a
> choice *within* React — Vite over Next.js, TanStack Query over Redux, Mantine over MUI/Ant/shadcn.
> Angular appears twice: a note about the pilot's XSS-exposed `localStorage` JWT, and *"the Angular
> pilot is archived, not converted."*
>
> So "React over Angular" was never a decision with reasons attached. It was an **assumption that
> acquired the authority of a decision by being written into an ADR** — an `Inferred` claim
> presented as `Confirmed`, which [KB-002](../source-of-truth-rules.md) exists to prevent.
>
> The decisive fact it never weighed: **the maintainer's background is C# and WPF, not frontend.**
> The runner writes the screens; he reviews and maintains them. Angular's component-plus-service
> shape, constructor DI and typed Reactive Forms map onto MVVM and XAML; React's hooks model does
> not.
>
> **What is still true and still worth reading here:**
>
> - The **Context** section below — ~140 screens, ~65 dense document editors, keyboard-first
>   operators, all state is server state. ADR-007 restates none of it and depends on it.
> - **Server-authoritative everything**, the **generated OpenAPI client**, **CSS-variable design
>   tokens**, **decimal.js**, and **one component library, never mixed** (R-22). None were React
>   decisions; all carry forward unchanged.
> - The **headless-table rationale** is sound engineering and was **deliberately reversed** by
>   ADR-007 — not because it is wrong, but because "own the grid internals" is right for a team with
>   deep frontend skill and wrong for a single maintainer new to it. See ADR-007 § *PrimeNG over
>   building headless*.
>
> **What was actually built against this ADR, and kept as history:** `M2-C01` (React scaffold,
> merged `12f172f`) and `M2-C04-01` (design tokens, merged `56b8ae2`). Both were real, both were
> verified, both are superseded. `tokens.css` survives the switch nearly verbatim — it is plain CSS
> custom properties, carrying the eight WCAG contrast corrections and the 12 px type-scale decision
> with it. This document is **not** rewritten to pretend that history went differently.

## Context

~140 target screens, of which ~65 are dense document editors (header form + editable line
grid + upstream-document picker + server-computed totals). Users are full-time operators
who work keyboard-first for whole shifts. All state is server state. A 152-screen × 5-right
permission matrix gates navigation, routes, and controls. Money and stock calculations must
never be computed on the client.

## Decision

| Concern | Choice |
|---|---|
| Framework | React 19 + TypeScript 5 (`strict`) |
| Build | Vite 6 |
| Routing | React Router v7 (library mode) |
| Server state | TanStack Query v5 |
| Client state | Zustand (auth/session/tenant/UI shell only) |
| Forms | React Hook Form |
| Validation | Zod, schemas **generated from OpenAPI** |
| UI library | Mantine 7 |
| Styling | CSS Modules + CSS-variable design tokens |
| Tables | TanStack Table v8 (headless) + TanStack Virtual |
| Charts | Recharts |
| HTTP | Axios with interceptors; **generated OpenAPI client** |
| Money display arithmetic | decimal.js |
| i18n | react-i18next from day one |
| Testing | Vitest + RTL + MSW + Playwright |

### Key rationales

**Vite over Next.js.** A fully authenticated, tenant-scoped internal tool has no SEO or
SSR requirement. Next.js would add a server runtime and deployment surface for no benefit.
Revisit only if a public customer portal appears.

**TanStack Query over Redux Toolkit Query / Redux.** Essentially all state in this app is
server state. Query's caching, invalidation-by-key-prefix, pagination, and mutation
lifecycle map directly onto the ERP's list/detail/save loop. Genuine client state is small
enough for Zustand.

**Mantine over MUI/Ant/shadcn.** Denser defaults (critical for ERP tables and forms),
strong form and table primitives, CSS-variable theming rather than a runtime style engine
(matters across 140 screens), built-in dark mode, permissive licence. MUI v6 is an
acceptable substitute if the team's existing skill favours it — **but the two are never
mixed.** Mixing libraries is precisely what makes the current MudBlazor + Bootstrap UI feel
incoherent (R-22).

**Headless tables.** No off-the-shelf grid will produce the density, keyboard model, and
server-driven paging this app needs without fighting it. TanStack Table headless + a
project-owned `DataGrid`/`LineItemGrid` gives full control at acceptable cost.

**React Hook Form (uncontrolled).** A 200-row line grid with controlled inputs re-renders
unacceptably. RHF keeps typing latency under target.

**Generated API client and Zod schemas.** 60–80 controllers hand-typed would drift within
weeks. Generation in CI makes contract breakage a build failure.

### Non-negotiable rules

1. **Never reimplement `CalculationService` in TypeScript** (BR-CALC-001). Totals come from
   `POST /documents/calculate`. A local optimistic preview is permitted for
   responsiveness, but must be overwritten by the server result before save.
2. **Never implement stock allocation client-side** (BR-STK-001).
3. **Client-side permission checks are UX affordances only** — the server enforces
   independently (ADR-004).
4. **No optimistic updates** for anything touching money, stock, or document status.
5. **Access token in memory; refresh token in an httpOnly cookie.** Explicitly diverging
   from the Angular pilot's `localStorage` JWT, which is XSS-exposed.
6. **One `DocumentEditor` component** configured 65 ways, not 65 bespoke editors.

## Consequences

**Positive.** Modern, hireable stack. The headless-table + `DocumentEditor` combination
turns the largest cost centre (65 document screens at 3,000–6,500 LOC each) into one
component plus configuration. Generated clients keep frontend and backend honest.

**Negative.** `DataGrid`, `LineItemGrid`, `RecordPickerDialog`, and `DocumentEditor` are
substantial in-house builds (~6–7 weeks in Phase 2) before any feature screen ships. That
front-loading is deliberate and is what makes Phases 3–4 tractable. Mantine is less
ubiquitous than MUI, so fewer engineers arrive knowing it.

**Neutral.** The Angular pilot is archived, not converted. Its only lasting contribution is
proof that the `AuthController` → `CurrencyController` → SPA path works.
