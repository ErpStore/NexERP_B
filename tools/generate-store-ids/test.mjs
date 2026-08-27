// tools/generate-store-ids/test.mjs
//
// Self-test for generate.mjs's parsing, identifier derivation and
// assertions. Runs against the REAL seed (to prove today's data is clean)
// plus synthetic in-memory fixtures for each failure mode the generator
// must catch -- no file on disk is ever mutated, so this is safe to run
// any time.
//
//   node tools/generate-store-ids/test.mjs
//
// Exit code: 0 if every check holds, 1 otherwise. Same convention as
// tools/test-migration-runner.mjs.
import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import {
  extractStoreSeedBlock,
  parseStoreRows,
  deriveIdentifier,
  assertInvariants,
} from './generate.mjs'

const REPO_ROOT = fileURLToPath(new URL('../../', import.meta.url))
const SEED_PATH = REPO_ROOT + 'V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs'

let failures = 0
function check(label, fn) {
  try {
    fn()
    console.log(`  ok  ${label}`)
  } catch (err) {
    failures++
    console.log(`FAIL  ${label}\n        ${err.message}`)
  }
}
function expectThrows(label, fn) {
  try {
    fn()
    failures++
    console.log(`FAIL  ${label}\n        expected an exception, none was thrown`)
  } catch {
    console.log(`  ok  ${label} (raised as expected)`)
  }
}

console.log('-- Against the real seed --')
const source = readFileSync(SEED_PATH, 'utf8')
const block = extractStoreSeedBlock(source)
const rows = parseStoreRows(block)

check('exactly 9 Store rows parsed', () => {
  if (rows.length !== 9) throw new Error(`expected 9, got ${rows.length}`)
})
check('real seed passes assertInvariants (no duplicates, no identifier collisions)', () => {
  assertInvariants(rows)
})
check('StoreId 6 is "REJECTION STORE"', () => {
  const r = rows.find((x) => x.storeId === 6)
  if (!r || r.storeName !== 'REJECTION STORE') throw new Error(`got ${JSON.stringify(r)}`)
})
check('StoreId 7 is "REWORK STORE"', () => {
  const r = rows.find((x) => x.storeId === 7)
  if (!r || r.storeName !== 'REWORK STORE') throw new Error(`got ${JSON.stringify(r)}`)
})
check('derived identifiers for all 9 real rows are unique and PascalCase-shaped', () => {
  const idents = rows.map((r) => deriveIdentifier(r.storeName))
  if (new Set(idents).size !== idents.length) throw new Error(`collision among ${idents.join(', ')}`)
  for (const id of idents) {
    if (!/^[A-Za-z_][A-Za-z0-9]*$/.test(id)) throw new Error(`"${id}" is not a valid C# identifier`)
  }
})
check('deriveIdentifier handles the punctuation/parenthesis case deterministically', () => {
  const got = deriveIdentifier('FINISH GOODS (FG) STORE')
  if (got !== 'FinishGoodsFgStore') throw new Error(`got "${got}"`)
})

console.log('\n-- Synthetic failure modes (in-memory only, nothing on disk touched) --')
expectThrows('duplicate StoreId is rejected', () => {
  assertInvariants([
    { storeId: 1, storeName: 'A STORE' },
    { storeId: 1, storeName: 'B STORE' },
  ])
})
expectThrows('duplicate StoreName is rejected', () => {
  assertInvariants([
    { storeId: 1, storeName: 'SAME STORE' },
    { storeId: 2, storeName: 'SAME STORE' },
  ])
})
expectThrows('two names collapsing to the same identifier is rejected, not silently disambiguated', () => {
  assertInvariants([
    { storeId: 1, storeName: 'MAIN STORE' },
    { storeId: 2, storeName: 'MAIN  STORE' }, // double space -> same identifier, different string
  ])
})
expectThrows('empty row set is rejected', () => {
  assertInvariants([])
})
check('a clean synthetic set with no violations passes', () => {
  assertInvariants([
    { storeId: 1, storeName: 'ALPHA STORE' },
    { storeId: 2, storeName: 'BETA STORE' },
  ])
})

console.log(`\n${failures === 0 ? 'ALL CHECKS PASSED' : `${failures} CHECK(S) FAILED`}`)
process.exit(failures === 0 ? 0 : 1)
