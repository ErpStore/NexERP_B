# nexgen-web — V.SMART / NexGen ERP React frontend

The React 19 + TypeScript SPA that replaces the Blazor Server UI, screen by screen. Created
by task **M2-C01**; at that point it is a **skeleton**: one placeholder route, no ERP screen,
no API call, no authentication.

The Blazor app (`V.SMART/V.SMART.Web`) stays live and authoritative until each screen here is
verified against it.

## Prerequisites

- Node **22 LTS** (`.nvmrc`). See _Node version_ below — `engines` is `>=22`, deliberately
  open-ended, and this tree was built and verified on Node 24.19.0.
- npm 10+ (`npm ci`, never `npm install`, in CI).

## Commands

| Command                                   | What it does                                                                            |
| ----------------------------------------- | --------------------------------------------------------------------------------------- |
| `npm ci`                                  | Install exactly what `package-lock.json` pins.                                          |
| `npm run dev`                             | Vite dev server on <http://localhost:5173>.                                             |
| `npm run typecheck`                       | `tsc --noEmit` over `src/`+`e2e/` and over the root config files.                       |
| `npm run lint`                            | ESLint flat config, `--max-warnings=0`.                                                 |
| `npm run format:check` / `npm run format` | Prettier.                                                                               |
| `npm run test -- --run`                   | Vitest + React Testing Library + MSW, single pass.                                      |
| `npm run coverage`                        | Vitest with v8 coverage and thresholds.                                                 |
| `npm run build`                           | Typecheck, then production Vite build into `dist/`.                                     |
| `npm run e2e`                             | Playwright; starts the dev server itself. Needs `npx playwright install chromium` once. |

## Configuration

Copy `.env.example` to `.env` and set:

```
VITE_API_BASE_URL=
```

**It has no default, on purpose.** A missing value must fail loudly. The Angular pilot
hardcodes `http://localhost:5144` in `frontend/vsmart-erp/src/environments/environment.ts:4`
_and_ in `environment.prod.ts:3` — its production build points at localhost. Do not reproduce
that. `.env` is git-ignored; `.env.example` is not.

## One component library only — Mantine 7

**ADR-003** (`docs/kb/decisions/ADR-003-react-stack.md`) selects Mantine and states the two
are never mixed. The live Blazor UI loads MudBlazor 8 _and_ Bootstrap 5 simultaneously; that
is recorded as risk **R-22** and is the incoherence this rule exists to prevent repeating.

`eslint.config.js` enforces it mechanically: importing from `@mui/*`, `antd`, `bootstrap`,
`react-bootstrap`, `primereact`, `@chakra-ui/*`, `@radix-ui/*` or `moment` is a lint error.
If you believe a second library is genuinely needed, stop and raise it — do not install it.

A second `no-restricted-imports` rule confines `src/shared/api/generated/**` (the OpenAPI
client, which arrives in **M2-B10**) to imports from `src/shared/api/**`, so that restriction
never has to be retrofitted.

## Where things go

`src/` follows the feature-sliced structure in KB-050 §Project structure:

```
src/
  app/        App.tsx, providers.tsx, router.tsx
  shared/     api/ auth/ components/ hooks/ lib/ types/ i18n/
  features/   one folder per ERP module (empty until M2-D)
  layouts/    AppShell / AuthLayout / PrintLayout (M2-C03)
  test/       setup.ts + MSW harness
e2e/          Playwright specs
```

Empty directories carry a `.gitkeep`. Features never import from each other — shared things
move to `shared/`.

## Bundle baseline (M2-C01)

Measured with `npm run build` on 2026-08-19, Vite 6.4.3, production mode:

| Asset                                                           | Raw       | Gzip         |
| --------------------------------------------------------------- | --------- | ------------ |
| `assets/index-*.js` (**entry chunk**)                           | 289.69 kB | **90.90 kB** |
| `assets/react-*.js` (react/react-dom/react-router vendor chunk) | 102.50 kB | 34.48 kB     |
| `assets/index-*.css` (Mantine base styles)                      | 201.38 kB | 29.30 kB     |
| `index.html`                                                    | 0.47 kB   | 0.29 kB      |

Initial JavaScript, gzipped: **125.38 kB** against KB-050's `< 250 KB gzip` target. This is
the number the app shell (M2-C03) and the first feature screens are measured against — it is
a baseline, not a pass mark.

Coverage baseline: statements 82.89 %, branches 100 %, functions 80 %, lines 82.89 %.
`vitest.config.ts` sets thresholds to the floor of those figures so the number can only rise.

## What is deliberately NOT here

Design tokens and theme (M2-C04-01) · authentication, Axios interceptors, route guards and
the permission store (M2-C02) · the app shell, sidebar and breadcrumbs (M2-C03) · the
generated API client (M2-B10) · money/stock guard rails (M2-C10) · any ERP business logic,
ever — the server is authoritative for validation, calculation, permissions and document
numbering.
