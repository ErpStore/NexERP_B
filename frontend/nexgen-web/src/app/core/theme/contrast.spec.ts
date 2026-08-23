import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { BACKGROUND_TOKENS, BOUNDARY_TOKENS, TEXT_TOKENS } from './tokens';

/**
 * WCAG 2.2 AA, computed - never eyeballed. KB-051 Accessibility commitments:
 * >= 4.5:1 for text and >= 3:1 for UI boundaries, in BOTH themes.
 *
 * The matrix is the full cross-product of the five tokens usable as a
 * background against every ink token, because nothing in a token layer
 * restricts which pairing a future component picks - the worst pair in that
 * matrix is the one the user eventually sees. That is the same scope KB-051
 * "Contrast corrections" measured, so this spec reproduces its ratios.
 *
 * If a pair fails, the failure names the pair and the measured ratio. Fix the
 * value; never lower the threshold.
 */
const TEXT_THRESHOLD = 4.5;
const BOUNDARY_THRESHOLD = 3;

const css = readFileSync(resolve(process.cwd(), 'src/styles/tokens.css'), 'utf8').replace(
  /\/\*[\s\S]*?\*\//g,
  '',
);

function palette(selector: string): Record<string, string> {
  const start = css.indexOf(selector);
  const open = css.indexOf('{', start);
  const close = css.indexOf('}', open);
  const out: Record<string, string> = {};
  for (const match of css.slice(open + 1, close).matchAll(/(--[\w-]+)\s*:\s*(#[0-9a-fA-F]{6});/g)) {
    out[match[1] ?? ''] = match[2] ?? '';
  }
  return out;
}

const lightPalette = palette(':root');
const darkPalette = { ...lightPalette, ...palette("[data-theme='dark']") };

function channel(hex: string, index: number): number {
  const value = parseInt(hex.slice(1 + index * 2, 3 + index * 2), 16) / 255;
  return value <= 0.03928 ? value / 12.92 : Math.pow((value + 0.055) / 1.055, 2.4);
}

function luminance(hex: string): number {
  return 0.2126 * channel(hex, 0) + 0.7152 * channel(hex, 1) + 0.0722 * channel(hex, 2);
}

function ratio(a: string, b: string): number {
  const [x, y] = [luminance(a), luminance(b)];
  return (Math.max(x, y) + 0.05) / (Math.min(x, y) + 0.05);
}

function resolveToken(theme: Record<string, string>, token: string): string {
  const value = theme[token];
  if (!value) {
    throw new Error(`no value for ${token}`);
  }
  return value;
}

function failures(
  theme: Record<string, string>,
  inks: readonly string[],
  threshold: number,
): string[] {
  const out: string[] = [];
  for (const ink of inks) {
    for (const background of BACKGROUND_TOKENS) {
      const measured = ratio(resolveToken(theme, ink), resolveToken(theme, background));
      if (measured < threshold) {
        out.push(`${ink} on ${background}: ${measured.toFixed(2)}:1 (needs ${threshold}:1)`);
      }
    }
  }
  return out;
}

describe('WCAG 2.2 AA contrast, computed from tokens.css', () => {
  it('parses both palettes', () => {
    expect(Object.keys(lightPalette).length).toBeGreaterThanOrEqual(16);
    expect(darkPalette['--bg-canvas']).not.toBe(lightPalette['--bg-canvas']);
  });

  it('light: every text token reaches 4.5:1 on every background token', () => {
    expect(failures(lightPalette, TEXT_TOKENS, TEXT_THRESHOLD)).toEqual([]);
  });

  it('dark: every text token reaches 4.5:1 on every background token', () => {
    expect(failures(darkPalette, TEXT_TOKENS, TEXT_THRESHOLD)).toEqual([]);
  });

  it('light: every boundary token reaches 3:1 on every background token', () => {
    expect(failures(lightPalette, BOUNDARY_TOKENS, BOUNDARY_THRESHOLD)).toEqual([]);
  });

  it('dark: every boundary token reaches 3:1 on every background token', () => {
    expect(failures(darkPalette, BOUNDARY_TOKENS, BOUNDARY_THRESHOLD)).toEqual([]);
  });

  it('reproduces the worst ratios KB-051 Contrast corrections recorded', () => {
    const worst = (theme: Record<string, string>, ink: string): number =>
      Math.min(
        ...BACKGROUND_TOKENS.map((bg) => ratio(resolveToken(theme, ink), resolveToken(theme, bg))),
      );
    const round = (n: number): number => Math.round(n * 100) / 100;

    expect(round(worst(lightPalette, '--border'))).toBe(3.19);
    expect(round(worst(darkPalette, '--border'))).toBe(3.3);
    expect(round(worst(lightPalette, '--border-strong'))).toBe(4.94);
    expect(round(worst(darkPalette, '--border-strong'))).toBe(5.01);
    expect(round(worst(lightPalette, '--text-disabled'))).toBe(4.6);
    expect(round(worst(darkPalette, '--text-disabled'))).toBe(4.57);
    expect(round(worst(lightPalette, '--success'))).toBe(4.61);
    expect(round(worst(lightPalette, '--warning'))).toBe(4.55);
    expect(round(worst(lightPalette, '--focus-ring'))).toBe(4.51);
    expect(round(worst(darkPalette, '--focus-ring'))).toBe(4.76);
    expect(round(worst(lightPalette, '--text-primary'))).toBe(15.58);
    expect(round(worst(darkPalette, '--text-primary'))).toBe(12.88);
  });
});
