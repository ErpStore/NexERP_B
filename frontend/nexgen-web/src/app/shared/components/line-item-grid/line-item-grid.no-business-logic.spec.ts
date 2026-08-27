import { readFileSync, readdirSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

/**
 * This task's own hardest-to-keep constraint, enforced rather than
 * remembered: `LineItemGrid` is an editing surface, and BR-CALC-001 - every
 * document total, tax, discount and round-off - stays server-side
 * (`CalculationService.cs:12-114`). A grid is exactly where "just multiply
 * qty by rate for the running total, it's only a preview" creeps in under
 * deadline pressure, so this test outlives the code review that would
 * otherwise be the only thing catching it.
 */

const DIR = resolve(__dirname);

const SOURCE_EXTENSIONS = ['.ts', '.html'];
const EXCLUDED_SUFFIXES = ['.spec.ts', 'test-fixtures.ts'];

/** Forbidden field names, BR-CALC-001's exact vocabulary, whole-word and case-insensitive. */
const FORBIDDEN_FIELD_NAMES = ['hsn', 'uom', 'cgst', 'sgst', 'igst', 'tcs', 'balqty'];

/**
 * Domain quantities a `*`/`/` next to is the specific defect this test
 * exists to catch - "just multiply qty by rate" is exactly one line.
 * Deliberately narrow: `rowHeightPx`, `viewportRows`, `decimalPlaces` and
 * every other layout/config number in this directory's real code shares no
 * word with this list, so a true grid-mechanics line never matches it.
 */
const DOMAIN_ARITHMETIC_WORDS = [
  'qty',
  'quantity',
  'rate',
  'price',
  'amount',
  'discount',
  'tax',
  'gst',
  'duty',
  'freight',
  'packing',
  'insurance',
  'total',
];

/** A hardcoded GST-rate-shaped literal: 5, 12, 18 or 28, as a percent or a bare multiplier. */
const HARDCODED_GST_RATE =
  /\b(0?\.05|0?\.12|0?\.18|0?\.28|5|12|18|28)\s*%|%\s*(0?\.05|0?\.12|0?\.18|0?\.28|5|12|18|28)\b/;

function sourceFiles(): string[] {
  return readdirSync(DIR, { recursive: true, encoding: 'utf8' })
    .filter((entry) => SOURCE_EXTENSIONS.some((ext) => entry.endsWith(ext)))
    .filter((entry) => !EXCLUDED_SUFFIXES.some((suffix) => entry.endsWith(suffix)))
    .filter((entry) => !entry.includes(`node_modules`))
    .sort();
}

/** Strips `//` line comments and JSDoc-style `*` continuation lines - this test scans code, not prose about code. */
function stripComments(source: string): string {
  return source
    .split('\n')
    .map((line) => {
      const trimmed = line.trim();
      if (trimmed.startsWith('*') || trimmed.startsWith('//') || trimmed.startsWith('/**')) {
        return '';
      }
      const inlineCommentIndex = line.indexOf('//');
      return inlineCommentIndex === -1 ? line : line.slice(0, inlineCommentIndex);
    })
    .join('\n');
}

describe("LineItemGrid - no ERP business logic (BR-CALC-001 and this task's own Business Rules)", () => {
  const files = sourceFiles();

  it('found at least one real source file to scan, so this suite cannot silently pass over an empty directory', () => {
    expect(files.length).toBeGreaterThan(10);
  });

  it.each(FORBIDDEN_FIELD_NAMES)('never references the field name "%s"', (name) => {
    const pattern = new RegExp(`\\b${name}\\b`, 'i');
    const offenders = files.filter((file) =>
      pattern.test(readFileSync(resolve(DIR, file), 'utf8')),
    );
    expect(offenders).toEqual([]);
  });

  it('never multiplies or divides a line using a domain quantity/money word on the same line', () => {
    const offenders: string[] = [];
    for (const file of files) {
      const code = stripComments(readFileSync(resolve(DIR, file), 'utf8'));
      for (const [index, line] of code.split('\n').entries()) {
        const trimmed = line.trim();
        // A `/` in a relative import/export path ('../../foo') is not
        // division - it is the single most common false positive this scan
        // would otherwise produce, given how many files here import from
        // `../../../utils/decimal`.
        if (
          trimmed.startsWith('import ') ||
          trimmed.startsWith('export ') ||
          trimmed.includes(' from ')
        ) {
          continue;
        }
        if (!/[*/]/.test(line)) {
          continue;
        }
        const hasDomainWord = DOMAIN_ARITHMETIC_WORDS.some((word) =>
          new RegExp(`\\b${word}`, 'i').test(line),
        );
        // A bare `*` in an import/export ('export *') or a generic type
        // parameter is not arithmetic - only flag a `*`/`/` that sits
        // between two identifier/number-like operands.
        const looksArithmetic = /[\w)\]]\s*[*/]\s*[\w(]/.test(line);
        if (hasDomainWord && looksArithmetic) {
          offenders.push(`${file}:${index + 1}: ${line.trim()}`);
        }
      }
    }
    expect(offenders).toEqual([]);
  });

  it('never hardcodes a GST-rate-shaped literal', () => {
    const offenders: string[] = [];
    for (const file of files) {
      const code = stripComments(readFileSync(resolve(DIR, file), 'utf8'));
      if (HARDCODED_GST_RATE.test(code)) {
        offenders.push(file);
      }
    }
    expect(offenders).toEqual([]);
  });
});
