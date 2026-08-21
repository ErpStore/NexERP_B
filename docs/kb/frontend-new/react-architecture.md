---
doc_id: KB-050
title: Proposed Frontend Architecture (Angular — rewrite pending, M2-C00)
module: frontend-new
source_files: []
entities: []
api_endpoints: ["GET /api/v1/me"]
database_tables: []
business_rules: []
status: proposal
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-013, KB-015, KB-040, KB-041]
---

# Proposed Frontend Architecture

> # ⚠ PARTLY SUPERSEDED, 2026-08-20 — the framework is now Angular
>
> [`ADR-007`](../decisions/ADR-007-angular-stack.md) selects **Angular + PrimeNG** and supersedes
> `ADR-003`. **This document has not been rewritten yet**, so read it with the map below rather
> than as a whole. Rewriting it is task **`M2-C00`**, and every `M2-C` task depends on the result.
>
> **Dead — do not implement:**
>
> | Section | Why |
> |---|---|
> | *Recommended stack* | Replaced wholesale by ADR-007's table |
> | *Why not Next.js* | A React-internal question. Angular CLI is the build |
> | *Why Mantine over MUI* | Replaced by PrimeNG |
> | *Project structure* | Needs the Angular layout (standalone components, feature routes, `core`/`shared`/`features`) |
> | *Data-fetching conventions* | TanStack Query is out; typed services over `HttpClient` with explicit `refresh()` are in |
>
> **Still valid and still binding — none of it was ever a React decision:**
>
> | Section | Why it survives |
> |---|---|
> | *Design constraints from the existing system* | Facts about the ERP, not the framework |
> | *Document editor pattern* | **The core abstraction.** Header + line grid + picker + server-computed totals is a shape, not a library |
> | *Workflow commands* | Same |
> | *Permission-based rendering* | The 152 × 5 matrix is the constraint; only the rendering syntax changes |
> | *Authentication flow* | Conceptually intact — but **token storage is `M2-C02`'s decision** against ADR-004, and must **not** copy the pilot's `localStorage` JWT |
> | *Performance targets*, *Accessibility* | Framework-agnostic; the WCAG work in KB-051 already met them once |
> | *What is deliberately not rebuilt* | Framework-agnostic scope boundary |
>
> **Error handling** is neither dead nor current: it predates `M2-A06`, which shipped the real
> `application/problem+json` contract. Use [ADR-002 §4](../decisions/ADR-002-rest-api-layer.md) and
> `V.SMART.Api/Middleware/ApiProblems.cs` as the source, not this section.
>
> The title and `doc_id` stay `KB-050` through the rewrite so the ~20 task files citing it do not
> all have to change twice.

> **Proposal.** Nothing here describes existing code. Constraints it must satisfy come
> from [`architecture/frontend-architecture-existing.md`](../architecture/frontend-architecture-existing.md)
> and [`architecture/auth-and-permissions.md`](../architecture/auth-and-permissions.md).

## Design constraints from the existing system

1. **~150 list screens and ~65 document-editor screens.** The document editor — header
   form + editable line grid + upstream-document picker + totals panel — is *the* screen.
   Optimise everything for it.
2. **440 routes** to reproduce, but the new app can consolidate (create/update/details
   collapse into one route with a mode).
3. **152-screen × 5-right permission matrix** must gate navigation, routes, and controls.
4. **Multi-tenant**, tenant fixed at login and carried in the JWT.
5. **Server holds the truth for calculations** (BR-CALC-001) — never reimplement in TS.
6. **Dense, keyboard-driven data entry.** Operators enter 20–50 line documents. Latency
   and keyboard flow matter more than animation.
7. **Server-side paging/filtering already exists** (`SearchWithDynamicFilterAsync`) — the
   grid must be server-driven, not client-side.
8. **PDF and Excel come from the server** as bytes.

## Recommended stack

| Concern | Choice | Rationale |
|---|---|---|
| **Framework** | **React 19** | Current major; Actions and `useOptimistic` suit form-heavy work |
| **Language** | **TypeScript 5.x**, `strict: true` | Non-negotiable at this domain complexity |
| **Build** | **Vite 6** | Fast HMR; the ~150-screen app needs good code splitting |
| **Routing** | **React Router v7** (framework mode off, library mode on) | Mature, data-router loaders/actions, nested layouts, `useBlocker` for the unsaved-changes guard the ERP already relies on |
| **Server state** | **TanStack Query v5** | The whole app is server state. Caching, invalidation, pagination, and mutation lifecycles are exactly the problem |
| **Client state** | **Zustand** (small stores only) | Auth/session, tenant, UI shell, permission map. Deliberately not Redux — there is very little genuine client state |
| **Forms** | **React Hook Form** | Uncontrolled by default → the 50-row line grid stays fast |
| **Validation** | **Zod** + `@hookform/resolvers` | Mirrors the `DataAnnotations` on ViewModels; **generate schemas from OpenAPI**, don't hand-write |
| **UI components** | **Mantine 7** (primary recommendation) | Dense by default, excellent form and table primitives, first-class TS, built-in dark mode, permissive licence. Alternative: shadcn/ui + Radix for maximum design control at higher build cost |
| **Styling** | **CSS Modules + design tokens** (Mantine's CSS-variable theme) | Avoids utility-class sprawl in 150 screens; tokens keep light/dark coherent |
| **Data grid** | **TanStack Table v8** (headless) + a project `DataGrid` wrapper; **TanStack Virtual** for row virtualisation | Server-side paging/sorting/filtering, column pinning, per-user column visibility (maps to `UserColumnPreference`). Headless keeps the ERP's dense look under our control |
| **Line-item editor** | Custom `LineItemGrid` on TanStack Table + RHF `useFieldArray` | This is the highest-value component in the app; it will not come from a library |
| **Charts** | **Recharts** (or ECharts if the dashboard grows) | Replaces Blazor-ApexCharts |
| **Dates** | **date-fns** + `date-fns-tz` | Financial-year helpers mirror `FinancialYearHelper.cs` |
| **Numbers/money** | **decimal.js** for any client-side money display arithmetic | JS floats must not be trusted for money; server remains authoritative |
| **HTTP** | **Axios** with interceptors, or `ky` | Bearer injection, refresh-on-401, correlation id, tenant header |
| **API client** | **Generated from OpenAPI** (`openapi-typescript` + `openapi-fetch`, or Orval for TanStack Query hooks) | 60–80 controllers by hand is untenable and drifts |
| **Notifications** | **Mantine Notifications** | Toasts for save/error; long-running ops get a progress surface |
| **Errors** | **react-error-boundary** + a global `ProblemDetails` handler | Business-rule 409s surface the server's message verbatim |
| **i18n** | **react-i18next**, wired from day one | Even if only `en` ships; retrofitting 150 screens is far worse |
| **Tables → Excel** | server endpoint | Reuse `ExcelExportService` |
| **PDF viewing** | native `<embed>`/blob URL, or `react-pdf` if annotation is needed | Server generates the PDF |
| **Testing** | **Vitest** + **React Testing Library**; **MSW** for API mocking; **Playwright** for E2E | |
| **Quality** | ESLint (flat) + Prettier + `typescript-eslint` + Husky/lint-staged | |
| **CI** | GitHub Actions: typecheck → lint → unit → build → Playwright | The repo has **no CI at all** today |

### Why not Next.js

The app is a fully authenticated, tenant-scoped internal tool behind a login. There is no
SEO requirement, no public content, and no meaningful SSR benefit. Next.js adds a server
runtime and deployment surface for no gain here. **Vite + React Router library mode** is
the right weight. Revisit only if a public customer portal is added.

### Why Mantine over MUI

MUI is the obvious peer, but Mantine's defaults are denser (better for ERP tables and
forms), its hooks library covers most of what this app needs, and its theming is CSS
variables rather than a runtime style engine — which matters across 150 screens. If the
team already knows MUI well, MUI v6 is an acceptable substitute; **do not mix the two.**

## Project structure

Feature-sliced, mirroring the ERP module map so the codebase is navigable by domain
experts:

```
src/
  app/
    router.tsx                 route tree + permission guards
    providers.tsx              Query, theme, i18n, error boundary, notifications
    App.tsx
  shared/
    api/
      generated/               ← OpenAPI-generated client + types (do not edit)
      client.ts                axios instance, interceptors
      queryKeys.ts             centralised key factory
    auth/
      useAuth.ts, authStore.ts, PermissionGate.tsx, useScreenRights.ts
    components/                design-system primitives (see design-system.md)
      DataGrid/  LineItemGrid/  RecordPickerDialog/  PageHeader/  FormField/
      ConfirmDialog/  BusyOverlay/  StatusBadge/  EmptyState/  ErrorState/
    hooks/  lib/  types/  i18n/
  features/
    masters/
      currency/  customer/  vendor/  item/  bom/  store/  hsn/  …
    sales/
      leads/  enquiry/  feasibility/  quotation/  sales-order/  contract-review/
      proforma-invoice/
    manufacturing/  labour/  outsourcing/  purchase/  subcontract/
    planning/  production/  inventory/  inspection/  maintenance/
    hr/  accounts/  reports/  dashboard/  settings/  utilities/
  layouts/
    AppShell.tsx  AuthLayout.tsx  PrintLayout.tsx
```

> **Realised 2026-08-19 by M2-C01 at `frontend/nexgen-web/`.** This tree is no longer a
> proposal: the skeleton above exists on disk, as empty directories carrying `.gitkeep`
> wherever the contents belong to a later task. Reconciliation, so nobody has to diff it:
>
> - **Created and populated:** `src/app/` (`App.tsx`, `providers.tsx`, `router.tsx`, plus
>   `App.test.tsx`), `src/shared/i18n/` (`en.json`, one key).
> - **Created empty (`.gitkeep`):** `src/shared/{api,auth,components,hooks,lib,types}/`,
>   `src/features/`, `src/layouts/`.
> - **Named above but deliberately NOT created yet**, because the file *is* the later task and
>   an empty stub would be a lie about readiness: `shared/api/generated/` and `client.ts`,
>   `queryKeys.ts` (**M2-B10**); `shared/auth/*` (**M2-C02**); every `shared/components/*`
>   primitive (**M2-C04-02/03**); `layouts/AppShell.tsx`, `AuthLayout.tsx`, `PrintLayout.tsx`
>   (**M2-C03**); every `features/*` module folder (**M2-D** onward). The ESLint rule confining
>   `shared/api/generated/**` to `shared/api/**` was nonetheless written now, so it never has to
>   be retrofitted.
> - **Present in the built tree but not named above** — three additions, none of them a new
>   concept: `src/test/` (Vitest `setup.ts` and the MSW harness), `src/vite-env.d.ts`, and a
>   top-level `e2e/` for Playwright, which sits beside `src/` rather than inside it.
>
> Measured bundle baseline at that commit: entry chunk **90.90 kB gzip**, initial JS
> **125.38 kB gzip** — see the performance targets below and
> `frontend/nexgen-web/README.md`.

> **Extended 2026-08-19 by M2-C04-01 — the theme layer.** Two additions to the tree above,
> both named by this task rather than by this document, and reported as **proposed paths**:
>
> - **`src/shared/theme/`** — `tokens.css` (the two palettes; the only file in `src/**`
>   allowed to contain a colour literal), `tokens.ts`, `theme.ts`, `ThemeProvider.tsx`,
>   `useColorScheme.ts`, `ThemeToggle.tsx` + `ThemeToggle.module.css`, `density.ts`,
>   `breakpoints.ts`, `README.md`, and four test files. It sits under `shared/` rather than
>   `shared/components/`: it is not a primitive, it is what the primitives are made of.
> - **`src/styles/global.css`** — reset, base typography, focus ring, tabular numerals,
>   reduced motion, and the six `@font-face` declarations. Fonts are **self-hosted** from
>   `public/fonts/` (also new); there is no CDN request at first paint.
>
> A third small addition, `src/test/tokens-source.ts`, reads `tokens.css` from disk so the
> contrast and drift tests measure the real stylesheet rather than a copy of it.
>
> **Measured cost against the `< 250 KB gzip` initial-JS target** (`npm run build`,
> 2026-08-19, same toolchain): entry chunk **91.59 kB gzip** (+0.69), vendor chunk unchanged
> at 34.48 kB, so initial JS is **126.07 kB gzip** — half the budget still unspent. CSS grew
> to 30.49 kB gzip (+1.19). The six `woff2` latin subsets add **139.74 kB** on disk but are
> **outside** the initial-JS figure: they load through `@font-face` with `font-display: swap`,
> off the critical path.

Each feature folder holds `api.ts` (query/mutation hooks), `schema.ts` (Zod),
`types.ts`, `routes.tsx`, `pages/`, `components/`. Features never import from each other
— shared things move to `shared/`.

## Authentication flow

```
/login  ──POST /api/v1/auth/login { tenant, username, password }
          → { accessToken, refreshToken, user, tenant, rights[] }
          → accessToken in memory (Zustand)
          → refreshToken in an httpOnly, SameSite=Strict cookie   ← not localStorage
          → rights[] into the permission store
        ──► redirect to the last route or /dashboard

401 on any request → single-flight refresh → retry once → else hard logout
Idle timeout → warning modal → logout (replaces the broken singleton SessionTimeoutService)
```

> **Deliberate divergence from the Angular pilot**, which stores the JWT in `localStorage`
> (`frontend/vsmart-erp/src/app/core/auth/auth.service.ts`). That is XSS-exposed. Access
> token in memory + refresh token in an httpOnly cookie is the standard we adopt.

## Permission-based rendering

The server returns the user's full right set at login (`GET /api/v1/me`), shaped as:

```ts
type Right = 'view' | 'create' | 'edit' | 'delete';
type ScreenRights = Record<string /* ScreenName */, {
  view: boolean; create: boolean; edit: boolean; delete: boolean; hidden: boolean;
}>;
```


> **The endpoint exists as of 2026-08-20 (M2-A07), and this is its exact wire shape.** The
> `ScreenRights` type above is correct and unchanged — what follows is the envelope around it,
> so the permission store is built against a contract rather than a guess. Framework-neutral:
> it is the same JSON whichever client consumes it.
>
> ```jsonc
> // GET /api/v1/me   — requires a bearer token; no parameters of any kind
> {
>   "userId": 7,
>   "userName": "vivek",
>   "tenantId": 3,
>   "role": "Administrator",          // "Administrator" | "User" | "" — the JWT ClaimTypes.Role
>   "rights": {                        // ScreenRights, keyed by Screens.ScreenName verbatim
>     "Currency": { "view": true, "create": false, "edit": true, "delete": false, "hidden": false }
>   }
> }
> ```
>
> - **A screen with no `UserRight` row has no key.** The map is *not* padded to 152 all-`false`
>   entries. **A missing key means DENY** — the client's default must never be "allow"
>   (BR-AUTH-002). `forScreen('X')` on an absent key returns all-`false`, it does not throw.
> - Keys are **ordinal and case-sensitive**, matching `Screens.ScreenName` exactly.
> - `hidden` is `IsHide`. Navigation filters on `view && !hidden`; it is not a second gate.
> - `role` is a plain string. **`ERPAdmin` is not a role** — `NavMenu.razor:36,148` and
>   `Home.razor:240` name it but `UserRole` has only `Administrator` and `User` (R-31). A guard
>   written against `ERPAdmin` can never match; do not port it.
> - Fetch it **after login and on every hard reload**, never from persisted client state: rights
>   are deliberately absent from the JWT (ADR-004 §2) precisely so a permission change takes
>   effect without waiting out a token.
> - A `500` from this call is an **outage**, not "no permissions" — the server never degrades a
>   rights-load failure to an empty map, so the client must not render one as a locked-down UI.
> - **Presentation only (ADR-004 §3).** The server re-checks every request independently and
>   answers `403` with the frozen problem body under *Error handling*. Never treat this map as
>   security.

Three layers, all reading the same store:

```tsx
// 1. Route guard
<Route element={<RequireScreen screen="Sales Order" right="view" />}>
  <Route path="/sales/orders" element={<SalesOrderList />} />
</Route>

// 2. Control gate
<PermissionGate screen="Sales Order" right="create">
  <Button onClick={create}>New Sales Order</Button>
</PermissionGate>

// 3. Imperative
const { canEdit, canDelete } = useScreenRights('Sales Order');
```

Navigation items are filtered by `view && !hidden` — reproducing `IsHide` semantics.

**Client-side gating is a UX affordance only.** The server enforces the same matrix
independently (ADR-004). Never treat the client check as security.

## Data-fetching conventions

```ts
// queryKeys.ts — one factory, no ad-hoc string keys
export const qk = {
  salesOrders: {
    all:  ['sales-orders'] as const,
    list: (q: SalesOrderQuery) => ['sales-orders', 'list', q] as const,
    one:  (id: number)         => ['sales-orders', 'detail', id] as const,
  },
} satisfies QueryKeyTree;
```

Rules:
- **Lists are server-paged.** Grid state (page, size, sort, filters) lives in the URL
  query string, so a filtered grid is shareable and survives refresh.
- **Reference data** (currencies, UOM, states, GST rates, terms) uses a long `staleTime`
  and is prefetched once at shell mount.
- **Mutations invalidate by key prefix**, never by manual cache surgery.
- **Optimistic updates only for trivial toggles.** Never for anything touching money,
  stock, or document state — the server is the source of truth (BR-CALC-001, BR-STK-001).
- **Typeahead pickers** (`SearchCustomers`, `SearchItems`) use a debounced query with
  `placeholderData: keepPreviousData`.

## Document editor pattern (the core abstraction)

Every one of the ~65 Upsert screens is an instance of:

```
┌─ PageHeader: title · breadcrumbs · status badge · actions (Save/Cancel/Print/Approve) ─┐
├─ HeaderForm      RHF + Zod; party picker cascades defaults from the server            │
├─ LineItemGrid    virtualised, keyboard-first, add/edit/delete/reorder rows            │
│                  ├─ item typeahead → server returns HSN, UOM, last price, tax         │
│                  ├─ inline validation per row                                          │
│                  └─ "Pull from upstream document" → RecordPickerDialog                 │
├─ TotalsPanel     read-only; values from POST /documents/calculate — never computed here│
└─ Attachments · Terms · Remarks · Audit trail                                           │
```

Implemented **once** as `<DocumentEditor>` with a per-document configuration object
(fields, columns, pickers, commands, endpoints). Build it for Sales Order first, then each
subsequent document is configuration plus its own exceptions.

**This is the single highest-leverage decision in the frontend.** It converts ~65 screens
of 3,000–6,500 LOC each into one component plus 65 configs.

### Workflow commands

Cancel, short-close, approve, reject, release, post are **server commands**
(`POST /{id}/{verb}`), never client-orchestrated sequences. The client's job is to collect
the input (e.g. the mandatory cancellation reason of BR-SO-003), call one endpoint, and
render the outcome.

## Error handling

| Layer | Behaviour |
|---|---|
| Route | `errorElement` per route; full-page `ErrorState` with retry |
| Component | `react-error-boundary` around grids and editors |
| Network | Axios interceptor → normalised `ProblemDetails` |
| Business rule (409) | toast/inline alert showing the **server's message verbatim** — these strings are product UX |
| Validation (400) | map `errors` dictionary onto RHF field errors |
| Permission (403) | inline "You don't have permission to …", not a redirect |
| Auth (401) | silent refresh, then hard logout |
| Unexpected (500) | toast with `traceId`, logged to the monitoring sink |

## Performance targets

| Metric | Target |
|---|---|
| Initial JS (shell + login) | < 250 KB gzip |
| Route chunk | < 150 KB gzip |
| Grid: 10,000 rows | virtualised, 60 fps scroll |
| Line editor: 200 rows | typing latency < 50 ms |
| Time to interactive on the app shell | < 2 s on a mid-range laptop |

Every feature route is `React.lazy` + `Suspense`. The generated API client is tree-shaken
per feature.

## Accessibility

WCAG 2.2 AA as the standard: full keyboard operation of grids and pickers, visible focus,
labelled form controls, `aria-live` for toasts and async results, 4.5:1 contrast in both
themes, no colour-only status encoding. Details in
[`design-system.md`](design-system.md).

## What is deliberately *not* rebuilt in React

| Capability | Stays server-side |
|---|---|
| Document totals & tax | `CalculationService` (BR-CALC-001) |
| Stock FIFO allocation | `StockManagerService` (BR-STK-001) |
| Printed documents | FastReport `.frx` → PDF |
| Analytical reports | 94 stored procedures |
| Excel export/import | EPPlus / ClosedXML |
| e-Invoice / e-Way Bill | `E_Invoice/` helpers |
| Document numbering | repositories + running-number tables |
| All validation of record | services + `DataAnnotations` (Zod mirrors it for UX only) |
