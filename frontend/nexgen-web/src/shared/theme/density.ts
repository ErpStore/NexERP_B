/**
 * Density is a functional requirement in this ERP, not a taste: operators work
 * whole shifts over dense structured data, and more visible rows means fewer
 * scrolls (KB-051 §Principles #1 -- 36px default, 30px compact).
 *
 * The same numbers are declared in tokens.css as --density-*; tokens.test.ts
 * asserts they agree.
 */
export const densities = {
  default: { rowHeight: 36, controlHeight: 36 },
  compact: { rowHeight: 30, controlHeight: 30 },
} as const;

export type Density = keyof typeof densities;

export const DENSITY_VALUES = ['default', 'compact'] as const;

export function isDensity(value: unknown): value is Density {
  return typeof value === 'string' && (DENSITY_VALUES as readonly string[]).includes(value);
}
