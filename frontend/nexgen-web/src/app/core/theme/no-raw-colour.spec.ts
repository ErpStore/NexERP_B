import { readdirSync, readFileSync } from 'node:fs';
import { join, relative, resolve, sep } from 'node:path';

/**
 * The token layer is the only durable defence against R-22 (two visual
 * languages in one app). ESLint covers the `.ts` and `.html` halves - see the
 * `no-restricted-syntax` colour rule in `eslint.config.js` - but
 * `angular.json`'s `lintFilePatterns` cannot see `.css`/`.scss`, so this scan
 * covers the whole of `src/**` instead of adding a second linter.
 *
 * `src/styles/tokens.css` is the one exemption: it is where colour lives.
 */
const SRC = resolve(process.cwd(), 'src');
const EXEMPT = [join('styles', 'tokens.css')];
const SCANNED_EXTENSIONS = ['.ts', '.html', '.css', '.scss', '.json'];

const HEX = /#[0-9a-fA-F]{3,8}\b/;
const FUNCTIONAL = /\b(rgb|hsl)a?\(/;

function walk(directory: string): string[] {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const full = join(directory, entry.name);
    if (entry.isDirectory()) {
      return walk(full);
    }
    return SCANNED_EXTENSIONS.some((extension) => entry.name.endsWith(extension)) ? [full] : [];
  });
}

function scan(): string[] {
  const offences: string[] = [];
  for (const file of walk(SRC)) {
    const relativePath = relative(SRC, file);
    if (EXEMPT.includes(relativePath)) {
      continue;
    }
    readFileSync(file, 'utf8')
      .split(/\r?\n/)
      .forEach((line, index) => {
        if (HEX.test(line) || FUNCTIONAL.test(line)) {
          const shown = relativePath.split(sep).join('/');
          offences.push(`src/${shown}:${index + 1}: ${line.trim()}`);
        }
      });
  }
  return offences;
}

describe('no raw colour literal outside the token layer', () => {
  it('finds none in src/**', () => {
    expect(scan()).toEqual([]);
  });

  it('would catch one if it were added', () => {
    // Assembled from parts so the scanner - and the repository-wide grep in
    // the task's verification commands - do not flag this file.
    const hex = '#' + 'ff' + '0000';
    const functional = 'rgb' + '(0, 0, 0)';
    expect(HEX.test(`color: ${hex};`)).toBe(true);
    expect(FUNCTIONAL.test(`color: ${functional};`)).toBe(true);
  });

  it('scans a meaningful number of files', () => {
    expect(walk(SRC).length).toBeGreaterThan(10);
  });
});
