# nexgen-web — the NexGen ERP SPA

Angular + PrimeNG frontend for V.SMART, created by task **M2-C01** under
[ADR-007](../../docs/kb/decisions/ADR-007-angular-stack.md). It replaces the React scaffold that
previously occupied this directory; ADR-007 discarded that stack on 2026-08-20.

Today it renders **one placeholder route**. The app shell is `M2-C03`, authentication and
permissions are `M2-C02`, design tokens are `M2-C04-01`, and the generated OpenAPI client is
`M2-B10`. Nothing here computes anything: the server stays authoritative for validation,
calculations, permissions and document numbering.

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
installed alongside it and used in `src/app/features/placeholder/placeholder.component.spec.ts`;
`src/app/app.component.spec.ts` uses `TestBed` directly because it asserts on the composed
provider set rather than on rendered output.

## API base URL — configuration, not source

**No file under `src/` contains an API host, and there is no default.** The Angular pilot at
`frontend/vsmart-erp/` hardcodes a `http://localhost` dev origin in _both_ `environment.ts` and
`environment.prod.ts`, so its production build points at a developer's laptop. ADR-007 calls that
_"a defect to remove, not a pattern to keep"_ (`ADR-007-angular-stack.md:182-184`).

The mechanism here:

1. `src/environments/environment.ts` / `environment.prod.ts` carry only `production` and the
   **path** of the runtime configuration document. No host, no scheme, no port.
2. `provideAppInitializer(loadAppConfig)` fetches `config/app-config.json` **before the app
   renders** (`src/app/core/config/app-config.ts`).
3. If that document cannot be fetched, or its `apiBaseUrl` is missing or blank, the initializer
   **throws and bootstrap fails loudly**. It never falls back to a host.
4. `public/config/app-config.json` ships `"apiBaseUrl": "/api"` — a _same-origin relative path_,
   not a host. Deployments that serve the API from another origin overwrite this one file; nothing
   is rebuilt. `public/config/app-config.example.json` documents the shape.

## Bundle baseline (`npm run build`, 2026-08-21)

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

## Structure

`src/` follows [KB-050 §Project structure](../../docs/kb/frontend-new/react-architecture.md#project-structure):
`core/` (singletons), `shared/` (stateless reusables), `layout/`, `features/` (one folder per ERP
module, each lazily routed), plus `environments/`, `styles/`, `i18n/` and `assets/`. Empty
directories carry a `.gitkeep` so the shape is visible from commit one.
