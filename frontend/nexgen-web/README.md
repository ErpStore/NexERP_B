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

## Theming and tokens (M2-C04-01)

Everything visual comes from `src/shared/theme/`. Its own
[README](src/shared/theme/README.md) is the catalogue and the how-to; the short version:

- `src/shared/theme/tokens.css` holds the two palettes — `:root` light,
  `[data-theme='dark']` dark, authored independently, never derived from one another. It is
  the **only** file in `src/**` allowed to contain a colour literal.
- Everything else uses `var(--token)`, or `token('--accent')` from `src/shared/theme/tokens`.
  `npm run lint` fails on a hex, `rgb()` or `hsl()` literal in TS/TSX, and
  `no-raw-colour.test.ts` fails on one in CSS. This is what stops a second visual language
  appearing (risk **R-22**).
- Adding a token means adding it to `tokens.css` **and** `tokens.ts`; `tokens.test.ts` fails
  if you do one without the other.
- `contrast.test.ts` computes WCAG 2.2 ratios for every ink × background pair in both themes.
  Eight KB-051 values failed its own ≥ 4.5:1 / ≥ 3:1 commitment and were corrected; the
  thresholds were not moved. See KB-051 §Colour for the measured table.
- The preference is `light | dark | system`, defaults to `system`, follows the OS live, and
  is written to `<html data-theme>` **before first paint** by a short inline script in
  `index.html` so a dark-mode user never sees a white frame.

**React and Blazor theme preferences are independent during the strangler period.** This app
persists the preference in `localStorage` (`nexgen.theme`); Blazor keeps using
`ThemeStateService` + the `UserThemePreference` row. Switching theme in one does not change
the other. Server persistence waits on a settings endpoint and on a decision about the
`UserThemePreference.IsDarkMode` boolean, which cannot represent `system` — **Q-33**, needed
by **M3-3**.

### Byte cost of the theme layer

Measured with `npm run build` on 2026-08-19, same toolchain as the baseline above:

| Asset                                    | Raw       | Gzip                              | Delta vs M2-C01 baseline                  |
| ---------------------------------------- | --------- | --------------------------------- | ----------------------------------------- |
| `assets/index-*.js` (entry chunk)        | 292.59 kB | **91.59 kB**                      | +0.69 kB gzip                             |
| `assets/react-*.js` (vendor)             | 102.50 kB | 34.48 kB                          | unchanged                                 |
| `assets/index-*.css`                     | 205.11 kB | 30.49 kB                          | +1.19 kB gzip                             |
| `index.html`                             | 1.39 kB   | 0.69 kB                           | +0.40 kB gzip                             |
| `fonts/*.woff2` (6 files, latin subsets) | 139.74 kB | n/a (woff2 is already compressed) | +139.74 kB, **not** in the initial bundle |

Initial JavaScript, gzipped: **126.07 kB** against KB-050's `< 250 KB gzip` target (was
125.38 kB). The fonts are fetched by `@font-face` with `font-display: swap`, so they are off
the critical path and outside that figure; they are 4 Inter weights (400/500/600/700) and
2 JetBrains Mono weights (400/500), latin subsets only, self-hosted from `public/fonts/`.
**No external font request is made** — no Google Fonts host appears anywhere in the source
or in `dist/`, which is what the two `git grep` checks in the task verification list assert.

## What is deliberately NOT here

Authentication, Axios interceptors, route guards and
the permission store (M2-C02) · the app shell, sidebar and breadcrumbs (M2-C03) · the
generated API client (M2-B10) · money/stock guard rails (M2-C10) · any ERP business logic,
ever — the server is authoritative for validation, calculation, permissions and document
numbering.
