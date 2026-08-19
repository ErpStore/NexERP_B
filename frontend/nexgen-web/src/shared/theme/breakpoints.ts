/**
 * KB-051 §Responsive behaviour, as ONE set of numbers.
 *
 * The same values are declared in tokens.css as --bp-*; tokens.test.ts asserts
 * the two agree, so a media query and a JS width check can never disagree.
 *
 * >=1440  primary target: full sidebar, 3-column forms, all grid columns
 * 1024-1439  rail sidebar, 2-column forms
 * 768-1023   overlay drawer, 1-column forms, low-priority columns dropped
 * <768       read + approve only; document editing is not supported on phone
 */
export const breakpoints = {
  xs: 0,
  sm: 768,
  md: 1024,
  lg: 1440,
} as const;

export type BreakpointName = keyof typeof breakpoints;

/** `min-width` media query for a breakpoint, for use in JS-driven matchMedia. */
export function minWidth(name: BreakpointName): string {
  return `(min-width: ${breakpoints[name]}px)`;
}
