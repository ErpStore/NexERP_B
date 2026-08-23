import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { toVariables } from '@primeuix/styled';

import { BREAKPOINTS } from './breakpoints';
import { DENSITY_HEIGHTS } from './density';
import { NexGenPresetExtension, NexGenThemeOptions } from './theme.preset';
import { COLOUR_TOKENS, DESIGN_TOKENS } from './tokens';

const TOKENS_CSS_PATH = resolve(process.cwd(), 'src/styles/tokens.css');
const css = readFileSync(TOKENS_CSS_PATH, 'utf8');

/** Strip comments, then read every `--name: value;` declaration in a block. */
function declarationsOf(selector: string): Map<string, string> {
  const source = css.replace(/\/\*[\s\S]*?\*\//g, '');
  const start = source.indexOf(selector);
  if (start < 0) {
    throw new Error(`tokens.css has no ${selector} block`);
  }
  const open = source.indexOf('{', start);
  const close = source.indexOf('}', open);
  const block = source.slice(open + 1, close);
  const found = new Map<string, string>();
  for (const match of block.matchAll(/(--[\w-]+)\s*:\s*([^;]+);/g)) {
    found.set(match[1] ?? '', (match[2] ?? '').trim());
  }
  return found;
}

const light = declarationsOf(':root');
const dark = declarationsOf("[data-theme='dark']");
const compact = declarationsOf("[data-density='compact']");

const allDeclared = new Set<string>([...light.keys(), ...dark.keys(), ...compact.keys()]);

describe('tokens.css <-> tokens.ts drift guard', () => {
  it('declares exactly the tokens tokens.ts names', () => {
    const named = new Set<string>(DESIGN_TOKENS);
    const missingFromCss = [...named].filter((t) => !allDeclared.has(t)).sort();
    const missingFromTs = [...allDeclared].filter((t) => !named.has(t)).sort();
    expect({ missingFromCss, missingFromTs }).toEqual({ missingFromCss: [], missingFromTs: [] });
  });

  it('gives every colour token a value in both palettes', () => {
    const missingLight = COLOUR_TOKENS.filter((t) => !light.has(t));
    const missingDark = COLOUR_TOKENS.filter((t) => !dark.has(t));
    expect({ missingLight, missingDark }).toEqual({ missingLight: [], missingDark: [] });
  });

  it('authors dark as an independent palette, not a filter over light', () => {
    // KB-051: "Both themes are authored as first-class palettes, not a filter
    // over one another."
    expect(css).not.toMatch(/filter\s*:/);
    expect(css).not.toMatch(/invert\(/);
    for (const token of COLOUR_TOKENS) {
      expect(dark.get(token)).not.toBe(light.get(token));
      expect(dark.get(token)).not.toMatch(/var\(/);
    }
  });

  it('sets color-scheme per palette so CSS light-dark() follows data-theme', () => {
    expect(light.size).toBeGreaterThan(0);
    expect(css).toMatch(/:root\s*\{[\s\S]*?color-scheme:\s*light;/);
    expect(css).toMatch(/\[data-theme='dark'\]\s*\{[\s\S]*?color-scheme:\s*dark;/);
  });

  it('keeps breakpoints.ts and the breakpoint tokens in step', () => {
    for (const [name, px] of Object.entries(BREAKPOINTS)) {
      expect(light.get(`--breakpoint-${name}`)).toBe(`${px}px`);
    }
  });

  it('keeps density.ts and the density tokens in step', () => {
    expect(light.get('--row-height-default')).toBe(`${DENSITY_HEIGHTS.default.row}px`);
    expect(light.get('--control-height-default')).toBe(`${DENSITY_HEIGHTS.default.control}px`);
    expect(light.get('--row-height-compact')).toBe(`${DENSITY_HEIGHTS.compact.row}px`);
    expect(light.get('--control-height-compact')).toBe(`${DENSITY_HEIGHTS.compact.control}px`);
    expect(compact.get('--row-height')).toBe('var(--row-height-compact)');
  });
});

/** Every string leaf of the preset, with its dotted path. */
function leaves(value: unknown, path: string[] = []): [string, string][] {
  if (typeof value === 'string') {
    return [[path.join('.'), value]];
  }
  if (value && typeof value === 'object') {
    return Object.entries(value).flatMap(([key, child]) => leaves(child, [...path, key]));
  }
  return [];
}

describe('the PrimeNG preset is a pure mapping onto the token layer (Q-67)', () => {
  const presetLeaves = leaves(NexGenPresetExtension);

  it('restates no colour value in TypeScript', () => {
    const literal = /#[0-9a-fA-F]{3,8}\b|\b(rgb|hsl)a?\(/;
    const offenders = presetLeaves.filter(([, v]) => literal.test(v));
    expect(offenders).toEqual([]);
  });

  it('references only tokens that tokens.css defines', () => {
    const referenced = new Set<string>();
    for (const [, value] of presetLeaves) {
      for (const match of value.matchAll(/var\((--[\w-]+)\)/g)) {
        referenced.add(match[1] ?? '');
      }
    }
    expect(referenced.size).toBeGreaterThan(20);
    const unknown = [...referenced].filter((t) => !allDeclared.has(t)).sort();
    expect(unknown).toEqual([]);
  });

  it('emits var(--token) verbatim into the generated --p-* declarations', () => {
    // This is the evidence for route A: PrimeNG's value emitter rewrites only
    // brace references ({a.b}) and passes any other string through unchanged,
    // so no colour has to be duplicated into TypeScript.
    const emitted = toVariables(
      { primary: { color: 'var(--accent)' }, formField: { borderColor: 'var(--border)' } },
      { prefix: 'p' },
    ) as { declarations: string };
    expect(emitted.declarations).toContain('--p-primary-color:var(--accent);');
    expect(emitted.declarations).toContain('--p-form-field-border-color:var(--border);');
  });

  it('switches PrimeNG on the same attribute the token layer switches on', () => {
    expect(NexGenThemeOptions.options.darkModeSelector).toBe('[data-theme="dark"]');
  });
});
