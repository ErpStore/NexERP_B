/**
 * The four KB-051 responsive bands, as numbers.
 *
 * These are the same numbers as `--breakpoint-*` in `src/styles/tokens.css`;
 * `tokens.spec.ts` asserts that they still are, so a media query and a
 * TypeScript check can never disagree.
 *
 * KB-051 Responsive behaviour:
 *   >= 1440  (primary target)  full sidebar, 3-column forms, all grid columns
 *   1024-1439                  rail sidebar, 2-column forms
 *   768-1023                   overlay drawer, 1-column forms
 *   < 768                      read + approve only; document editing is not
 *                              supported on a phone
 */
export const BREAKPOINTS = {
  xs: 0,
  sm: 768,
  md: 1024,
  lg: 1440,
} as const;

export type BreakpointName = keyof typeof BREAKPOINTS;

/** The bands in ascending order - useful for resolving a width to a band. */
export const BREAKPOINT_ORDER: readonly BreakpointName[] = ['xs', 'sm', 'md', 'lg'];

/** A `min-width` media query for the band, e.g. `(min-width: 1024px)`. */
export function minWidthQuery(name: BreakpointName): string {
  return `(min-width: ${BREAKPOINTS[name]}px)`;
}

/** The band a viewport width falls in. */
export function bandFor(width: number): BreakpointName {
  let band: BreakpointName = 'xs';
  for (const name of BREAKPOINT_ORDER) {
    if (width >= BREAKPOINTS[name]) {
      band = name;
    }
  }
  return band;
}
