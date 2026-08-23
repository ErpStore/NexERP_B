import { readdirSync, readFileSync } from 'node:fs';
import { join, relative, resolve, sep } from 'node:path';

/**
 * The architecture half of M2-C10's enforcement.
 *
 * `eslint.config.js` bans the same patterns, but a lint rule can be silenced
 * with an inline `eslint-disable` and does not see code written before the rule
 * existed. This scan cannot be silenced from inside the file it is scanning.
 *
 * Scope: every `.ts` and `.html` file under `src/`, except this module (which is
 * where `decimal.js` and explicit rounding belong) and the exemptions listed
 * below, each of which carries its reason.
 */
const SRC = resolve(process.cwd(), 'src');

/** Paths relative to `src/`, in POSIX form. */
const DECIMAL_MODULE = 'app/shared/utils/decimal/';

const EXEMPT = new Map<string, string>([
  [
    'app/core/theme/contrast.spec.ts',
    'WCAG contrast ratios are not money: no ERP value passes through it (M2-C04-01).',
  ],
]);

const SCANNED_EXTENSIONS = ['.ts', '.html'];

const MONEY_WORD = String.raw`[A-Za-z_$][A-Za-z0-9_$]*(?:amount|price|rate|qty|quantity|total|tax|discount|gross|net)[A-Za-z0-9_$]*`;

interface Rule {
  readonly name: string;
  readonly pattern: RegExp;
  /** Applied to the raw line rather than to the comment/string-stripped one. */
  readonly raw?: boolean;
}

const RULES: Rule[] = [
  {
    name: 'decimal.js imported outside the decimal module',
    pattern: /from\s*['"]decimal\.js/,
    raw: true,
  },
  { name: '.toFixed() - use format() or the money pipe', pattern: /\.toFixed\s*\(/ },
  { name: 'parseFloat - use parseUserInput()', pattern: /\bparseFloat\s*\(/ },
  { name: 'Math.round/floor/ceil - use round()', pattern: /\bMath\s*\.\s*(round|floor|ceil)\b/ },
  { name: 'DecimalPipe/CurrencyPipe coerce to number', pattern: /\b(DecimalPipe|CurrencyPipe)\b/ },
  {
    name: 'arithmetic applied to a money-named identifier',
    pattern: new RegExp(String.raw`${MONEY_WORD}\s*(?:\+\+|--|[-+*/%]=?)(?!=)`, 'i'),
  },
  {
    name: 'arithmetic producing a money-named identifier',
    pattern: new RegExp(String.raw`[-+*/%]\s*${MONEY_WORD}\b`, 'i'),
  },
];

/**
 * Blank out string literals and comments so that an import path, a CSS class or
 * a sentence of prose cannot look like arithmetic. Crude on purpose: it is a
 * scanner, not a parser, and it errs towards scanning less rather than towards
 * false accusations.
 */
function stripLiterals(source: string): string {
  return source
    .replace(/\/\*[\s\S]*?\*\//g, ' ')
    .replace(/(^|[^:])\/\/.*$/gm, '$1')
    .replace(/<!--[\s\S]*?-->/g, ' ')
    .replace(/'(?:\\.|[^'\\])*'/g, "''")
    .replace(/"(?:\\.|[^"\\])*"/g, '""')
    .replace(/`(?:\\.|[^`\\])*`/g, '``');
}

function walk(directory: string): string[] {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const full = join(directory, entry.name);
    if (entry.isDirectory()) {
      return walk(full);
    }
    return SCANNED_EXTENSIONS.some((extension) => entry.name.endsWith(extension)) ? [full] : [];
  });
}

function scannedFiles(): { path: string; relativePath: string }[] {
  return walk(SRC)
    .map((path) => ({ path, relativePath: relative(SRC, path).split(sep).join('/') }))
    .filter(
      ({ relativePath }) => !relativePath.startsWith(DECIMAL_MODULE) && !EXEMPT.has(relativePath),
    );
}

export function scan(): string[] {
  const offences: string[] = [];
  for (const { path, relativePath } of scannedFiles()) {
    const rawLines = readFileSync(path, 'utf8').split(/\r?\n/);
    const codeLines = stripLiterals(rawLines.join('\n')).split('\n');
    rawLines.forEach((rawLine, index) => {
      const codeLine = codeLines[index] ?? '';
      for (const rule of RULES) {
        if (rule.pattern.test(rule.raw === true ? rawLine : codeLine)) {
          offences.push(`src/${relativePath}:${index + 1}: ${rule.name} -- ${rawLine.trim()}`);
        }
      }
    });
  }
  return offences;
}

describe('no float money arithmetic outside shared/utils/decimal', () => {
  it('finds no offence in src/**', () => {
    expect(scan()).toEqual([]);
  });

  it('scans a meaningful number of files', () => {
    expect(scannedFiles().length).toBeGreaterThan(10);
  });

  it('would catch each banned pattern if it were written', () => {
    // Assembled from parts so this file is not itself an offender, and so the
    // repository-wide greps in the task's verification commands stay clean.
    const samples: [string, string][] = [
      ['decimal.js imported', `import Decimal from '${'decimal'}.js';`],
      ['toFixed', `const shown = value.to${'Fixed'}(2);`],
      ['parseFloat', `const n = parse${'Float'}(raw);`],
      ['Math.round', `const n = Math.${'round'}(x);`],
      ['CurrencyPipe', `imports: [Currency${'Pipe'}]`],
      ['arithmetic after', 'const t = lineTotal * 2;'],
      ['arithmetic before', 'const t = 2 * lineTotal;'],
      ['compound assignment', 'runningTotal += 1;'],
    ];
    for (const [label, sample] of samples) {
      const stripped = stripLiterals(sample);
      const caught = RULES.some((rule) => rule.pattern.test(rule.raw === true ? sample : stripped));
      expect(caught, `${label} should be caught`).toBe(true);
    }
  });

  it('does not accuse prose, import paths or comparisons', () => {
    const innocent = [
      "import { format } from '../utils/decimal/total-free-path';",
      'if (lineTotal === undefined) { return; }',
      'const label = "the total price of the order";',
      '// discount / tax handling lives on the server',
    ];
    for (const line of innocent) {
      const stripped = stripLiterals(line);
      const caught = RULES.some((rule) => rule.pattern.test(rule.raw === true ? line : stripped));
      expect(caught, `should not accuse: ${line}`).toBe(false);
    }
  });

  it('documents a reason for every exemption', () => {
    for (const [path, reason] of EXEMPT) {
      expect(reason.length, `${path} needs a reason`).toBeGreaterThan(20);
    }
  });
});
