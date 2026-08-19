import { createTheme, type MantineColorsTuple } from '@mantine/core';

import { breakpoints } from './breakpoints';
import { token } from './tokens';

/**
 * The Mantine 7 theme, expressed ENTIRELY over the CSS variables in tokens.css.
 *
 * No hex value appears here on purpose. ADR-003 chose Mantine over a runtime
 * style engine precisely because its theming compiles to CSS variables: pointing
 * Mantine's scales at ours means a theme switch changes variable VALUES only --
 * no re-render, no remount, no layout shift -- and it keeps tokens.css the single
 * source of colour.
 */

// Mantine requires ten shades. Ours is a semantic token, not a hue ramp, so every
// index resolves to the same variable; components must not depend on a shade
// index carrying meaning.
const accent: MantineColorsTuple = Array.from({ length: 10 }, () =>
  token('--accent'),
) as unknown as MantineColorsTuple;

export const theme = createTheme({
  primaryColor: 'accent',
  colors: { accent },
  white: token('--bg-surface'),
  black: token('--text-primary'),

  fontFamily: token('--font-sans'),
  fontFamilyMonospace: token('--font-mono'),
  defaultRadius: 'md',

  fontSizes: {
    xs: token('--text-xs'),
    sm: token('--text-sm'),
    md: token('--text-base'),
    lg: token('--text-lg'),
    xl: token('--text-xl'),
  },
  lineHeights: {
    xs: token('--leading-xs'),
    sm: token('--leading-sm'),
    md: token('--leading-base'),
    lg: token('--leading-lg'),
    xl: token('--leading-xl'),
  },
  headings: {
    fontFamily: token('--font-sans'),
    sizes: {
      h1: { fontSize: token('--text-2xl'), lineHeight: token('--leading-2xl') },
      h2: { fontSize: token('--text-xl'), lineHeight: token('--leading-xl') },
      h3: { fontSize: token('--text-lg'), lineHeight: token('--leading-lg') },
      h4: { fontSize: token('--text-base'), lineHeight: token('--leading-base') },
      h5: { fontSize: token('--text-sm'), lineHeight: token('--leading-sm') },
      h6: { fontSize: token('--text-xs'), lineHeight: token('--leading-xs') },
    },
  },
  spacing: {
    xs: token('--space-1'),
    sm: token('--space-2'),
    md: token('--space-4'),
    lg: token('--space-5'),
    xl: token('--space-6'),
  },
  radius: {
    xs: token('--radius-sm'),
    sm: token('--radius-sm'),
    md: token('--radius-md'),
    lg: token('--radius-lg'),
    xl: token('--radius-lg'),
  },
  shadows: {
    xs: token('--elevation-1'),
    sm: token('--elevation-1'),
    md: token('--elevation-2'),
    lg: token('--elevation-3'),
    xl: token('--elevation-3'),
  },
  // Same numbers as breakpoints.ts, which tokens.test.ts ties to --bp-* in CSS.
  breakpoints: {
    xs: `${breakpoints.xs}px`,
    sm: `${breakpoints.sm}px`,
    md: `${breakpoints.md}px`,
    lg: `${breakpoints.lg}px`,
    xl: `${breakpoints.lg}px`,
  },
});
