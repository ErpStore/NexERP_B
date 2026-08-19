import { describe, expect, it } from 'vitest';

import { blockFor, declaredTokenNames, tokenValues } from '@/test/tokens-source';

import { breakpoints } from './breakpoints';
import { densities } from './density';
import { allTokenNames, colorTokenNames, token } from './tokens';

/**
 * The drift guard. tokens.css holds the values, tokens.ts holds the names; a
 * token added to one and not the other is a silent hole -- a component would
 * either reference a variable that does not exist (transparent) or a variable no
 * type knows about. Neither fails loudly on its own, so this test does.
 */
describe('tokens.css <-> tokens.ts', () => {
  const declared = declaredTokenNames();

  it('declares every name tokens.ts knows about', () => {
    expect([...allTokenNames].filter((name) => !declared.includes(name))).toEqual([]);
  });

  it('knows about every name tokens.css declares', () => {
    expect(declared.filter((name) => !(allTokenNames as readonly string[]).includes(name))).toEqual(
      [],
    );
  });

  it('gives all 16 semantic colour tokens a value in BOTH themes', () => {
    expect(colorTokenNames).toHaveLength(16);
    for (const theme of ['light', 'dark'] as const) {
      const values = tokenValues(theme);
      for (const name of colorTokenNames) {
        expect(values[name], `${name} in ${theme}`).toMatch(/^#[0-9a-f]{6}$/i);
      }
    }
  });

  it('authors dark as an independent palette, not a filter over light', () => {
    const dark = blockFor("[data-theme='dark']");
    expect(dark).not.toMatch(/filter\s*:/);
    expect(dark).not.toMatch(/invert\s*\(/);
    expect(dark).not.toMatch(/opacity\s*:/);
    // Every colour is stated outright.
    for (const name of colorTokenNames) {
      expect(dark, `${name} must be restated in the dark palette`).toContain(`${name}:`);
    }
  });

  it('emits var() references, not values, from token()', () => {
    expect(token('--accent')).toBe('var(--accent)');
  });
});

/**
 * The numbers TypeScript computes with must equal the numbers CSS lays out with.
 * A media query and a JS width check that disagree is a bug nobody sees until a
 * customer resizes a window.
 */
describe('CSS and TypeScript agree on the numbers', () => {
  const light = tokenValues('light');

  it.each(Object.entries(breakpoints))('breakpoint %s', (name, px) => {
    expect(light[`--bp-${name}`]).toBe(`${px}px`);
  });

  it('default density', () => {
    expect(light['--density-row-height']).toBe(`${densities.default.rowHeight}px`);
    expect(light['--density-control-height']).toBe(`${densities.default.controlHeight}px`);
  });

  it('compact density', () => {
    const compact = blockFor("[data-density='compact']");
    expect(compact).toContain(`--density-row-height: ${densities.compact.rowHeight}px`);
    expect(compact).toContain(`--density-control-height: ${densities.compact.controlHeight}px`);
  });
});
