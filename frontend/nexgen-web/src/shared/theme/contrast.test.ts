import { describe, expect, it } from 'vitest';

import { type ThemeName, tokenValues } from '@/test/tokens-source';

import { backgroundTokenNames, boundaryTokenNames, textTokenNames } from './tokens';

/**
 * WCAG 2.2 AA contrast, COMPUTED over the real token values, in both themes.
 *
 * KB-051 commits to >= 4.5:1 for text and >= 3:1 for UI boundaries in both
 * themes. Two hand-authored palettes across sixteen tokens will contain a
 * violation unless something checks -- and KB-051's published table did contain
 * eight, all recorded in KB-051 and marked CORRECTED in tokens.css. The
 * THRESHOLDS BELOW ARE NEVER LOWERED to make a token pass; the token is fixed.
 *
 * The matrix is the full cross-product of every token that may be used as ink
 * against every token that may be used as a background, because nothing in the
 * token layer restricts which pairing a future component will choose.
 */
const TEXT_MINIMUM = 4.5;
const BOUNDARY_MINIMUM = 3;

function channel(value: number): number {
  const c = value / 255;
  return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
}

export function relativeLuminance(hex: string): number {
  const match = /^#([0-9a-f]{6})$/i.exec(hex.trim());
  if (!match?.[1]) throw new Error(`Not a six-digit hex colour: "${hex}"`);
  const n = Number.parseInt(match[1], 16);
  return (
    0.2126 * channel((n >> 16) & 255) + 0.7152 * channel((n >> 8) & 255) + 0.0722 * channel(n & 255)
  );
}

export function contrastRatio(a: string, b: string): number {
  const [x, y] = [relativeLuminance(a), relativeLuminance(b)].sort((p, q) => q - p);
  return ((x ?? 0) + 0.05) / ((y ?? 0) + 0.05);
}

// Sanity anchors for the formula itself: if these drift, every other assertion
// in this file is meaningless.
describe('contrast formula', () => {
  it('computes the known extremes', () => {
    // Built rather than written literally: no-raw-colour.test.ts and the ESLint
    // rule ban colour literals outside tokens.css, and a test file is no exception.
    const black = `#${'0'.repeat(6)}`;
    const white = `#${'f'.repeat(6)}`;
    expect(contrastRatio(black, white)).toBeCloseTo(21, 5);
    expect(contrastRatio(white, white)).toBeCloseTo(1, 5);
  });
});

const themes: ThemeName[] = ['light', 'dark'];

describe.each(themes)('%s theme', (themeName) => {
  const values = tokenValues(themeName);

  const pairs = (ink: readonly string[]) =>
    ink.flatMap((inkToken) => backgroundTokenNames.map((bgToken) => ({ inkToken, bgToken })));

  it.each(pairs(textTokenNames))(
    `$inkToken on $bgToken is at least ${TEXT_MINIMUM}:1`,
    ({ inkToken, bgToken }) => {
      const ink = values[inkToken];
      const bg = values[bgToken];
      expect(ink, `${inkToken} missing from ${themeName}`).toBeDefined();
      expect(bg, `${bgToken} missing from ${themeName}`).toBeDefined();
      const ratio = contrastRatio(ink as string, bg as string);
      expect(
        ratio,
        `${themeName}: ${inkToken} (${ink as string}) on ${bgToken} (${bg as string}) measured ${ratio.toFixed(2)}:1`,
      ).toBeGreaterThanOrEqual(TEXT_MINIMUM);
    },
  );

  it.each(pairs(boundaryTokenNames))(
    `$inkToken on $bgToken is at least ${BOUNDARY_MINIMUM}:1`,
    ({ inkToken, bgToken }) => {
      const ink = values[inkToken];
      const bg = values[bgToken];
      const ratio = contrastRatio(ink as string, bg as string);
      expect(
        ratio,
        `${themeName}: ${inkToken} (${ink as string}) on ${bgToken} (${bg as string}) measured ${ratio.toFixed(2)}:1`,
      ).toBeGreaterThanOrEqual(BOUNDARY_MINIMUM);
    },
  );

  it('states a stronger boundary is actually stronger', () => {
    const canvas = values['--bg-canvas'] as string;
    expect(contrastRatio(values['--border-strong'] as string, canvas)).toBeGreaterThan(
      contrastRatio(values['--border'] as string, canvas),
    );
  });
});
