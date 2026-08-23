/**
 * The token *names*, as types. The token *values* live in
 * `src/styles/tokens.css` and nowhere else.
 *
 * This split is deliberate and is the whole defence against R-22 (a second
 * visual language creeping in): TypeScript may reference a token, never
 * restate one. `tokens.spec.ts` fails if this file and `tokens.css` drift
 * apart in either direction.
 *
 * Specification: docs/kb/frontend-new/design-system.md (KB-051).
 */

/** Tokens usable as a background. Every contrast pair is measured against these. */
export const BACKGROUND_TOKENS = [
  '--bg-canvas',
  '--bg-surface',
  '--bg-surface-raised',
  '--bg-subtle',
  '--accent-subtle',
] as const;

/** Tokens used to paint text. WCAG 2.2 AA threshold: 4.5:1. */
export const TEXT_TOKENS = [
  '--text-primary',
  '--text-secondary',
  '--text-disabled',
  '--accent',
  '--success',
  '--warning',
  '--danger',
  '--info',
] as const;

/** Tokens used to paint a UI boundary. WCAG 2.2 AA threshold: 3:1. */
export const BOUNDARY_TOKENS = ['--border', '--border-strong', '--focus-ring'] as const;

/**
 * The 16 semantic colour tokens of KB-051 Colour. Semantic names only - no
 * component may reference a raw hue, and no token here names one.
 */
export const COLOUR_TOKENS = [...BACKGROUND_TOKENS, ...TEXT_TOKENS, ...BOUNDARY_TOKENS] as const;

export const ELEVATION_TOKENS = [
  '--elevation-0',
  '--elevation-1',
  '--elevation-2',
  '--elevation-3',
] as const;

export const TYPOGRAPHY_TOKENS = [
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

export const SPACING_TOKENS = [
  '--space-4',
  '--space-8',
  '--space-12',
  '--space-16',
  '--space-24',
  '--space-32',
  '--space-48',
] as const;

export const RADIUS_TOKENS = [
  '--radius-sm',
  '--radius-md',
  '--radius-lg',
  '--radius-full',
] as const;

export const MOTION_TOKENS = [
  '--motion-fast',
  '--motion-base',
  '--motion-slow',
  '--motion-ease',
] as const;

export const BREAKPOINT_TOKENS = [
  '--breakpoint-xs',
  '--breakpoint-sm',
  '--breakpoint-md',
  '--breakpoint-lg',
] as const;

export const DENSITY_TOKENS = [
  '--row-height-default',
  '--row-height-compact',
  '--control-height-default',
  '--control-height-compact',
  '--row-height',
  '--control-height',
] as const;

export const SHELL_TOKENS = ['--header-height', '--sidebar-width', '--sidebar-rail-width'] as const;

/** Every token the design system defines. The drift guard's left-hand side. */
export const DESIGN_TOKENS = [
  ...COLOUR_TOKENS,
  ...ELEVATION_TOKENS,
  ...TYPOGRAPHY_TOKENS,
  ...SPACING_TOKENS,
  ...RADIUS_TOKENS,
  ...MOTION_TOKENS,
  ...BREAKPOINT_TOKENS,
  ...DENSITY_TOKENS,
  ...SHELL_TOKENS,
] as const;

export type DesignToken = (typeof DESIGN_TOKENS)[number];
export type BackgroundToken = (typeof BACKGROUND_TOKENS)[number];
export type TextToken = (typeof TEXT_TOKENS)[number];
export type BoundaryToken = (typeof BOUNDARY_TOKENS)[number];

/**
 * A `var(--token)` reference, checked at compile time. This is the only way
 * TypeScript is allowed to name a colour: it produces a reference, never a
 * value, so there is exactly one place a palette can be edited.
 */
export function tokenRef(token: DesignToken): string {
  return `var(${token})`;
}

/**
 * The token's resolved value in the live document. Returns '' during SSR or
 * before styles are applied - callers must tolerate that rather than assume.
 */
export function tokenValue(token: DesignToken, element?: Element): string {
  if (typeof getComputedStyle !== 'function') {
    return '';
  }
  const target = element ?? document.documentElement;
  return getComputedStyle(target).getPropertyValue(token).trim();
}
