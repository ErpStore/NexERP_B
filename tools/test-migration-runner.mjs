// tools/test-migration-runner.mjs
//
// Exercises the ORCHESTRATION LOGIC of .claude/workflows/migration-runner.js
// with stubbed agents: model routing, retry bounds, escalation triggers and
// every stop condition. No model calls and no repository writes -- this is
// pure control flow, which is exactly the part that must be deterministic.
//
// A loop that decides retries and escalation by inference cannot be tested.
// That is the main reason the orchestrator is a script and not a prompt.
//
//   node tools/test-migration-runner.mjs
//
// Exit code: 0 if every assertion holds, 1 otherwise.
//
// Deliberately NOT a .NET test project: none exists in the solution and
// creating one is task M0-12-01's scope (INV-023). Same standalone-guard
// precedent as tools/check-agent-system.sh and tools/check-no-build-output.sh.
import { readFileSync } from 'node:fs'

const SRC = new URL('../.claude/workflows/migration-runner.js', import.meta.url)
const body = readFileSync(SRC, 'utf8').replace(/^export const meta/m, 'const meta')

const makeRunner = () =>
  new Function('agent', 'parallel', 'pipeline', 'log', 'phase', 'args', 'budget',
    `return (async () => { ${body} })()`)

let failures = 0
const check = (name, cond, detail) => {
  if (cond) console.log(`  ok   ${name}`)
  else { console.log(`  FAIL ${name}${detail ? ' — ' + detail : ''}`); failures++ }
}

async function run({ selection, verdicts, diagnoses, args = {}, budgetTotal = null }) {
  const calls = []
  let vi = 0, di = 0
  const agent = async (prompt, opts = {}) => {
    calls.push({ phase: opts.phase, label: opts.label, model: opts.model, agentType: opts.agentType })
    switch (opts.phase) {
      case 'Select': return selection
      case 'Investigate': return { summary: 'stub', escalationTriggers: [] }
      case 'Implement': return { implemented: 'stub', filesChanged: ['a.cs'], selfCheck: 'ok' }
      case 'Validate': return verdicts[Math.min(vi++, verdicts.length - 1)]
      case 'Diagnose': return diagnoses[Math.min(di++, diagnoses.length - 1)]
      default: return { status: 'REVIEW', nextTaskId: 'M0-02', nextTaskTitle: 'next' }
    }
  }
  const budget = { total: budgetTotal, spent: () => 0, remaining: () => budgetTotal ?? Infinity }
  const result = await makeRunner()(agent, null, null, () => {}, () => {}, args, budget)
  return { result, calls }
}

const SEL = (over = {}) => ({
  stopRequested: false, taskId: 'M0-15', title: 'Toolchain baseline',
  complexity: 'MEDIUM', risk: 'LOW', requiresHuman: false, safetyStop: false,
  rationale: 'P0', maxRetries: 2, ...over,
})
const PASS = { verdict: 'PASS', criteria: ['all met'], scopeOk: true, failureCategory: 'none' }
const FAIL = (cat = 'build') => ({ verdict: 'FAIL', failureCategory: cat, whatFailed: 'x', criteria: [], scopeOk: true })
const DIAG = (over = {}) => ({ rootCause: 'typo', causeClass: 'implementation-error', disposition: 'fixed', ...over })

const modelsFor = (calls, phase) => calls.filter(c => c.phase === phase).map(c => c.model)

console.log('\n1. clean PASS, one task')
{
  const { result, calls } = await run({ selection: SEL(), verdicts: [PASS], diagnoses: [DIAG()] })
  check('one implement attempt', calls.filter(c => c.phase === 'Implement').length === 1)
  check('verdict recorded PASS', result.results[0].verdict === 'PASS')
  check('stops at task budget', /task budget reached/.test(result.stopReason), result.stopReason)
  check('MEDIUM routes sonnet to implement', modelsFor(calls, 'Implement')[0] === 'sonnet')
  check('MEDIUM routes sonnet to validate', modelsFor(calls, 'Validate')[0] === 'sonnet')
  check('LOW-cost investigate on MEDIUM = sonnet', modelsFor(calls, 'Investigate')[0] === 'sonnet')
  check('delegates to real agent types',
    calls.some(c => c.agentType === 'migration-implementer') &&
    calls.some(c => c.agentType === 'migration-validator'))
}

console.log('\n2. FAIL(build) then PASS — retry at same model')
{
  const { result, calls } = await run({
    selection: SEL(), verdicts: [FAIL('build'), PASS], diagnoses: [DIAG()],
  })
  check('two implement attempts', calls.filter(c => c.phase === 'Implement').length === 2)
  check('one diagnose', calls.filter(c => c.phase === 'Diagnose').length === 1)
  check('first diagnose stays sonnet (1st failure, soft category)', modelsFor(calls, 'Diagnose')[0] === 'sonnet')
  check('ends PASS', result.results[0].verdict === 'PASS')
  check('attempts counted = 2', result.results[0].attempts === 2)
}

console.log('\n3. FAIL(business-rule) — escalates immediately, no retry spent at sonnet')
{
  const { calls } = await run({
    selection: SEL(), verdicts: [FAIL('business-rule'), PASS],
    diagnoses: [DIAG({ causeClass: 'business-rule', disposition: 'escalate' })],
  })
  check('diagnose escalated to opus', modelsFor(calls, 'Diagnose')[0] === 'opus')
  check('retry implements at opus', modelsFor(calls, 'Implement')[1] === 'opus')
  check('retry validates at opus', modelsFor(calls, 'Validate')[1] === 'opus')
}

console.log('\n4. FAIL x3 — retry budget exhausted, BLOCKED, no 4th attempt')
{
  const { result, calls } = await run({
    selection: SEL(), verdicts: [FAIL(), FAIL(), FAIL()], diagnoses: [DIAG()],
  })
  check('exactly 3 implement attempts (maxRetries=2)', calls.filter(c => c.phase === 'Implement').length === 3,
    `got ${calls.filter(c => c.phase === 'Implement').length}`)
  check('stops on exhausted budget', /retry budget exhausted/.test(result.stopReason), result.stopReason)
  check('run halts, task not counted as completed', result.tasksCompleted === 0)
  check('but it is reported as attempted+blocked', result.tasksAttempted === 1 && result.blocked.length === 1)
  check('a stop was recorded to disk', calls.some(c => c.label === 'record-stop'))
}

console.log('\n5. HIGH risk forces opus validation even at MEDIUM complexity')
{
  const { calls } = await run({ selection: SEL({ risk: 'HIGH' }), verdicts: [PASS], diagnoses: [DIAG()] })
  check('validate = opus (risk floor)', modelsFor(calls, 'Validate')[0] === 'opus')
  check('implement stays sonnet (risk does not raise implement)', modelsFor(calls, 'Implement')[0] === 'sonnet')
}

console.log('\n6. LOW complexity — cheapest model investigates, never implements')
{
  const { calls } = await run({ selection: SEL({ complexity: 'LOW' }), verdicts: [PASS], diagnoses: [DIAG()] })
  check('investigate = haiku', modelsFor(calls, 'Investigate')[0] === 'haiku')
  check('implement floored to sonnet', modelsFor(calls, 'Implement')[0] === 'sonnet')
  check('validate never haiku', modelsFor(calls, 'Validate')[0] !== 'haiku')
}

console.log('\n7. HIGH complexity — opus throughout')
{
  const { calls } = await run({ selection: SEL({ complexity: 'HIGH' }), verdicts: [PASS], diagnoses: [DIAG()] })
  check('investigate = opus', modelsFor(calls, 'Investigate')[0] === 'opus')
  check('implement = opus', modelsFor(calls, 'Implement')[0] === 'opus')
}

console.log('\n8. stop conditions before any work')
{
  const a = await run({ selection: SEL({ stopRequested: true, stopReason: 'human asked' }), verdicts: [PASS], diagnoses: [DIAG()] })
  check('STOP_REQUESTED halts before implementing', a.calls.filter(c => c.phase === 'Implement').length === 0)
  check('stop reason preserved', /human asked/.test(a.result.stopReason), a.result.stopReason)

  const b = await run({ selection: SEL({ requiresHuman: true, humanReason: 'needs DBA access' }), verdicts: [PASS], diagnoses: [DIAG()] })
  check('requiresHuman halts', /requires a human/.test(b.result.stopReason), b.result.stopReason)
  check('no implementation attempted', b.calls.filter(c => c.phase === 'Implement').length === 0)

  const c = await run({ selection: SEL({ safetyStop: true, safetyReason: 'dirty tree' }), verdicts: [PASS], diagnoses: [DIAG()] })
  check('safetyStop halts', /safety stop/.test(c.result.stopReason), c.result.stopReason)

  const d = await run({ selection: SEL({ taskId: '' , rationale: 'all done'}), verdicts: [PASS], diagnoses: [DIAG()] })
  check('no ready task halts cleanly', /no dependency-ready task remains/.test(d.result.stopReason), d.result.stopReason)

  const e = await run({ selection: SEL(), verdicts: [PASS], diagnoses: [DIAG()], budgetTotal: 1000 })
  check('budget reserve halts before work', /execution budget reached/.test(e.result.stopReason), e.result.stopReason)
  check('no agent spawned under budget stop', e.calls.length === 1 && e.calls[0].label === 'record-stop')
}

console.log('\n8b. investigation surfaces an escalation trigger — raises the work to HIGH')
{
  const calls = []
  const agent = async (prompt, opts = {}) => {
    calls.push({ phase: opts.phase, model: opts.model })
    switch (opts.phase) {
      case 'Select': return SEL({ complexity: 'LOW', risk: 'LOW' })
      case 'Investigate': return { summary: 's', escalationTriggers: ['business rule unclear'] }
      case 'Implement': return { implemented: 's', filesChanged: [], selfCheck: 'ok' }
      case 'Validate': return PASS
      default: return { status: 'REVIEW', nextTaskId: '' }
    }
  }
  const budget = { total: null, spent: () => 0, remaining: () => Infinity }
  await makeRunner()(agent, null, null, () => {}, () => {}, {}, budget)
  const m = (ph) => calls.filter(c => c.phase === ph).map(c => c.model)
  // The task was classified LOW from frontmatter, before anyone had read the code.
  check('investigate still ran cheap', m('Investigate')[0] === 'haiku')
  check('implement raised to opus by the trigger', m('Implement')[0] === 'opus')
  check('validate raised to opus by the trigger', m('Validate')[0] === 'opus')
}

console.log('\n9. environment failure blocks instead of retrying')
{
  const { result, calls } = await run({
    selection: SEL(), verdicts: [FAIL('environment'), PASS],
    diagnoses: [DIAG({ causeClass: 'environment', disposition: 'blocked', rootCause: 'no DB access' })],
  })
  check('does not retry an environment failure', calls.filter(c => c.phase === 'Implement').length === 1)
  check('blocks with the cause', /blocked during diagnosis/.test(result.stopReason), result.stopReason)
}

console.log('\n10. repeated fix already tried — escalates rather than looping')
{
  const { calls } = await run({
    selection: SEL(), verdicts: [FAIL(), PASS],
    diagnoses: [DIAG({ triedBefore: true })],
  })
  check('escalates on a repeat fix', modelsFor(calls, 'Implement')[1] === 'opus')
}

console.log('\n11. multi-task run honours maxTasks')
{
  const { result, calls } = await run({ selection: SEL(), verdicts: [PASS], diagnoses: [DIAG()], args: { maxTasks: 3 } })
  check('processes 3 tasks', result.tasksCompleted === 3, `got ${result.tasksCompleted}`)
  check('3 validations ran', calls.filter(c => c.phase === 'Validate').length === 3)
}

console.log('\n12. maxRetries is configurable per run')
{
  const { calls } = await run({ selection: SEL({ maxRetries: 0 }), verdicts: [FAIL(), PASS], diagnoses: [DIAG()] })
  check('maxRetries=0 means a single attempt', calls.filter(c => c.phase === 'Implement').length === 1)
}

console.log(failures === 0
  ? `\nLOOP LOGIC OK — all assertions passed.`
  : `\nLOOP LOGIC FAILED — ${failures} assertion(s) failed.`)
process.exit(failures === 0 ? 0 : 1)
