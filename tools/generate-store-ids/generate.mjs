// tools/generate-store-ids/generate.mjs
//
// Generates V.SMART/V.SMART.Shared/Utility_Constants/StoreIds.cs from the
// `Store` seed in ApplicationDbContext.cs -- the only definition of these
// ids anywhere (R-66, M2-B05).
//
// Deliberately NOT a .NET tool: no .NET test/generator project exists in
// the solution and creating one is out of this task's scope. Same
// standalone-guard precedent as tools/check-agent-system.sh and
// tools/test-migration-runner.mjs (see that file's header).
//
//   node tools/generate-store-ids/generate.mjs
//
// Exit code: 0 and the file is (re)written if every assertion holds.
// Exit code: 1 and the file is NOT written if any assertion fails --
// this generator must fail loudly, not silently disambiguate or skip a
// bad row. See M2-B05.md Target Result for the assertions this encodes.
import { readFileSync, writeFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'

const REPO_ROOT = fileURLToPath(new URL('../../', import.meta.url))
const SEED_PATH = REPO_ROOT + 'V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs'
const OUT_PATH = REPO_ROOT + 'V.SMART/V.SMART.Shared/Utility_Constants/StoreIds.cs'

// ---------------------------------------------------------------------------
// 1. Extract the `builder.Entity<Store>().HasData(...)` block.
// ---------------------------------------------------------------------------
export function extractStoreSeedBlock(source) {
  const startMarker = 'builder.Entity<Store>().HasData('
  const start = source.indexOf(startMarker)
  if (start === -1) {
    throw new Error(`Could not find "${startMarker}" in ${SEED_PATH}`)
  }
  const openParenIdx = start + startMarker.length - 1
  // Walk parens to find the matching close -- the block contains nested
  // `{ }` but no nested `(` other than the outer HasData( itself, so a
  // simple paren counter is sufficient and won't be fooled by braces.
  let depth = 0
  let i = openParenIdx
  for (; i < source.length; i++) {
    if (source[i] === '(') depth++
    else if (source[i] === ')') {
      depth--
      if (depth === 0) break
    }
  }
  if (depth !== 0) {
    throw new Error('Unbalanced parentheses while scanning the Store HasData(...) block')
  }
  return source.slice(openParenIdx + 1, i)
}

// ---------------------------------------------------------------------------
// 2. Parse each `new Store { StoreId = N, StoreName = "...", ... }` row.
// ---------------------------------------------------------------------------
export function parseStoreRows(block) {
  const rowRe = /new Store\s*\{\s*StoreId\s*=\s*(\d+)\s*,\s*StoreName\s*=\s*"([^"]*)"/g
  const rows = []
  let m
  while ((m = rowRe.exec(block)) !== null) {
    rows.push({ storeId: Number(m[1]), storeName: m[2] })
  }
  return rows
}

// ---------------------------------------------------------------------------
// 3. Deterministic identifier derivation.
//    Split on any run of characters that aren't letters/digits, PascalCase
//    each surviving word, join. Documented here because it must be
//    reproducible by a reader without running the script.
// ---------------------------------------------------------------------------
export function deriveIdentifier(name) {
  const words = name.split(/[^A-Za-z0-9]+/).filter(Boolean)
  let ident = words
    .map((w) => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase())
    .join('')
  if (/^[0-9]/.test(ident)) ident = '_' + ident
  return ident
}

// ---------------------------------------------------------------------------
// 4. Assertions. Every one of these throws (fails loudly) rather than
//    silently dropping or disambiguating a row.
// ---------------------------------------------------------------------------
export function assertInvariants(rows) {
  if (rows.length === 0) {
    throw new Error('Parsed zero Store rows -- the seed format probably changed; regex needs updating')
  }

  const seenIds = new Map()
  for (const r of rows) {
    if (seenIds.has(r.storeId)) {
      throw new Error(
        `Duplicate StoreId ${r.storeId}: "${seenIds.get(r.storeId)}" and "${r.storeName}"`
      )
    }
    seenIds.set(r.storeId, r.storeName)
  }

  const seenNames = new Map()
  for (const r of rows) {
    if (seenNames.has(r.storeName)) {
      throw new Error(`Duplicate StoreName "${r.storeName}" (StoreId ${r.storeId} and ${seenNames.get(r.storeName)})`)
    }
    seenNames.set(r.storeName, r.storeId)
  }

  const seenIdents = new Map()
  for (const r of rows) {
    const ident = deriveIdentifier(r.storeName)
    if (seenIdents.has(ident)) {
      throw new Error(
        `StoreName "${r.storeName}" and "${seenIdents.get(ident).storeName}" both derive identifier "${ident}" -- generator refuses to silently disambiguate`
      )
    }
    seenIdents.set(ident, r)
  }
}

// ---------------------------------------------------------------------------
// 5. Emit the C# file.
// ---------------------------------------------------------------------------
export function renderCSharp(rows) {
  const sorted = [...rows].sort((a, b) => a.storeId - b.storeId)
  const maxIdentLen = Math.max(...sorted.map((r) => deriveIdentifier(r.storeName).length))

  const constLines = sorted
    .map((r) => {
      const ident = deriveIdentifier(r.storeName)
      const pad = ' '.repeat(maxIdentLen - ident.length)
      return `        public const int ${ident}${pad} = ${r.storeId};    // "${r.storeName}"`
    })
    .join('\n')

  const nameEntries = sorted
    .map((r) => `            { ${r.storeId}, "${r.storeName}" },`)
    .join('\n')

  return `// <auto-generated>
// Generated by tools/generate-store-ids/generate.mjs from the \`Store\` seed
// in V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs. Do not edit by
// hand -- if this file is wrong, fix the generator and regenerate:
//
//   node tools/generate-store-ids/generate.mjs
//
// See docs/kb/execution/tasks/M2-B05.md and
// docs/kb/risks/technical-debt-register.md (R-66) for why this exists.
// </auto-generated>
namespace V.SMART.Shared.Utility_Constants
{
    public static class StoreIds
    {
${constLines}

        public static readonly System.Collections.Generic.IReadOnlyDictionary<int, string> Names =
            new System.Collections.Generic.Dictionary<int, string>
            {
${nameEntries}
            };
    }
}
`
}

// ---------------------------------------------------------------------------
// Entry point.
// ---------------------------------------------------------------------------
function main() {
  const source = readFileSync(SEED_PATH, 'utf8')
  const block = extractStoreSeedBlock(source)
  const rows = parseStoreRows(block)
  assertInvariants(rows)
  const csharp = renderCSharp(rows)
  writeFileSync(OUT_PATH, csharp, 'utf8')
  console.log(`Wrote ${OUT_PATH} -- ${rows.length} store(s):`)
  for (const r of [...rows].sort((a, b) => a.storeId - b.storeId)) {
    console.log(`  ${r.storeId}\t${deriveIdentifier(r.storeName)}\t"${r.storeName}"`)
  }
}

// Only run when executed directly (not when imported by the self-test).
if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  try {
    main()
  } catch (err) {
    console.error(`generate-store-ids: FAILED -- ${err.message}`)
    process.exit(1)
  }
}
