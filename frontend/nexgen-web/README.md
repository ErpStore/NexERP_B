# nexgen-web — the NexGen ERP SPA

Angular + PrimeNG frontend for V.SMART, created by task **M2-C01** under
[ADR-007](../../docs/kb/decisions/ADR-007-angular-stack.md). It replaces the React scaffold that
previously occupied this directory; ADR-007 discarded that stack on 2026-08-20.

Today it renders **one real destination, `/dashboard`**, behind login, a permission check and
the real app shell. Authentication and permissions are `M2-C02`; the shell — header, sidebar,
breadcrumbs, ⌘K palette — is `M2-C03` (both implemented 2026-08-27, `Needs Review` — see
[Authentication and permissions](#authentication-and-permissions) and
[Navigation and shell](#navigation-and-shell)); design tokens are `M2-C04-01`; the generated
OpenAPI client is `M2-B10`. Nothing here computes anything: the server stays authoritative for
validation, calculations, permissions and document numbering.

## Toolchain actually observed at scaffold time (2026-08-21, Windows)

| Thing           | Version                                                                                                                                       |
| --------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| Node.js         | `v24.19.0` (`node --version`) — `.nvmrc` pins major **24**                                                                                    |
| npm             | `11.17.0`                                                                                                                                     |
| Angular CLI     | `22.1.5` (`ng version`)                                                                                                                       |
| Angular runtime | `@angular/core` 22.1.3 resolved from `^22.1.0`                                                                                                |
| PrimeNG         | `22.1.0`                                                                                                                                      |
| TypeScript      | `6.0.3` resolved from `~6.0.2` — `@angular/compiler-cli@22.1.3` peers `typescript >=6.0 <6.1`, so `typescript@latest` (7.x) does **not** work |

These are recorded, not assumed. ADR-007 pins the **major**; the patches above are what this
workspace actually installed.

## Commands

Run all of these from `frontend/nexgen-web/`.

| Command                                   | What it does                                                                                                    |
| ----------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `npm ci`                                  | Install exactly what `package-lock.json` pins. CI never uses `npm install`.                                     |
| `npm start`                               | Dev server on <http://localhost:4200>.                                                                          |
| `npm run typecheck`                       | `tsc --noEmit` over the app, spec and e2e projects.                                                             |
| `npm run lint`                            | `ng lint --max-warnings=0` — type-aware `typescript-eslint` plus the Angular template a11y rules.               |
| `npm run format` / `npm run format:check` | Prettier. No Husky hook; CI is the gate.                                                                        |
| `npm run test` / `npm run test:ci`        | Unit tests (`test:ci` adds `--no-watch`).                                                                       |
| `npm run build`                           | Production build with the budgets in `angular.json`.                                                            |
| `npm run e2e`                             | Playwright smoke spec. It starts the dev server itself; run `npx playwright install chromium` once per machine. |

## The unit runner: Vitest, and why

ADR-007 left the choice open — _"the scaffold task picks Jest or Vitest and records why"_
(`ADR-007-angular-stack.md:154-155`). **Vitest.**

- It is the Angular CLI's own default at 22.1.5: `ng new --test-runner` offers `karma` and
  `vitest` and defaults to `vitest`. The CLI generates and maintains the wiring
  (`@angular/build:unit-test`), so there is no hand-rolled Jest transform to keep alive across
  Angular majors. Jest would mean owning that seam ourselves for no gain here.
- Karma is deprecated and is explicitly ruled out by ADR-007 and by M2-C01.
- Vitest was already the runner on the discarded React scaffold, so the one piece of local
  familiarity carries over.

[Angular Testing Library](https://testing-library.com/docs/angular-testing-library/intro) is
installed alongside it and used throughout — e.g.
`src/app/features/dashboard/dashboard.component.spec.ts`; `src/app/app.component.spec.ts` uses
`TestBed` directly because it asserts on the composed provider set rather than on rendered
output.

## API base URL — configuration, not source

**No file under `src/` contains an API host, and there is no default.** The Angular pilot at
`frontend/vsmart-erp/` hardcodes a `http://localhost` dev origin in _both_ `environment.ts` and
`environment.prod.ts`, so its production build points at a developer's laptop. ADR-007 calls that
_"a defect to remove, not a pattern to keep"_ (`ADR-007-angular-stack.md:182-184`).

The mechanism here:

1. `src/environments/environment.ts` / `environment.prod.ts` carry only `production` and the
   **path** of the runtime configuration document. No host, no scheme, no port.
2. `provideAppInitializer(bootstrapApp)` (`src/app/core/api/app-bootstrap.ts`, **M2-C02**) fetches
   `config/app-config.json` **before the app renders** (`src/app/core/config/app-config.ts`), then
   wires `ApiConfiguration.rootUrl` — what every generated client call actually targets — to it,
   then runs the silent-auth bootstrap (see [Authentication and
   permissions](#authentication-and-permissions) below). All three steps are sequenced with real
   `await`s in one function, not three separate initializers: Angular runs every
   `provideAppInitializer` factory **concurrently** (`Promise.all`), so two separate initializers
   here would race and a login/refresh call could fire before `rootUrl` was set.
3. If the config document cannot be fetched, or its `apiBaseUrl` is missing or blank, the
   initializer **throws and bootstrap fails loudly**. It never falls back to a host.
4. `public/config/app-config.json` ships `"apiBaseUrl": "/api"` — a _same-origin relative path_,
   not a host. Deployments that serve the API from another origin overwrite this one file; nothing
   is rebuilt. `public/config/app-config.example.json` documents the shape.

## Authentication and permissions

Built by **M2-C02** (`src/app/core/auth/`, `src/app/core/http/`, `src/app/features/auth/`).

**The client permission store is for RENDERING ONLY — it is not a security boundary.** The
server re-checks the caller's `UserRight` rows on every request
([ADR-004](../../docs/kb/decisions/ADR-004-server-side-authorization.md) §3); hiding a button
here is a UX affordance, never enforcement. `PermissionService`'s own file header,
`HasRightDirective`'s TSDoc and `forScreen()`'s TSDoc all repeat this so it cannot be missed by
reading only one of them.

- **Token custody: both the access token and the refresh token live in memory only**
  (`TokenStore`), never `localStorage`, never `sessionStorage`, never a public signal. This
  deviates from this task's own recommended default (access token in memory, refresh token in
  an httpOnly cookie) because the real `POST /api/v1/auth/refresh` returns the refresh token as
  a plain response-body string, not a `Set-Cookie` header — building the cross-origin cookie
  transport is `M2-A05`'s scope. **Consequence: a hard page reload always ends the session** —
  `bootstrap()` finds no refresh token on a fresh load and settles straight to `anonymous`. A
  full login → refresh → logout cycle is asserted (by spying on `Storage.prototype.setItem`) to
  write nothing to either Web Storage.
- **Deny-by-default, matching `RightsHelper.cs`.** A screen the caller holds no `UserRight` row
  for has no key in `PermissionService`'s rights map at all — `forScreen(name)` returns every
  field `false` for a missing key, never `undefined` treated as "allow".
- **Three permission-reading surfaces, one service:** the `requireScreen(screen, right)`
  `CanActivateFn` factory only gates _authentication_ — an authenticated caller always
  activates the route regardless of the specific right, because `CanActivateFn` can only
  return `true`/`false`/`UrlTree`, with no way to "activate but render something else."
  Rendering the denial is the **routed component's own job**: read
  `PermissionService.forScreen(name)` and branch to `app-permission-denied-state`, exactly as
  `PlaceholderComponent` does for its own `requireScreen('Dashboard', 'view')` route. The
  `*appHasRight` structural directive and `forScreen()` itself are the same pattern at
  control-level and imperative-signal granularity respectively.
- **Single-flight refresh.** `authInterceptor` attaches `Authorization: Bearer <token>` to every
  non-`/api/v1/auth/*` request; a 401 triggers one shared in-flight refresh for every concurrent
  waiter, each original request is retried exactly once, and a failed refresh performs a hard
  logout with no second attempt.
- **Idle timeout replaces `SessionTimeoutService`, not ports it.** That Blazor service is
  `AddSingleton` with one shared `_lastActivity` field — every concurrent user shares one idle
  clock (R-17). `IdleTimeoutService` is `providedIn: 'root'` with every field an instance field,
  per browser tab, regression-tested by constructing two instances directly in one test.
- **No `/register` route, page, form or link.** `Q-09` (is self-registration still wanted) was
  answered **No** by the repository owner on 2026-08-27 — a self-registered user would land
  deny-by-default with zero `UserRight` rows and an empty application.
- **Known gaps, disclosed rather than fixed here:** no automated `axe` accessibility scan runs
  over `/login` or the idle-warning dialog (keyboard operability and focus behaviour are
  covered by unit tests, not an automated accessibility-tree scan). `npm run e2e` now also
  covers a full (network-mocked) login → shell → logout pass, added by `M2-C03` — see
  [Navigation and shell](#navigation-and-shell) — but it is still mocked at the network layer,
  not against a live backend + database, since none is reachable from a plain dev checkout.

## Navigation and shell

Built by **M2-C03** (`src/app/layout/`, `src/app/core/navigation/`,
`src/app/shared/components/{sidebar,nav-group,nav-item,header,user-menu,
financial-year-selector,breadcrumbs,page-header,tabs,command-palette}/`,
`src/app/features/dashboard/`).

- **The sidebar is filtered by SCREEN RIGHTS (`view && !hidden`), never by role.**
  `RightsHelper.cs`'s deny-by-default is reproduced exactly: an item whose `screenName` has
  no row in the rights map, or `view: false`, or `hidden: true`, is filtered out.
  `NavMenu.razor:36,148`'s `Roles="Administrator,ERPAdmin,User"` gate (R-31, KB-060) is
  **not** reproduced — role is never read anywhere in the navigation code, enforced by a
  repo-wide static scan (`sidebar.roles.spec.ts`).
- **`core/navigation/navigation.config.ts` is the one nav data source** — the sidebar and
  the ⌘K command palette both read it through `NavFilterService`, never duplicate it. Built
  from the _expanded_ `<MudNavMenu>` tree in `NavMenu.razor`, not the mini-rail's `NavGroups`
  dictionary, which genuinely disagrees with it on two points — see the file's own header
  comment and INV-033 (`docs/kb/investigation-registry.md`) for the full finding, including
  two likely copy-paste `ScreenName` defects in the Blazor source reproduced verbatim, not
  silently corrected.
- **`shared/components/` still never imports the authentication module** — the same rule
  `M2-C02` established. `NavFilterService` (`core/navigation/`) is the one seam that reads
  `PermissionService`; `SidebarComponent` and the command palette read `NavFilterService`
  instead. Caught by the same repo-wide scan test that already polices this for
  `permission-denied-state.component.ts`.
- **The command palette (⌘K / Ctrl+K) searches only the permission-filtered tree** — a
  screen the caller lacks rights to is unreachable through it, even by exact name, or the
  palette would be a directory of the tenant's configuration a caller cannot otherwise see.
  Fuzzy matching is a small, dependency-free subsequence matcher
  (`core/navigation/fuzzy-match.ts`), not a library.
- **Only `/dashboard` is a real, registered destination route today.** `navigation.config.ts`
  is nav _data_ for all ~145 items INV-033 mapped; `app.routes.ts` does not yet declare
  routes for the other ~144 — those are `M2-D01` onward's job, one screen at a time. Clicking
  an unbuilt item today falls through to the wildcard route back to `/`, which is the honest
  state of "the frame exists, the screens do not yet."
- **Responsive by real breakpoint** (`core/theme/breakpoint.service.ts`, KB-051): `≥1440`
  full 240 px sidebar (user-toggled between that and the 56 px rail, persisted);
  `1024–1439` forces the rail; `768–1023` becomes an overlay drawer (`p-drawer`); `<768`
  still renders the full shell (KB-051's "read-and-approve only" is an application-wide
  policy for that band, not a shell-rendering difference). The `sm`-band drawer's own
  rendering was not exercised in a real narrow-viewport browser session — disclosed, not
  hidden; see `docs/kb/execution/tasks/M2-C03.md`'s Close-out.
- **`unsavedChangesGuard` / `useBeforeUnloadGuard`** (`core/navigation/unsaved-changes.guard.ts`)
  replace `UnsavedChangesModal.razor`, built and unit-tested but with no real consumer yet —
  `M2-C08` (the document editor) is the first candidate.
- **Known gaps, disclosed rather than fixed here:** no automated `axe` scan over the shell;
  the header's "tenant name" is `Tenant ${tenantId}`, not a real display name — `/me`
  deliberately carries none (`MeController.cs`'s own doc comment, R-01).

## Bundle baseline (`npm run build`, 2026-08-27, after M2-C03)

| Chunk                                               | Raw           | Estimated transfer (gzip) |
| --------------------------------------------------- | ------------- | ------------------------- |
| `main-*.js`                                         | 421.87 kB     | 86.75 kB                  |
| `chunk-*.js` (Angular + PrimeNG runtime)            | 184.61 kB     | 54.14 kB                  |
| `styles-*.css` (tokens + base layer + `primeicons`) | 19.55 kB      | 4.02 kB                   |
| **Initial total**                                   | **626.04 kB** | **144.92 kB**             |
| Lazy `shell-component` chunk                        | 213.10 kB     | 40.68 kB                  |
| Lazy `dashboard-component` chunk                    | 9.12 kB       | 2.46 kB                   |

Against [KB-050](../../docs/kb/frontend-new/react-architecture.md)'s **< 250 kB gzip** target:
**144.92 kB, 58 % of budget, comfortably inside it.** Against `angular.json`'s own separate
**raw**-byte budget (warns at 600 kB, errors at 800 kB): a warning at +26.04 kB (+4.3 %) over
the warn threshold, well short of the error threshold — the build still succeeds. Not
investigated further given this task's time budget; a reasonable target for a dedicated
bundle-analysis follow-up, not raised here. `primeicons` (the shell/sidebar/header icon set,
this task's own addition, wired in `styles.scss`) is the whole of the CSS growth; the JS
growth is `shared/components/overlay/` (`Drawer`, `Popover`) and `shared/components/form/`
(`Select`) now being reachable from more of the initial dependency graph than M2-C04-01's
baseline exercised — nothing new was added to `app.component.ts` or `app.config.ts` for this
task.

## Bundle baseline (`npm run build`, 2026-08-23, after M2-C04-01)

| Chunk                                    | Raw           | Estimated transfer (gzip) |
| ---------------------------------------- | ------------- | ------------------------- |
| `chunk-*.js` (Angular + PrimeNG runtime) | 223.11 kB     | 66.75 kB                  |
| `main-*.js`                              | 218.60 kB     | 38.49 kB                  |
| `styles-*.css` (tokens + base layer)     | 4.65 kB       | 1.39 kB                   |
| **Initial total**                        | **446.36 kB** | **106.63 kB**             |
| Lazy `placeholder-component` chunk       | 17.68 kB      | 5.04 kB                   |

M2-C04-01 added **9.51 kB raw / 2.43 kB gzip** in total: **4.65 kB raw / 1.39 kB gzip** of CSS
(the token layer, the `@font-face` block and the global base layer, where `styles-*.css` was 0
bytes before), plus **4.86 kB raw / 1.04 kB gzip** of JavaScript — the token-driven PrimeNG
preset and `ThemeService`. Initial total moved from 436.85 kB / 104.20 kB to
446.36 kB / 106.63 kB, i.e. **43 % of KB-050 budget**.

**Fonts are assets, not part of the initial bundle**, and the browser fetches a subset only when
a glyph in its `unicode-range` is used. `public/fonts/` totals **173.7 kB** on disk —
`inter-latin-wght-normal.woff2` 48,256 B, `inter-latin-ext-wght-normal.woff2` 85,068 B,
`jetbrains-mono-latin-wght-normal.woff2` 40,404 B. A typical Latin-1 page therefore fetches
**88.7 kB** of font (Inter latin + JetBrains Mono latin); the 85 kB latin-ext file is fetched only
when an extended-Latin glyph appears. woff2 is already Brotli-compressed, so gzip does not shrink
it further. Angular's `initial` budget does not count assets, and neither does KB-050's
`< 250 KB gzip` initial-bundle target — but the bytes are real on a cold load, so they are
recorded here rather than left to be discovered.

### Baseline before M2-C04-01 (`npm run build`, 2026-08-21)

| Chunk                                    | Raw           | Estimated transfer (gzip) |
| ---------------------------------------- | ------------- | ------------------------- |
| `chunk-*.js` (Angular + PrimeNG runtime) | 222.23 kB     | 66.56 kB                  |
| `main-*.js`                              | 214.62 kB     | 37.64 kB                  |
| `styles-*.css`                           | 0 bytes       | 0 bytes                   |
| **Initial total**                        | **436.85 kB** | **104.20 kB**             |
| Lazy `placeholder-component` chunk       | 17.68 kB      | 5.05 kB                   |

Against [KB-050](../../docs/kb/frontend-new/react-architecture.md)'s budget of **< 250 KB gzip**
for the initial bundle: **104.20 kB, comfortably inside it.** This is the baseline `M2-C03` is
measured against, and it is a _placeholder_ app — the shell, auth and the first real screens will
move it.

`angular.json`'s production budgets are expressed in **raw** bytes (Angular budgets cannot be set
on transfer size): initial warns at 600 kB and errors at 800 kB, roughly the raw equivalent of the
250 kB gzip target at the ~3× ratio measured above. A per-route-chunk budget is deliberately not
set until a real feature route exists to size it against.

## One component library. Only PrimeNG

ADR-007: _"one library, never mixed"_. Mixing is exactly what makes the current Blazor UI
incoherent — it loads MudBlazor 8.11 **and** Bootstrap 5 (R-22). Enforcement is in two places:

- `package.json` — `primeng` and `@primeuix/themes` are the only UI packages. `@angular/cdk` is
  present because `primeng@22.1.0` peer-requires it; it is a behaviour-primitives package, not a
  component library, and is **not** `@angular/material`.
- `eslint.config.js` — `no-restricted-imports` bans `@angular/material`, `@mui/*`, `antd`,
  `bootstrap`, `primereact`, `@chakra-ui/*`, `react`, `react-dom` and `moment`, and bans importing
  `core/api/generated/**` from anywhere outside `core/api/**`.

> **PrimeNG 22 emits a "PrimeUI license is not configured" console warning and can render an
> unblockable licence banner** (`node_modules/primeng/types/primeng-license.d.ts`;
> `providePrimeNG` accepts a `license` option). No key is configured here. This was not known when
> ADR-007 chose PrimeNG — it is recorded as a risk and an open question for the owner, not
> silently worked around.

## Theming and tokens

Every colour, size, radius, shadow, duration and breakpoint in the app comes from
`src/styles/tokens.css`. **A colour literal may appear in that file and nowhere else** — not in
a component stylesheet, not in a template, not in TypeScript, not in the PrimeNG preset. ESLint
enforces it for `.ts` and `.html`; `src/app/core/theme/no-raw-colour.spec.ts` covers
`.css`/`.scss` too, which `angular.json`'s `lintFilePatterns` cannot reach.

Light and dark are two hand-authored palettes, not one filtered into the other, and
`src/app/core/theme/contrast.spec.ts` computes the WCAG 2.2 ratio of every ink × background
token pair in **both** themes (≥ 4.5:1 text, ≥ 3:1 UI boundaries). If a value fails, change the
value — the threshold is never lowered.

How to add a token, and how PrimeNG is driven from the same layer:
[`src/app/core/theme/README.md`](src/app/core/theme/README.md).

`ThemeService` (root-provided, signals) holds a `light | dark | system` preference defaulting
to `system`, follows `prefers-color-scheme` live, and writes `<html data-theme>` and
`<html data-density>`. A short inline script in `src/index.html` sets `data-theme` **before
first paint** so a dark-mode user never sees a white flash; it cannot live in Angular because it
must run before the bundle is parsed.

Fonts are **self-hosted** (`public/fonts/`, SIL OFL 1.1, licence text alongside) with
`font-display: swap`. There is no `fonts.googleapis.com` or `fonts.gstatic.com` request
anywhere in the build.

> **The SPA's theme preference and Blazor's are independent during the strangler period.**
> Blazor uses `ThemeStateService` plus the `UserThemePreference` entity; the SPA stores its
> preference in `localStorage` under `nexgen.theme`. Switching theme in one does not change
> the other. Server-side persistence needs a settings endpoint **and** a decision on the entity,
> which is a single `bool IsDarkMode` and cannot represent `system` — Q-33, due with M3-3.

## Forms

The form layer lives in `src/app/shared/components/form/` and is exported through
`src/app/shared/components/index.ts`. It is built by **M2-C04-02** against
[KB-051 §Forms](../../docs/kb/frontend-new/design-system.md#forms).

| Control                                     | PrimeNG surface                    | Use it for                                                                                                                                |
| ------------------------------------------- | ---------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `app-form-layout`                           | —                                  | The page-level form. 3 columns at ≥1440, 2 at 1024–1439, 1 at ≤1023. Owns the skeleton, the sticky footer and the form-level error alert. |
| `app-form-section`                          | —                                  | A titled, optionally collapsible group of fields.                                                                                         |
| `app-form-field`                            | —                                  | **The one validation-display mechanism.** Every control renders through it.                                                               |
| `app-text-input`                            | `[pInputText]`                     | Single-line text. Trims on commit.                                                                                                        |
| `app-textarea`                              | `[pTextarea]`                      | Multi-line text.                                                                                                                          |
| `app-number-input`                          | `p-inputnumber`                    | Quantities. Holds a `Qty`, never a `number`.                                                                                              |
| `app-currency-input`                        | `p-inputnumber`                    | Money. Holds a `Money`, never a `number`.                                                                                                 |
| `app-amount-or-percent-input`               | `p-inputnumber` + `p-selectbutton` | The recurring `…AmtOrPer` pair. Captures `{ value, isAmount }` and computes nothing.                                                      |
| `app-select`                                | `p-select`                         | One value from a known list.                                                                                                              |
| `app-multi-select`                          | `p-multiselect`                    | Several values from a known list.                                                                                                         |
| `app-combobox`                              | `p-autocomplete`                   | Search-and-select over a list too large to render — takes a caller-supplied async loader.                                                 |
| `app-date-picker` / `app-date-range-picker` | `p-datepicker`                     | Dates. Typing is always available; the calendar is never the only entry path.                                                             |
| `app-checkbox`                              | `p-checkbox`                       | A boolean **saved on submit**.                                                                                                            |
| `app-radio-group`                           | `p-radiobutton`                    | One of a few mutually exclusive values.                                                                                                   |
| `app-switch`                                | `p-toggleswitch`                   | A boolean with **immediate effect**.                                                                                                      |
| `app-file-upload`                           | `p-fileupload`                     | Collecting `File` objects. It performs no transport.                                                                                      |

**Switch versus checkbox.** `app-switch` is for a toggle that takes effect the moment it is
flipped. A field that is saved when the form is submitted uses `app-checkbox`. Across ~140
screens the two will otherwise be used interchangeably and mean nothing.

**No business logic in a control.** A control captures, formats and _displays_ validation. It
never applies a party cascade, a duplicate-line check, a quantity balance, a tax rule or an
`…AmtOrPer` calculation. The server stays authoritative for validation, calculation,
permissions and document numbering; client validators mirror `DataAnnotations` for UX only.
Cross-field and cross-row rules are **not** expressible as field validators and are extracted
server-side by each wave's `-03` step.

Per-control usage, the keyboard model and the loading/empty/error triad each control owns are in
[`src/app/shared/components/form/README.md`](src/app/shared/components/form/README.md).

## Feedback and overlays

`src/app/shared/components/overlay/` and `src/app/shared/components/feedback/`, built by
**M2-C04-03** against
[KB-051 §Overlays, §Feedback and §State patterns](../../docs/kb/frontend-new/design-system.md#overlays).
Per-component detail: [`overlay/README.md`](src/app/shared/components/overlay/README.md) and
[`feedback/README.md`](src/app/shared/components/feedback/README.md).

| Component                                     | PrimeNG surface                   | Use it for                                               |
| --------------------------------------------- | --------------------------------- | -------------------------------------------------------- |
| `app-modal`                                   | `p-dialog`                        | A question or a short form. Sizes `sm/md/lg/full`.       |
| `app-drawer`                                  | `p-drawer`                        | Record detail **without losing the list behind it**.     |
| `app-confirm-dialog`                          | `p-confirmdialog`                 | Confirmation, optionally with a required reason.         |
| `app-popover`                                 | `p-popover`                       | A small surface anchored to a control.                   |
| `[appTooltip]`                                | `[pTooltip]`                      | A short label. Opens on **focus** as well as hover.      |
| `app-context-menu`                            | `p-contextmenu`                   | Row and record actions, with a visible trigger.          |
| `ToastService`                                | `p-toast`                         | Transient confirmation. The only message-service caller. |
| `app-inline-alert`                            | `p-message`                       | A message next to the thing it is about.                 |
| `app-busy-overlay`                            | `p-blockui` + `p-progressspinner` | A busy region. Full page only by explicit opt-in.        |
| `app-skeleton`, `app-skeleton-table`, `-form` | `p-skeleton`                      | First load. Never a spinner on a blank page.             |
| `app-progress-bar`                            | `p-progressbar`                   | Refetch and determinate progress.                        |
| `app-empty-state`                             | own markup                        | "No data yet" **or** "no results for these filters".     |
| `app-error-state`                             | own markup                        | Server message verbatim + `traceId` + Retry.             |
| `app-permission-denied-state`                 | own markup                        | Which screen right is missing. No retry.                 |

**Modal or drawer.** KB-051 §Do not: "Use modals for anything that needs the list behind it —
use a drawer." If the operator will look back at the list while the overlay is open, it is a
drawer. Everything else that must be answered before work continues is a modal.

**Toast policy.** Success and info clear themselves after 4 s; **an error toast is sticky** and
needs an explicit dismiss. A toast is never the only copy of something the user must act on —
that goes in `app-inline-alert` next to the form, or `app-error-state` for the whole surface.
`feedback/toast.service.ts` is the only file that imports PrimeNG's message service, so a
future change of toast implementation touches one file.

**Empty-state variants.** "No data yet" offers the create action; "no results for these
filters" offers Clear filters. They are different situations and are never interchangeable.

**No ERP business rule lives in either directory.** `app-confirm-dialog` provides the
_capability_ BR-SO-003 needs — collect a mandatory reason — while the rule itself (when a
reason is required, the downstream-transaction checks, the quantity reversion) stays
server-side.

**Bundle cost, measured.** The single `<p-toast>` and `<app-confirm-dialog>` hosts in
`app.component.html` are eager by necessity, and they carry `p-dialog`, `p-confirmdialog`,
`app-form-field` and `app-textarea` with them: initial total moved from 446.36 kB raw /
106.63 kB gzip to **710.39 kB raw / 158.02 kB gzip** (`npm run build`, 2026-08-23). That is
63 % of KB-050's `< 250 KB gzip` target but **over Angular's 600 kB raw warning budget**, so
the build now prints one budget warning while still exiting 0. Importing the two hosts from
their files rather than from the `shared/components` barrel is what keeps it at 710 kB rather
than 1.31 MB — the barrel drags every form control and `decimal.js` into the initial chunk.
Deferring the confirm-dialog host is the obvious next move and is recorded in KB-060.

## Money and quantities

**The server owns every calculated result. The client never does.** Document totals, tax,
discount, freight, TCS and round-off come from `CalculationService.UpdateTotalsAsync`
(BR-CALC-001); stock allocation comes from `StockManagerService` (BR-STK-001). A screen may show
a **provisional** local preview for responsiveness — it must be visually marked as provisional,
and it is **overwritten by the server's result before save**. There is no line-total, tax or
allocation function in this codebase, and adding one is a defect, not a feature.

Every money, quantity, rate, percentage and tax value goes through
[`src/app/shared/utils/decimal/`](src/app/shared/utils/decimal/README.md) (M2-C10) and is
displayed with the `money` pipe in `src/app/shared/pipes/`. JavaScript's `number` is IEEE-754
binary floating point (`0.1 + 0.2 !== 0.3`), the server-side model is C# `decimal`, and a
one-paisa disagreement is an invoice that will not reconcile.

Consequently, **outside that one folder**:

- `decimal.js` may not be imported — use the module's `index.ts`.
- `parseFloat`, `Number.parseFloat` and unary `+` coercion are banned; use `parseUserInput()`.
- `.toFixed()` is banned; use `format()` or the `money` pipe.
- `Math.round`, `Math.floor` and `Math.ceil` are banned; use `round()`.
- Angular's `DecimalPipe` and `CurrencyPipe` must not be used on a money value — both coerce to
  `number`.
- An **absent** amount renders as an em dash, never `0.00`.

`eslint.config.js` enforces all of it, and
`src/app/shared/utils/decimal/no-float-money.spec.ts` scans `src/**` for the same patterns so an
inline `eslint-disable` cannot slip one through. The one exemption,
`src/app/core/theme/contrast.spec.ts`, computes WCAG contrast ratios and carries no ERP value; it
is listed with that reason in both places.

**The wire, updated 2026-08-26 (Q-85, `M2-B13`):** a money-typed property now crosses as a JSON
**string**, not a number — a backend `JsonConverter`, opt-in per property. `fromApi()` already
accepted a string alongside a number (it was written anticipating exactly this change), so no
update to this folder was needed to consume it.

## Structure

`src/` follows [KB-050 §Project structure](../../docs/kb/frontend-new/react-architecture.md#project-structure):
`core/` (singletons), `shared/` (stateless reusables), `layout/`, `features/` (one folder per ERP
module, each lazily routed), plus `environments/`, `styles/`, `i18n/` and `assets/`. Empty
directories carry a `.gitkeep` so the shape is visible from commit one.
