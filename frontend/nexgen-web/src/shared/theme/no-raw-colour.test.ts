import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, resolve, sep } from 'node:path';

import { describe, expect, it } from 'vitest';

/**
 * The mechanical half of R-22's defence. ESLint bans colour literals in TS/TSX;
 * ESLint does not read CSS, so this test scans everything under src/ -- .ts,
 * .tsx and .css alike -- and allows a colour literal in exactly one file.
 *
 * A second visual language appears the moment a component hardcodes a colour.
 * Making that fail the test run is the difference between a token layer and a
 * suggestion.
 */
// process.cwd() rather than import.meta.url: under Vitest's jsdom environment
// import.meta.url is not a file: URL.
const SRC = resolve(process.cwd(), 'src') + sep;
const ALLOWED = ['shared/theme/tokens.css'];
const EXTENSIONS = ['.ts', '.tsx', '.css'];

// Built from pieces so the patterns cannot match this file's own source.
const HEX = new RegExp('#' + '[0-9a-fA-F]{3,8}' + '\\b');
const FUNCTIONAL = new RegExp('\\b(?:rgba?|hsla?|lab|lch|oklch|color-mix)' + '\\(');

function walk(dir: string, out: string[] = []): string[] {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) walk(full, out);
    else if (EXTENSIONS.some((extension) => entry.endsWith(extension))) out.push(full);
  }
  return out;
}

describe('no raw colour outside the token layer', () => {
  it('finds none in src/**', () => {
    const offenders: string[] = [];
    for (const file of walk(SRC)) {
      const relative = file.slice(SRC.length).split(sep).join('/');
      if (ALLOWED.includes(relative)) continue;
      // This test file necessarily describes the patterns it bans.
      if (relative.endsWith('no-raw-colour.test.ts')) continue;
      readFileSync(file, 'utf8')
        .split('\n')
        .forEach((line, index) => {
          if (HEX.test(line) || FUNCTIONAL.test(line)) {
            offenders.push(`${relative}:${index + 1}: ${line.trim()}`);
          }
        });
    }
    expect(offenders, offenders.join('\n')).toEqual([]);
  });

  it('does actually catch one when it is there', () => {
    // Proves the scanner is not vacuously green.
    const sample = ['color: ', '#', 'AABBCC', ';'].join('');
    expect(HEX.test(sample)).toBe(true);
    expect(FUNCTIONAL.test(['rgb', '(0 0 0)'].join(''))).toBe(true);
  });
});
