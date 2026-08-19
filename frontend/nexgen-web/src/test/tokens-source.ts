import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

/**
 * Reads the ACTUAL token stylesheet from disk and parses its custom properties.
 *
 * Tests assert against this rather than against a duplicated table of values:
 * a copy would drift, and a drifting copy of the contrast source is worse than
 * no test at all.
 */
const TOKENS_CSS_PATH = resolve(process.cwd(), 'src/shared/theme/tokens.css');
const GLOBAL_CSS_PATH = resolve(process.cwd(), 'src/styles/global.css');

export function readTokensCss(): string {
  return readFileSync(TOKENS_CSS_PATH, 'utf8');
}

export function readGlobalCss(): string {
  return readFileSync(GLOBAL_CSS_PATH, 'utf8');
}

function stripComments(css: string): string {
  return css.replace(/\/\*[\s\S]*?\*\//g, '');
}

/** All custom properties declared anywhere in tokens.css, in declaration order. */
export function declaredTokenNames(): string[] {
  const names = new Set<string>();
  for (const match of stripComments(readTokensCss()).matchAll(/(--[a-z0-9-]+)\s*:/g)) {
    if (match[1]) names.add(match[1]);
  }
  return [...names];
}

/** The declaration block for a selector, comments removed. */
export function blockFor(selector: string): string {
  const css = stripComments(readTokensCss());
  const index = css.indexOf(selector);
  if (index === -1) throw new Error(`Selector ${selector} not found in tokens.css`);
  const open = css.indexOf('{', index);
  const close = css.indexOf('}', open);
  return css.slice(open + 1, close);
}

function parseBlock(selector: string): Record<string, string> {
  const values: Record<string, string> = {};
  for (const match of blockFor(selector).matchAll(/(--[a-z0-9-]+)\s*:\s*([^;]+);/g)) {
    if (match[1] && match[2]) values[match[1]] = match[2].trim();
  }
  return values;
}

export type ThemeName = 'light' | 'dark';

/**
 * Resolved token values per theme. The dark block overrides the light one
 * exactly as the cascade does in the browser, so what the test measures is what
 * a dark-mode user sees.
 */
export function tokenValues(theme: ThemeName): Record<string, string> {
  const light = parseBlock(':root');
  if (theme === 'light') return light;
  return { ...light, ...parseBlock("[data-theme='dark']") };
}
