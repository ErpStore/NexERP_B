import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

/**
 * ADR-005, enforced rather than remembered.
 *
 * Export and print are **server** capabilities: `ExcelExportService`
 * (`V.SMART/V.SMART.Shared/Services/ExcelExportService.cs:24`, `:113`) builds
 * the bytes and the SPA asks for them. The rule is easy to violate later under
 * schedule pressure - "just add `xlsx` for this one screen" - and a lint rule
 * outlives a code review, which is why this test exists rather than a comment.
 *
 * If a future task genuinely needs one of these, the change is an ADR that
 * supersedes ADR-005, not an edit to this list.
 */
const FORBIDDEN_PACKAGES: readonly string[] = [
  'xlsx',
  'xlsx-populate',
  'exceljs',
  'papaparse',
  'jspdf',
  'pdfmake',
  'pdf-lib',
  'json2csv',
  'write-excel-file',
  'file-saver',
];

interface PackageManifest {
  readonly dependencies?: Record<string, string>;
  readonly devDependencies?: Record<string, string>;
  readonly optionalDependencies?: Record<string, string>;
  readonly peerDependencies?: Record<string, string>;
}

const manifest = JSON.parse(
  readFileSync(resolve(process.cwd(), 'package.json'), 'utf8'),
) as PackageManifest;

const declared = new Set([
  ...Object.keys(manifest.dependencies ?? {}),
  ...Object.keys(manifest.devDependencies ?? {}),
  ...Object.keys(manifest.optionalDependencies ?? {}),
  ...Object.keys(manifest.peerDependencies ?? {}),
]);

describe('ADR-005 - the client never generates a file', () => {
  it.each(FORBIDDEN_PACKAGES)('does not depend on %s', (name) => {
    expect(declared.has(name)).toBe(false);
  });

  it('lists every dependency it checked, so the guard cannot silently pass on an empty set', () => {
    expect(FORBIDDEN_PACKAGES.length).toBeGreaterThan(0);
    expect(declared.size).toBeGreaterThan(0);
  });
});
