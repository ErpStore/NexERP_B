---
doc_id: KB-050
title: Proposed Frontend Architecture (Angular)
module: frontend-new
source_files:
  - frontend/vsmart-erp/src/app/app.config.ts
  - frontend/vsmart-erp/src/app/app.routes.ts
  - frontend/vsmart-erp/src/app/core/auth/auth.service.ts
  - frontend/vsmart-erp/src/app/core/auth/auth.guard.ts
  - frontend/vsmart-erp/src/app/core/auth/auth.interceptor.ts
  - frontend/vsmart-erp/src/environments/environment.ts
  - frontend/vsmart-erp/src/environments/environment.prod.ts
  - V.SMART/V.SMART.Api/Middleware/ApiProblems.cs
  - V.SMART/V.SMART.Api/Middleware/ProblemTypes.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: [BR-CALC-001, BR-STK-001, BR-SO-001, BR-SO-003, BR-AUTH-002]
status: proposal
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-013, KB-015, KB-040, KB-041, KB-051, KB-105, ADR-002, ADR-004, ADR-007]
---

# Proposed Frontend Architecture

> **Rewritten for Angular on 2026-08-20 by task `M2-C00`.** [`ADR-007`](../decisions/ADR-007-angular-stack.md)
> selects **Angular + PrimeNG** and supersedes `ADR-003`; the partly-superseded banner that stood
> here until the rewrite is gone, superseded by this text. Nothing below instructs React, Vite,
> Mantine, TanStack, Zustand, React Hook Form or Zod as the thing to build.
>
> **The filename stays `react-architecture.md` deliberately.** ~20 task files and `INDEX.md` cite
> this path; renaming it would have to change all of them in the same commit for no behavioural
> gain, and the citation key that matters is the `doc_id`, which is and stays **KB-050**. The
> filename is a historical artefact, not a claim about the framework. If it is ever renamed, every
> citation moves in that one commit.

> **Proposal.** Nothing here describes existing code, except where a `file:line` citation says
> otherwise — the Angular pilot and the API error contract both exist today. Constraints it must
> satisfy come from [`architecture/frontend-architecture-existing.md`](../architecture/frontend-architecture-existing.md)
> and [`architecture/auth-and-permissions.md`](../architecture/auth-and-permissions.md).

## Design constraints from the existing system

*Unchanged by the framework switch — these are facts about the ERP, not about React or Angular.*

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

**[ADR-007](../decisions/ADR-007-angular-stack.md) is authoritative for every row below.** This
table is a copy of ADR-007's decision table (`ADR-007-angular-stack.md:86-103`) with only the
"Change from ADR-003" column dropped. **The rationale is not restated here** — it lives in ADR-007
§*Key rationales*, and a second copy of it is a second thing to keep in sync. If this table and
ADR-007 ever disagree, ADR-007 wins and this document is the one that is wrong.

| Concern | Choice |
|---|---|
| Framework | **Angular**, standalone components, `strict` TypeScript |
| Exact major version | **Angular 22.x** — owner decision *"use the latest Angular version"*, 2026-08-20 |
| Build | **Angular CLI** (esbuild) |
| Routing | **Angular Router**, functional guards |
| Server state | **Typed Angular services over `HttpClient`**, explicit refetch |
| Client state | **Angular signals** in services |
| Forms | **Typed Reactive Forms** |
| Validation | Angular validators, shapes **generated from OpenAPI** |
| UI library | **PrimeNG** — one library, never mixed |
| Styling | **CSS-variable design tokens**, component styles |
| Tables | **PrimeNG Table**; `LineItemGrid` re-evaluated, see below |
| Charts | PrimeNG Charts |
| HTTP | `HttpClient` + interceptors, **generated OpenAPI client** |
| Money display arithmetic | **decimal.js** |
| i18n | **Runtime-switchable** (`ngx-translate` or equivalent), from day one |
| Testing | **Jest or Vitest + Angular Testing Library + Playwright** |

The *"see below"* in the **Tables** row is ADR-007's own forward reference to its
§*Key rationales* (`ADR-007-angular-stack.md:144-152`) — **not** a pointer into this document. It
is copied verbatim rather than resolved so the two tables stay byte-comparable; read it there.

**"Angular 22.x" is the major, not a patch pin.** ADR-007 measured `@angular/core` 22.1.3,
`@angular/cli` 22.1.5 and `primeng` 22.1.0 against the npm registry on 2026-08-20
(`ADR-007-angular-stack.md:105-117`) and deliberately records the **major** only, because pinning a
patch guarantees the document is wrong within weeks. `M2-C01` runs `ng version` and records the
exact patch it installs. **The pilot at `frontend/vsmart-erp/` is three majors behind at 19.2**, so
"adopt the pilot" is not "adopt as-is" — ADR-007 recommends scaffolding fresh on 22 and porting its
9 components, and leaves the call to `M2-C11` (`ADR-007-angular-stack.md:119-130`).

Three things ADR-007 records as **carried over unchanged**, repeated here only because tasks cite
this document for them: server-authoritative validation / calculation / permissions / document
numbering; the OpenAPI-generated API client; and the CSS-variable design tokens of
[KB-051](design-system.md), including the eight WCAG contrast corrections and the 12 px workhorse
type scale (`ADR-007-angular-stack.md:164-169`).

### Framework-agnostic choices ADR-007 does not decide

These were in the original KB-050 stack table, were never React decisions, and ADR-007 neither
adopts nor overrides them. They remain **proposal-level** — the task that first needs one picks it
and records the pick.

| Concern | Proposal | Note |
|---|---|---|
| Dates | `date-fns` (+ `date-fns-tz`) | Financial-year helpers mirror `FinancialYearHelper.cs` |
| Tables → Excel | server endpoint | Reuse `ExcelExportService` |
| PDF viewing | native `<embed>` / blob URL | The server generates the PDF |
| Notifications | PrimeNG Toast | Follows from "one component library, never mixed" |
| Quality gates | ESLint + Prettier; CI: typecheck → lint → unit → build → Playwright | Shape proven by the pre-ADR-007 `M2-C01` CI job, re-pointed at Angular |

*Removed in this rewrite:* the **"Why not Next.js"** and **"Why Mantine over MUI"** sections. Both
were React-internal questions. Angular CLI is the build and PrimeNG is the component library, both
by ADR-007. Nothing replaces them — SSR is not proposed here and is not decided by any accepted ADR.

## Project structure

Feature-sliced, mirroring the ERP module map so the codebase is navigable by domain experts, and
following the layering the Angular pilot already uses (`core/` · `shared/` · `features/` ·
`layout/`, confirmed on disk under `frontend/vsmart-erp/src/app/`).

```
src/
  main.ts                        bootstrapApplication(AppComponent, appConfig)
  app/
    app.config.ts                providers: router, HttpClient + interceptors,
                                 PrimeNG theme, animations, i18n
    app.routes.ts                root route tree; feature routes lazy-loaded
    app.component.ts             root host
    core/                        singletons, provided in root
      auth/                      auth.service.ts, auth.guard.ts, auth.interceptor.ts,
                                 permission.service.ts, has-right.directive.ts
      http/                      error.interceptor.ts, correlation.interceptor.ts,
                                 tenant.interceptor.ts
      api/generated/             OpenAPI-generated client + types (do not edit)
      config/                    environment access, feature flags
    shared/                      stateless, reusable, imports no feature
      components/                design-system primitives (see design-system.md)
        data-grid/  line-item-grid/  record-picker-dialog/  page-header/  form-field/
        confirm-dialog/  busy-overlay/  status-badge/  empty-state/  error-state/
      directives/  pipes/  models/  utils/
    layout/
      shell/  auth-layout/  print-layout/
    features/                    one folder per ERP module, each lazily routed
      masters/
        currency/  customer/  vendor/  item/  bom/  store/  hsn/  ...
      sales/
        leads/  enquiry/  feasibility/  quotation/  sales-order/  contract-review/
        proforma-invoice/
      manufacturing/  labour/  outsourcing/  purchase/  subcontract/
      planning/  production/  inventory/  inspection/  maintenance/
      hr/  accounts/  reports/  dashboard/  settings/  utilities/
  environments/                  environment.ts / environment.prod.ts - see the defect below
  styles/                        tokens.css, global styles
  assets/  i18n/                 runtime translation bundles
```

Rules, each of which the pilot either already honours or is the counter-example for:

- **Standalone components only.** No `NgModule`. The pilot bootstraps this way already
  (`frontend/vsmart-erp/src/app/app.config.ts:10-25`).
- **Every feature route is lazy** (`loadComponent` / `loadChildren`). The pilot eagerly imports all
  four of its routed components (`frontend/vsmart-erp/src/app/app.routes.ts:3-6`) — acceptable at
  9 components, **not** acceptable at ~150 screens, and the single clearest reason its structure is
  a baseline rather than a template.
- **Features never import from each other.** Anything two features need moves to `shared/`;
  anything that must be a singleton lives in `core/`.
- **Each feature folder holds** `*.routes.ts`, a typed `*.service.ts` over `HttpClient`, `models/`,
  and its page and component folders. Validators are generated from OpenAPI, not hand-written.

> **`frontend/nexgen-web/` still exists on disk** — the React scaffold built by the pre-ADR-007
> `M2-C01` and discarded by ADR-007 (`ADR-007-angular-stack.md:194-195`). **Whether it is deleted
> or left dormant is the re-scoped `M2-C01`'s decision, not this document's**
> (`ADR-007-angular-stack.md:223-225`). Nothing in this document is realised by that tree; the
> "realised at `frontend/nexgen-web/`" reconciliation note and the design-token extension note that
> stood in this section described React artefacts and were removed with the rest of the React text.

## Authentication flow

```
/login  --POST /api/v1/auth/login { tenant, username, password }
          -> { accessToken, refreshToken, user, tenant, rights[] }
          -> rights[] into the permission service (a signal)
        --> redirect to the returnUrl or /dashboard

Every request        -> functional HttpInterceptorFn attaches Authorization: Bearer <token>
                        (plus tenant / correlation headers)
Every guarded route  -> functional CanActivateFn; a denial returns a UrlTree to /login
401 on any request   -> single-flight refresh -> retry once -> else hard logout
Idle timeout         -> warning dialog -> logout (replaces the broken singleton
                        SessionTimeoutService)
```

The Angular shapes are the pilot's, and as shapes they are right: a `CanActivateFn` returning
`true` or a `UrlTree` (`frontend/vsmart-erp/src/app/core/auth/auth.guard.ts:11-20`), and an
`HttpInterceptorFn` cloning the request with a `Bearer` header
(`frontend/vsmart-erp/src/app/core/auth/auth.interceptor.ts:12-24`), registered through
`provideHttpClient(withInterceptors([...]))` (`frontend/vsmart-erp/src/app/app.config.ts:14`).

### Token storage is an open decision owned by `M2-C02`

**This document does not decide where the token lives, and no later task may treat any storage
mechanism as settled by reading this section.** The decision belongs to **`M2-C02`**, taken against
[ADR-004](../decisions/ADR-004-server-side-authorization.md) and recorded there — ADR-007 assigns it
explicitly (`ADR-007-angular-stack.md:178-180`: *"`M2-C02` decides the token storage model … and
must not copy the pilot's approach by default"*).

The binding constraints on that decision:

- **The pilot's `localStorage` JWT is explicitly not endorsed and must not be carried forward.**
  `frontend/vsmart-erp/src/app/core/auth/auth.service.ts:29-35,60-61,66-72` stores both the token
  (`TOKEN_KEY = 'vsmart_jwt'`) and the user object in `localStorage`. Any script executing in the
  page can read it — XSS-exposed, flagged by ADR-003 and re-flagged by ADR-007.
- **The client is never the enforcement point.** ADR-004 keeps the server authoritative for every
  right check, so this is a token-theft question, not an authorisation question.
- Whatever is chosen must still support the single-flight refresh and hard-logout behaviour above.

Until `M2-C02` records its decision this is an **Unknown**, not an omission.

### Environment configuration — a defect in the pilot, not a pattern

`frontend/vsmart-erp/src/environments/environment.ts:1-5` and
`frontend/vsmart-erp/src/environments/environment.prod.ts:1-4` **both** hardcode
`apiBaseUrl: 'http://localhost:5144'` — confirmed identical, i.e. **the pilot's production build
points at localhost**. ADR-007 names this *"a defect to remove, not a pattern to keep"*
(`ADR-007-angular-stack.md:182-184`).

The replacement rule: the API base URL is **configuration, not source**, and a missing value fails
loudly at startup rather than silently defaulting to a developer's machine. The scaffold task owns
the mechanism; no file under `src/**` may contain a hardcoded API host.

## Permission-based rendering

The server returns the user's full right set at login (`GET /api/v1/me`), shaped as:

```ts
export type Right = 'view' | 'create' | 'edit' | 'delete';

export interface ScreenRight {
  view: boolean; create: boolean; edit: boolean; delete: boolean; hidden: boolean;
}

export type ScreenRights = Record<string /* ScreenName */, ScreenRight>;
```

Three layers, all reading the same `PermissionService` signal. **The 152 × 5 matrix is unchanged;
only the rendering syntax changes** — a structural directive instead of a wrapper component.

```ts
// 1. Route guard - a functional CanActivateFn produced by a factory
{
  path: 'sales/orders',
  canActivate: [requireScreen('Sales Order', 'view')],
  loadComponent: () => import('./sales-order-list.component')
                         .then(m => m.SalesOrderListComponent),
}
```

```html
<!-- 2. Control gate - a structural directive, not a wrapper component -->
<button pButton *appHasRight="'Sales Order'; right: 'create'" (click)="create()">
  New Sales Order
</button>
```

```ts
// 3. Imperative - signals on the injected service
private readonly rights = inject(PermissionService).forScreen('Sales Order');
protected readonly canEdit = computed(() => this.rights().edit);
```

Navigation items are filtered by `view && !hidden` — reproducing `IsHide` semantics.

**Client-side gating is a UX affordance only.** The server enforces the same matrix independently
([ADR-004](../decisions/ADR-004-server-side-authorization.md), BR-AUTH-002) and answers `403` with
the frozen body described under *Error handling*. Never treat the client check as security.

## Data-fetching conventions

**No query library.** ADR-007 replaces TanStack Query with typed Angular services over `HttpClient`
that hold state in signals and expose an explicit `refresh()`; the rationale is ADR-007's and is not
restated here. The shape:

```ts
@Injectable({ providedIn: 'root' })
export class SalesOrderService {
  private readonly http = inject(HttpClient);

  private readonly listState = signal<SalesOrderListPage | null>(null);
  readonly list = this.listState.asReadonly();
  readonly loading = signal(false);

  /** Explicit, and the only thing that changes the list. No cache keys, no invalidation graph. */
  async refresh(query: SalesOrderQuery): Promise<void> { /* GET, then listState.set(...) */ }

  async create(vm: SalesOrderVM): Promise<number> { /* POST, then refresh() */ }
}
```

Rules:

- **Lists are server-paged.** Grid state (page, size, sort, filters) lives in the URL query string
  (`ActivatedRoute` query params), so a filtered grid is shareable and survives a reload.
- **Reference data** (currencies, UOM, states, GST rates, terms) is loaded once by a `core/` service
  at shell start and held in a signal; it is not re-fetched per screen.
- **A mutation is followed by an explicit `refresh()`** of the affected service. There is no cache
  to invalidate and no manual cache surgery to get wrong.
- **Optimistic updates only for trivial toggles.** Never for anything touching money, stock or
  document state — the server is the source of truth (BR-CALC-001, BR-STK-001).
- **Typeahead pickers** (`SearchCustomers`, `SearchItems`) debounce with RxJS (`debounceTime` +
  `switchMap`) and keep the previous result visible while the next one loads. RxJS stays where
  ADR-007 puts it: HTTP and event streams, not component state.
- **Every call goes through the generated OpenAPI client**, so the frontend cannot drift from the
  API silently.

## Document editor pattern (the core abstraction)

*The core abstraction, and a shape rather than a library — unchanged by the framework switch.*

Every one of the ~65 Upsert screens is an instance of:

```
+- PageHeader: title . breadcrumbs . status badge . actions (Save/Cancel/Print/Approve) -+
|- HeaderForm      typed Reactive Form; party picker cascades defaults from the server   |
|- LineItemGrid    virtualised, keyboard-first, add/edit/delete/reorder rows              |
|                  |- item typeahead -> server returns HSN, UOM, last price, tax          |
|                  |- inline validation per row                                           |
|                  +- "Pull from upstream document" -> RecordPickerDialog                 |
|- TotalsPanel     read-only; values from POST /documents/calculate - never computed here |
+- Attachments . Terms . Remarks . Audit trail                                            |
```

Implemented **once** as `<app-document-editor>` taking a per-document configuration object (fields,
columns, pickers, commands, endpoints) as a typed input. Build it for Sales Order first; each
subsequent document is then configuration plus its own exceptions.

**This is the single highest-leverage decision in the frontend.** It converts ~65 screens of
3,000–6,500 LOC each into one component plus 65 configs.

### Workflow commands

Cancel, short-close, approve, reject, release and post are **server commands**
(`POST /{id}/{verb}`), never client-orchestrated sequences
([ADR-002 §3](../decisions/ADR-002-rest-api-layer.md)). The client's job is to collect the input
(e.g. the mandatory cancellation reason of BR-SO-003), call one endpoint, and render the outcome.

## Error handling

**Rewritten 2026-08-20 against the shipped contract.** The section that stood here predated
`M2-A06`; the real contract now exists in code. The authorities are
[ADR-002 §4](../decisions/ADR-002-rest-api-layer.md) and
`V.SMART/V.SMART.Api/Middleware/ApiProblems.cs`, described in its own doc comment as *"the one
place that builds every error body this API returns"* (`ApiProblems.cs:7-13`). Where this section
and that file disagree, the file wins.

Invariants, **Confirmed**:

- Every error response carries the media type `application/problem+json` (`ApiProblems.cs:16`).
- Every error body carries `traceId` (`ApiProblems.cs:43`; the `403` sets it at `ApiProblems.cs:86`
  and the `400` at `ApiProblems.cs:131`), plus `type`, `title`, `status` and `instance` — the
  request path (`ApiProblems.cs:40`).
- `type` is a stable identifier URI under `https://api.v-smart.local/problems/`
  (`ProblemTypes.cs:17`), **not** a dereferenceable URL. Branch on `type`, never on `title`.

| Status | `type` (`ProblemTypes.cs`) | Server behaviour, with evidence | Client behaviour |
|---|---|---|---|
| **400** validation | `validation-failed` (`:20`) | `ValidationProblemDetails` with an `errors` dictionary **keyed by field**, carrying the `DataAnnotations` messages verbatim (`ApiProblems.cs:117-133`) | Map `errors` onto the Reactive Form controls by key; show a summary only for keys that match no control |
| **400 / 503** tenant | `tenant-unresolved` (`:38`) | 400 on the login path, 503 when it surfaces mid-request; **no connection string, host name or configuration value ever appears in the body** (`ApiProblems.cs:90-102`) | Login: inline "tenant not recognised". Mid-request: a retryable full-page error. Never present `detail` as a diagnostic hint |
| **401** | `unauthenticated` (`:23`), `invalid-token` (`:26`) | Deliberately uninformative — the caller learns no more than the old `"Invalid username or password."` (`ApiProblems.cs:59-64`) | Single-flight refresh, retry once, else hard logout. Never explain *why* |
| **403** screen right | `screen-right-denied` (`:29`) | **Frozen shape, one producer only** (KB-105 §7.1). `title` is the constant `"Screen right denied."`; `detail` is composed from the **required** screen and right only, never from what was found; extensions `screen` and `right` (`ApiProblems.cs:66-88`) | Inline "You don't have permission to …" — **not** a redirect and not a logout. Read `screen`/`right` from the extensions. A `403` on a route the client believed was permitted means the client's right set is stale: refresh it |
| **404** | `not-found` (`:32`) | Minimal (`ApiProblems.cs:55-57`) | Full-page "not found" for a route; inline for a lookup |
| **409** business rule | `business-rule` (`:35`) | **The service's own message is carried into `title` verbatim — not reworded, not prefixed, not truncated** (`ApiProblems.cs:47-53`). Those strings are product UX written by the domain team (BR-SO-001; ADR-002 §4 gives *"Cannot delete this Sales Order as a Sales DC transaction exists."* as the pattern) | Show `title` **exactly as received**, in a toast or inline alert. Never substitute friendlier text, never append, never translate, never truncate. A hard rule, not a default |
| **500** | `unhandled` (`:41`) | `traceId` and a constant title only — no exception message, type, stack trace or inner exception, **in any environment** (`ApiProblems.cs:104-115`) | Toast the constant title with the `traceId` visible and copyable; log to the monitoring sink |

Layers that implement the table:

| Layer | Responsibility |
|---|---|
| `core/http/error.interceptor.ts` | The single place that parses a problem body into a typed `ApiProblem`. Nothing else parses error bodies |
| Route | A route-level error component: full-page `ErrorState` with retry |
| Component | Grids and editors render an inline error surface rather than unmounting |
| Global | An `ErrorHandler` override for unhandled *client-side* exceptions — distinct from server problems, and never presented as one |

## Performance targets

*Unchanged — framework-agnostic budgets.*

| Metric | Target |
|---|---|
| Initial JS (shell + login) | < 250 KB gzip |
| Route chunk | < 150 KB gzip |
| Grid: 10,000 rows | virtualised, 60 fps scroll |
| Line editor: 200 rows | typing latency < 50 ms |
| Time to interactive on the app shell | < 2 s on a mid-range laptop |

Every feature route is lazily loaded (`loadComponent` / `loadChildren`), and the generated API
client is tree-shaken per feature. **The measured bundle baselines previously recorded here were
React figures and have been removed** — they say nothing about an Angular build. The Angular
scaffold task re-measures against the same targets, and that measurement becomes the new baseline.

## Accessibility

WCAG 2.2 AA as the standard: full keyboard operation of grids and pickers, visible focus,
labelled form controls, `aria-live` for toasts and async results, 4.5:1 contrast in both
themes, no colour-only status encoding. Details in [`design-system.md`](design-system.md), whose
eight contrast corrections and type scale carry over to Angular unchanged
(`ADR-007-angular-stack.md:167-169`).

## What is deliberately *not* rebuilt in the SPA

> Heading renamed 2026-08-20 from *"… in React"*. `M2-C08-01` (×2) and `M2-C08-02` still link the
> old `#what-is-deliberately-not-rebuilt-in-react` anchor; both files are stale under ADR-007 and
> the links are corrected when those tasks are re-specified. No other section anchor changed, so
> every other citation in the ~20 files that reference this document still resolves.

| Capability | Stays server-side |
|---|---|
| Document totals & tax | `CalculationService` (BR-CALC-001) |
| Stock FIFO allocation | `StockManagerService` (BR-STK-001) |
| Printed documents | FastReport `.frx` → PDF |
| Analytical reports | 94 stored procedures |
| Excel export/import | EPPlus / ClosedXML |
| e-Invoice / e-Way Bill | `E_Invoice/` helpers |
| Document numbering | repositories + running-number tables |
| All validation of record | services + `DataAnnotations` (client validators mirror it for UX only) |

## The Angular pilot — what to keep, what not to copy

`frontend/vsmart-erp/` is an Angular 19.2 + PrimeNG 19.1 pilot (`frontend/vsmart-erp/package.json`:
`@angular/core ^19.2.0`, `@angular/cli ^19.2.27`, `primeng ^19.1.4`, `typescript ~5.7.2`). ADR-007
adopts it as the baseline and `M2-C11` inherits it — as **patterns to port onto Angular 22**,
not a tree to adopt at 19.2 (`ADR-007-angular-stack.md:119-130`). It holds 9 components across
`core/auth`, `features/auth/login`, `features/currency` (list, form, service, models) and
`layout/shell` — the full file inventory is **INV-021** and is not repeated here.

**Gets it right — adopt as-is:**

| Pattern | Evidence |
|---|---|
| Standalone bootstrap through an `ApplicationConfig` provider array | `frontend/vsmart-erp/src/app/app.config.ts:10-25` |
| `provideHttpClient(withInterceptors([...]))` — functional interceptors | `frontend/vsmart-erp/src/app/app.config.ts:14` |
| Functional `CanActivateFn` returning `true` or a `UrlTree`, not a class guard | `frontend/vsmart-erp/src/app/core/auth/auth.guard.ts:11-20` |
| Functional `HttpInterceptorFn` cloning the request with a `Bearer` header | `frontend/vsmart-erp/src/app/core/auth/auth.interceptor.ts:12-24` |
| Signals for auth state, exposed read-only via `asReadonly()` / `computed()` | `frontend/vsmart-erp/src/app/core/auth/auth.service.ts:34-40` |
| `core/` · `features/` · `layout/` · `shared/` layering | `frontend/vsmart-erp/src/app/` tree |
| PrimeNG configured once, with a single theme preset | `frontend/vsmart-erp/src/app/app.config.ts:16-23` |

**Do not copy:**

| Anti-pattern | Evidence | Why |
|---|---|---|
| JWT and user object in `localStorage` | `core/auth/auth.service.ts:29-35,60-61,66-72` | XSS-exposed. Storage is `M2-C02`'s decision — see *Token storage* above |
| `http://localhost:5144` hardcoded in **both** environment files | `environments/environment.ts:1-5`, `environments/environment.prod.ts:1-4` | The production build points at localhost. A defect to remove, not a pattern to copy |
| Eager `import` of every routed component | `app.routes.ts:3-6` | Fine at 9 components, fatal to the bundle budget at ~150 screens. Lazy `loadComponent` from the first feature onward |
| The auth URL built by string concatenation inside the service | `core/auth/auth.service.ts:53-58` | The API client is generated from OpenAPI; hand-built URLs drift silently |
| `api/auth/login` — an unversioned path | `core/auth/auth.service.ts:54` | ADR-002 versions the API (`/api/v1/...`). The pilot predates it |
| Karma + Jasmine | `package.json` (`karma ~6.4.0`, `jasmine-core ~5.6.0`) | Karma is deprecated; ADR-007 moves testing to Jest or Vitest + Angular Testing Library |
| No permission gating of any kind | no `PermissionService` or right directive exists anywhere in the pilot | The 152 × 5 matrix is not optional. `M2-C02` introduces it |
| Committed build output (`dist/`, `.angular/cache/`) | R-14 (KB-060) | Never reproduce it in the new app |
