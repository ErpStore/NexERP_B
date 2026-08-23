/**
 * Display density. KB-051 Principles puts it first: "Default row height 36 px
 * (compact 30 px). More rows visible = fewer scrolls = faster work."
 *
 * The heights below mirror `--row-height-*` / `--control-height-*` in
 * `src/styles/tokens.css`; `tokens.spec.ts` asserts they still match.
 * `ThemeService` writes the choice to `<html data-density>`, which is what
 * actually swaps the CSS variables - nothing reads these numbers to lay out.
 */
export const DENSITIES = ['default', 'compact'] as const;

export type Density = (typeof DENSITIES)[number];

export const DEFAULT_DENSITY: Density = 'default';

/** Row and control heights in px, for the rare consumer that needs the number. */
export const DENSITY_HEIGHTS: Record<Density, { row: number; control: number }> = {
  default: { row: 36, control: 36 },
  compact: { row: 30, control: 30 },
};

export function isDensity(value: unknown): value is Density {
  return typeof value === 'string' && (DENSITIES as readonly string[]).includes(value);
}
