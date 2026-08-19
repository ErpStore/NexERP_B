# Design tokens and theme (M2-C04-01)

The token layer implementing [KB-051](../../../../../docs/kb/frontend-new/design-system.md).
Every colour, size, space, radius, shadow, duration, breakpoint and density in the app comes
from here. Nothing else may name a colour.

## Files

| File                           | Role                                                                                                                                              |
| ------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `tokens.css`                   | The values. `:root` is the light palette, `[data-theme='dark']` the dark one. **The only file in `src/**` allowed to contain a colour literal.**  |
| `tokens.ts`                    | The names, as types, plus `token('--accent') -> 'var(--accent)'`. Value-free for colour on purpose: a hex here would be a second source of truth. |
| `theme.ts`                     | The Mantine 7 theme, expressed entirely over `var(--…)`. No hex.                                                                                  |
| `ThemeProvider.tsx`            | Owns the resolved scheme and density; writes `data-theme` / `data-density` on `<html>`; wraps `MantineProvider`. Exports `useTheme()`.            |
| `useColorScheme.ts`            | The `light \| dark \| system` preference, live `matchMedia` subscription, local persistence.                                                      |
| `ThemeToggle.tsx`              | The three-state control. **Exported, not placed** — M2-C03 puts it in the header.                                                                 |
| `breakpoints.ts`, `density.ts` | The same numbers as CSS, for code that has to compute with them. `tokens.test.ts` asserts they agree.                                             |
| `contrast.test.ts`             | Computes WCAG 2.2 ratios over every ink × background pair, both themes.                                                                           |
| `tokens.test.ts`               | Drift guard: `tokens.css` and `tokens.ts` must name exactly the same tokens.                                                                      |
| `no-raw-colour.test.ts`        | Scans `src/**` for colour literals outside `tokens.css`.                                                                                          |

## How to add a token

1. Add it to `tokens.css` — **both** palettes if it is a colour. Never derive dark from light.
2. Add its name to the matching group in `tokens.ts`, which puts it in `allTokenNames`.
3. If it is ink or a boundary, add it to `textTokenNames` or `boundaryTokenNames` so
   `contrast.test.ts` measures it. A colour that no test measures is a colour that will
   eventually fail a user.
4. Run `npm run test -- --run`. The drift guard fails if you did step 1 without step 2.

## Rules for using one

- **Semantic names only.** Use `--danger`, never a red. A component that knows what red means
  is a component that has to be edited when the meaning changes.
- **Never a raw colour.** `npm run lint` rejects a hex, `rgb()` or `hsl()` literal in TS/TSX;
  `no-raw-colour.test.ts` rejects one in CSS too. `tokens.css` is the single exception.
- **Never colour alone.** Status is icon + text + colour (KB-051 §Status vocabulary); the
  badge that applies it is M2-C05-01's, not this layer's.
- `--text-sm` (12/18) is the workhorse for table body and form inputs, not `--text-base`.
  This is a density decision, not a taste one.
- Anything numeric gets `font-variant-numeric: tabular-nums` and right alignment — the
  `.numeric` / `[data-numeric]` classes in `src/styles/global.css` do it for you.

## Contrast: eight KB-051 values were corrected

KB-051 commits to ≥ 4.5:1 for text and ≥ 3:1 for UI boundaries **in both themes**. Its
published palette does not meet that commitment across the full ink × background matrix.
The threshold was **not** lowered; the eight failing values were corrected and each is
marked `CORRECTED` in `tokens.css` with the ratio it failed at. The measured table lives in
KB-051 §Colour. The most visible consequence is that `--border` is materially darker than
KB-051 drew it: a hairline at 1.2:1 is decoration, not a boundary.

## Theme persistence is local, and that is temporary

The preference is stored in `localStorage` under `nexgen.theme` (`nexgen.density` for
density). It is **not** sent to the server, because there is nowhere to send it:
`IUserThemePreferenceService` has no HTTP surface, and the entity behind it,
`UserThemePreference`, is a single `bool IsDarkMode`
(`V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/UserThemePreference.cs:20`)
which cannot represent `system`. Whether that entity becomes tri-state or `system` resolves
client-side before persistence is **Q-33** in `docs/kb/open-questions.md`, owned by product +
backend and needed by **M3-3**.

Until then, **the React and Blazor theme preferences are independent.** A user who switches
to dark here still sees Blazor in whatever `ThemeStateService` /`UserThemePreference` last
gave them. That is expected during the strangler period, not a bug.

## Why the inline script in `index.html`

React mounts _after_ the first paint. Resolving the theme in React means a dark-mode user
sees one white frame on every load. The five-line script in `index.html` sets
`data-theme` synchronously before anything renders. Keep its storage keys in step with
`useColorScheme.ts` and `ThemeProvider.tsx`.
