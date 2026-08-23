# The theme layer

Everything visual in the SPA resolves to a token defined in
[`src/styles/tokens.css`](../../../styles/tokens.css). This directory holds the TypeScript
half: the token _names_, the PrimeNG preset built over them, and the service that decides
which palette is painted.

Specification: [KB-051](../../../../../docs/kb/frontend-new/design-system.md). Placement rule
(`core/` is for root singletons): [KB-050](../../../../../docs/kb/frontend-new/react-architecture.md).

| File               | What it is                                                                                              |
| ------------------ | ------------------------------------------------------------------------------------------------------- |
| `tokens.ts`        | The token names, as types, plus `tokenRef()` / `tokenValue()`. **No value.**                            |
| `theme.preset.ts`  | The PrimeNG preset, mapped entirely onto `var(--token)`.                                                |
| `theme.service.ts` | Signals: `preference`, `resolvedScheme`, `density`. Writes `<html data-theme>` / `<html data-density>`. |
| `breakpoints.ts`   | The four KB-051 bands, as numbers.                                                                      |
| `density.ts`       | `default` (36 px) / `compact` (30 px).                                                                  |

## The one rule

**A colour literal may appear in `src/styles/tokens.css` and nowhere else.** Not in a
component `.scss`, not in a template, not in TypeScript, not in the PrimeNG preset.

Two things enforce it, because neither can reach the whole tree on its own:

- `eslint.config.js` bans hex, `rgb()` and `hsl()` literals in TypeScript and templates —
  `angular.json`'s `lintFilePatterns` covers only `src/**/*.ts` and `src/**/*.html`;
- `no-raw-colour.spec.ts` scans `.ts`, `.html`, `.css`, `.scss` and `.json` under `src/**`,
  which is how the stylesheets are covered without adding a second linter.

## Adding a token

1. Add it to `:root` in `tokens.css`. If it is a colour, add it to `[data-theme='dark']` too
   — **write the dark value out**; deriving it with `filter`, `invert` or an opacity trick is
   rejected by `tokens.spec.ts`, and KB-051 requires two first-class palettes.
2. Add the name to the matching array in `tokens.ts`. `tokens.spec.ts` fails if the two
   files disagree in either direction; that drift guard is the point of the split.
3. If it is a colour used for text or a UI boundary, add it to `TEXT_TOKENS` or
   `BOUNDARY_TOKENS`. `contrast.spec.ts` will then measure it against every background token
   in both themes and fail below 4.5:1 / 3:1. **If it fails, change the value — never the
   threshold.**
4. If it duplicates a number that also exists in `breakpoints.ts` or `density.ts`, extend the
   drift assertions in `tokens.spec.ts` alongside it.

## How PrimeNG is themed — the answer to Q-67

**Route A: the preset is a pure mapping onto the CSS variables.** No colour is duplicated
into TypeScript.

PrimeNG 22 emits every preset token as a `--p-*` custom property. Its value emitter
(`getVariableValue` in `@primeuix/styled`) rewrites `{a.b}` references into `var(--p-a-b)`
and passes any brace-free string through **verbatim**, so a token whose value is
`var(--accent)` reaches the stylesheet unchanged. Observed, not assumed:

```
toVariables({ primary: { color: 'var(--accent)' } }, { prefix: 'p' }).declarations
  === '--p-primary-color:var(--accent);'
```

That assertion is `tokens.spec.ts` → _"emits var(--token) verbatim into the generated `--p-*`
declarations"_. The documented CSS-layer fallback (overriding `--p-*` in `styles.scss`) was
therefore **not** needed.

**One thing to know about Aura 3.x.** It has no `colorScheme.dark` block: its semantic values
use the CSS `light-dark()` function, which is selected by the `color-scheme` property rather
than by a selector. That would normally fight a `[data-theme]` token layer. It does not here,
because `tokens.css` sets `color-scheme: light` on `:root` and `color-scheme: dark` under
`[data-theme='dark']`, and `app.config.ts` sets `darkModeSelector` to the same attribute — so
an Aura token we have _not_ overridden still flips at exactly the moment one we _have_ does.
Both mechanisms hang off the single signal `ThemeService` writes.

**Do not** feed a `var(--…)` to `updatePrimaryPalette()`, `palette()`, `shade()` or `tint()`:
those helpers require a hex literal and silently return their input otherwise. Override the
semantic leaves instead, as `theme.preset.ts` does.

## Persistence, and what is deliberately missing

The preference is stored in `localStorage` under `nexgen.theme` (density: `nexgen.density`),
and the pre-paint script in `src/index.html` reads the same keys before the bundle is parsed
so a dark-mode user never sees a white flash.

There is **no server persistence**, and that is not an oversight:

- `IUserThemePreferenceService` has no HTTP surface at all (INV-006, negative result);
- the entity that exists cannot hold the answer. `UserThemePreference` is a single
  `bool IsDarkMode`
  (`V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/UserThemePreference.cs:20`,
  re-confirmed 2026-08-23) with **no representation for `system`**, which is the default
  preference.

Whether it becomes tri-state is **Q-33**, owned by product + backend and needed by **M3-3**.
Do not extend the entity from the frontend.

During the strangler period the SPA's theme and Blazor's theme are **independent**: switching
here does not change what a Blazor page renders, and vice versa.
