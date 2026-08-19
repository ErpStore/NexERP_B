/**
 * The token NAME registry. tokens.css holds the values; this file holds the
 * names and the type that makes them usable from TypeScript.
 *
 * Deliberately value-free for colour: a hex here would be a second source of
 * truth, and the whole point of the layer is that there is exactly one. The
 * drift guard in tokens.test.ts asserts every custom property declared in
 * tokens.css appears here and vice versa, so a token added to one and not the
 * other fails the test run.
 *
 * Non-colour numbers that TypeScript genuinely has to compute with -- the
 * breakpoints and the density heights -- are exported from breakpoints.ts and
 * density.ts, and tokens.test.ts asserts they equal the CSS values.
 */

export const colorTokenNames = [
  '--bg-canvas',
  '--bg-surface',
  '--bg-surface-raised',
  '--bg-subtle',
  '--border',
  '--border-strong',
  '--text-primary',
  '--text-secondary',
  '--text-disabled',
  '--accent',
  '--accent-subtle',
  '--success',
  '--warning',
  '--danger',
  '--info',
  '--focus-ring',
] as const;

/** Tokens legitimately used as a background behind text or a boundary. */
export const backgroundTokenNames = [
  '--bg-canvas',
  '--bg-surface',
  '--bg-surface-raised',
  '--bg-subtle',
  '--accent-subtle',
] as const;

/** Tokens that may be used as a text colour. Must reach 4.5:1 on every background. */
export const textTokenNames = [
  '--text-primary',
  '--text-secondary',
  '--text-disabled',
  '--accent',
  '--success',
  '--warning',
  '--danger',
  '--info',
] as const;

/** Tokens that draw a UI boundary. Must reach 3:1 on every background. */
export const boundaryTokenNames = ['--border', '--border-strong', '--focus-ring'] as const;

export const typographyTokenNames = [
  '--font-sans',
  '--font-mono',
  '--text-xs',
  '--text-sm',
  '--text-base',
  '--text-lg',
  '--text-xl',
  '--text-2xl',
  '--leading-xs',
  '--leading-sm',
  '--leading-base',
  '--leading-lg',
  '--leading-xl',
  '--leading-2xl',
  '--weight-regular',
  '--weight-medium',
  '--weight-semibold',
  '--weight-bold',
] as const;

export const spacingTokenNames = [
  '--space-1',
  '--space-2',
  '--space-3',
  '--space-4',
  '--space-5',
  '--space-6',
  '--space-7',
] as const;

export const radiusTokenNames = [
  '--radius-sm',
  '--radius-md',
  '--radius-lg',
  '--radius-full',
] as const;

export const elevationTokenNames = [
  '--elevation-0',
  '--elevation-1',
  '--elevation-2',
  '--elevation-3',
] as const;

export const motionTokenNames = [
  '--motion-fast',
  '--motion-base',
  '--motion-slow',
  '--motion-ease',
] as const;

export const breakpointTokenNames = ['--bp-xs', '--bp-sm', '--bp-md', '--bp-lg'] as const;

export const densityTokenNames = ['--density-row-height', '--density-control-height'] as const;

export const allTokenNames = [
  ...colorTokenNames,
  ...typographyTokenNames,
  ...spacingTokenNames,
  ...radiusTokenNames,
  ...elevationTokenNames,
  ...motionTokenNames,
  ...breakpointTokenNames,
  ...densityTokenNames,
] as const;

export type TokenName = (typeof allTokenNames)[number];
export type ColorTokenName = (typeof colorTokenNames)[number];

/**
 * The only sanctioned way to reference a token from TypeScript or from a CSS
 * Module written through JS. `token('--accent')` -> `var(--accent)`.
 * Mistyping a name is a compile error, not a silently transparent style.
 */
export function token(name: TokenName): string {
  return `var(${name})`;
}

/** Same, with a fallback for a value that must resolve even outside the app root. */
export function tokenWithFallback(name: TokenName, fallback: string): string {
  return `var(${name}, ${fallback})`;
}
