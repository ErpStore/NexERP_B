export const meta = {
  name: 'migration-runner',
  description: 'Autonomous V.SMART/NexGen ERP migration loop: select, classify, route, investigate, implement, validate, retry, escalate, complete, repeat',
  whenToUse: 'Start the autonomous migration runner. Processes dependency-ready tasks continuously until a stop condition is reached.',
  phases: [
    { title: 'Select', detail: 'read migration state, pick the next dependency-ready task, classify it' },
    { title: 'Investigate', detail: 'read-only investigation of the legacy implementation' },
    { title: 'Implement', detail: 'implement exactly the current task' },
    { title: 'Validate', detail: 'independent PASS/FAIL against acceptance criteria' },
    { title: 'Diagnose', detail: 'root-cause a failed validation, fix or escalate' },
    { title: 'Complete', detail: 'record the outcome, update the KB, hand over to the next task' },
    { title: 'Halt', detail: 'record the stop reason in runner-state.md' },
  ],
}

// ===========================================================================
// migration-runner — the orchestration mechanism.
//
// The agents in .claude/agents/ are WORKERS. This script is the ORCHESTRATOR:
// it owns task selection, model routing, the retry/escalation decision, the
// completion decision and every stop condition. Control flow here is real
// JavaScript, so the loop is deterministic rather than model-improvised.
//
// Specification (human-readable): docs/kb/execution/autonomous-runner.md (KB-091).
// This file is the EXECUTABLE authority for the values in KB-091 sections 3-6;
// tools/check-agent-system.sh asserts the two agree, because a script cannot
// read the document (workflow scripts have no filesystem access).
//
// PERSISTENT STATE. This script holds no state that matters. Everything the
// loop depends on is read from and written to the repository by the agents:
//   docs/kb/execution/runner-state.md    (KB-093) run status, attempt, models
//   docs/kb/execution/current-task.md    (KB-089) the active task
//   docs/kb/execution/task-tracker.md    (KB-081) canonical task status
//   docs/kb/execution/failure-log.md     (KB-092) every failed attempt
// Only compact structured summaries pass between stages -- never a transcript.
// ===========================================================================

const REPO = 'C:\\Kumar\\NexGen-ERP---2025-master\\NexGen-ERP---2025-master'

// --- configuration (override per run via the `args` object) ----------------
const cfg = {
  maxTasks: 5,                 // tasks per run. Was 1 ("one task, one session") until
                               // 2026-08-20, when the owner asked the runner to continue.
                               // 5 is a SAFETY BOUND, not a target: the run also stops on
                               // no-ready-task or budgetReserve, whichever comes first.
                               // Each task still gets its own branch FROM MASTER, and
                               // allowMerge/allowPush stay false -- continuing produces more
                               // branches awaiting review, never anything on master.
                               // NOTE: a task closes as `Needs Review`, so its dependents are
                               // NOT selectable. Consecutive tasks are therefore INDEPENDENT
                               // ones; a dependency chain still advances only via a human merge.
                               // That is deliberate (owner decision 2026-08-20), not a bug.
  maxRetries: 2,               // 2 retries = up to 3 implementation attempts
  escalateAfterFailures: 2,    // the 2nd FAIL on a task forces the escalation model
  maxEscalations: 1,           // one escalation per task, then BLOCKED
  budgetReserve: 60000,        // stop if fewer output tokens than this remain
  allowMerge: false,           // never merge or push without a human instruction
  allowPush: false,
  allowSchemaChange: false,
  allowDestructiveDb: false,
  stopOnRequiresHuman: true,   // honour a task marked as needing human approval
  ...(typeof args === 'object' && args !== null ? args : {}),
}

// --- model routing (KB-091 section 5.1) ------------------------------------
// Aliases only. A dated model id pins a model that goes stale.
const ROUTING = {
  investigate: { LOW: 'haiku',  MEDIUM: 'sonnet', HIGH: 'opus' },
  implement:   { LOW: 'sonnet', MEDIUM: 'sonnet', HIGH: 'opus' },
  validate:    { LOW: 'sonnet', MEDIUM: 'sonnet', HIGH: 'opus' },
  diagnose:    { LOW: 'sonnet', MEDIUM: 'sonnet', HIGH: 'opus' },
}
const ESCALATION_MODEL = 'opus'

// Reasoning effort: spend it where correctness is decided, save it elsewhere.
const EFFORT = { investigate: 'medium', implement: 'medium', validate: 'high', diagnose: 'high' }

// Floors that override the table (KB-091 section 5.2).
function routeModel(role, complexity, risk, escalated) {
  if (escalated) return ESCALATION_MODEL
  let model = (ROUTING[role] || {})[complexity] || 'sonnet'
  if (role === 'validate') {
    if (model === 'haiku') model = 'sonnet'          // floor 1: never cheapest
    if (risk === 'HIGH') model = 'opus'              // floor 2: high risk
  }
  if (role === 'implement' && model === 'haiku') model = 'sonnet'  // floor 3
  return model
}

// --- structured contracts --------------------------------------------------
const SELECTION = {
  type: 'object', additionalProperties: false,
  required: ['stopRequested', 'taskId', 'complexity', 'risk', 'requiresHuman', 'safetyStop', 'rationale'],
  properties: {
    stopRequested: { type: 'boolean', description: 'runner-state.md requests a stop' },
    stopReason: { type: 'string' },
    taskId: { type: 'string', description: 'next dependency-ready task id, or "" if none' },
    title: { type: 'string' },
    complexity: { type: 'string', enum: ['LOW', 'MEDIUM', 'HIGH'] },
    risk: { type: 'string', enum: ['LOW', 'MEDIUM', 'HIGH'] },
    classificationReason: { type: 'string' },
    maxRetries: { type: 'integer' },
    requiresHuman: { type: 'boolean', description: 'needs a product/architecture decision, credentials, DBA access, or approval' },
    humanReason: { type: 'string' },
    safetyStop: { type: 'boolean', description: 'a KB-091 section 8 condition blocks starting this task' },
    safetyReason: { type: 'string' },
    tiedCandidates: { type: 'array', items: { type: 'string' } },
    rationale: { type: 'string' },
  },
}

const INVESTIGATION = {
  type: 'object', additionalProperties: false,
  required: ['summary', 'escalationTriggers'],
  properties: {
    summary: { type: 'string' },
    reusedInvestigations: { type: 'array', items: { type: 'string' } },
    findings: { type: 'array', items: { type: 'string' } },
    unknowns: { type: 'array', items: { type: 'string' } },
    escalationTriggers: { type: 'array', items: { type: 'string' }, description: 'KB-091 section 6.3 triggers observed' },
  },
}

const IMPLEMENTATION = {
  type: 'object', additionalProperties: false,
  required: ['implemented', 'filesChanged', 'selfCheck'],
  properties: {
    implemented: { type: 'string' },
    filesChanged: { type: 'array', items: { type: 'string' } },
    commandsRun: { type: 'array', items: { type: 'string' } },
    notDone: { type: 'array', items: { type: 'string' } },
    assumptions: { type: 'array', items: { type: 'string' } },
    blocked: { type: 'boolean' },
    blockedReason: { type: 'string' },
    selfCheck: { type: 'string', description: 'honest read of the acceptance criteria' },
  },
}

const VERDICT = {
  type: 'object', additionalProperties: false,
  required: ['verdict', 'criteria', 'scopeOk'],
  properties: {
    verdict: { type: 'string', enum: ['PASS', 'FAIL'] },
    failureCategory: {
      type: 'string',
      enum: ['build', 'test', 'acceptance-criterion', 'regression', 'business-rule', 'architecture', 'environment', 'none'],
    },
    whatFailed: { type: 'string' },
    evidence: { type: 'string' },
    criteria: { type: 'array', items: { type: 'string' } },
    regressions: { type: 'string' },
    missingRules: { type: 'string' },
    scopeOk: { type: 'boolean' },
  },
}

const DIAGNOSIS = {
  type: 'object', additionalProperties: false,
  required: ['rootCause', 'causeClass', 'disposition'],
  properties: {
    rootCause: { type: 'string', description: '"unknown" is legitimate and escalates' },
    causeClass: {
      type: 'string',
      enum: ['implementation-error', 'missing-dependency', 'business-rule', 'architecture', 'legacy-behaviour', 'environment', 'unknown'],
    },
    disposition: { type: 'string', enum: ['fixed', 'retry', 'escalate', 'blocked'] },
    fixApplied: { type: 'string' },
    triedBefore: { type: 'boolean', description: 'this fix is already in failure-log.md for this task' },
    escalateReason: { type: 'string' },
  },
}

const COMPLETION = {
  type: 'object', additionalProperties: false,
  required: ['status', 'nextTaskId'],
  properties: {
    status: { type: 'string', enum: ['REVIEW', 'COMPLETED', 'BLOCKED'] },
    statusReason: { type: 'string' },
    kbUpdated: { type: 'array', items: { type: 'string' } },
    discoveries: { type: 'array', items: { type: 'string' } },
    nextTaskId: { type: 'string', description: 'next dependency-ready task, or "" if none' },
    nextTaskTitle: { type: 'string' },
  },
}

// --- shared preamble -------------------------------------------------------
// Every agent bootstraps from the repository, never from a conversation.
const PREAMBLE = `Repository root: ${REPO}
That NESTED folder is the git repository; the parent directory is only a wrapper and every
relative path breaks if you use it. Work from the nested root.

Read ${REPO}\\CLAUDE.md first -- invariant context, authority order, standing constraints,
known traps. Then read only what your task names. The repository is the persistent memory of
this migration; nothing is being pasted to you from a previous conversation, and nothing you
learn survives unless you write it into the repository.

Procedure: docs/kb/execution/workflow.md (KB-088).
Runner policy: docs/kb/execution/autonomous-runner.md (KB-091).

Hard constraints: do NOT merge or push. Do NOT change the database schema unless the task
explicitly authorises it. Do NOT reimplement ERP business logic in TypeScript. Do NOT invent
a business rule -- one without file:line evidence is not a rule. Do NOT touch unrelated
modules. Never claim a command result you did not observe.`

const cfgLine = `Runner config: maxRetries=${cfg.maxRetries}, allowMerge=${cfg.allowMerge}, ` +
  `allowSchemaChange=${cfg.allowSchemaChange}, allowDestructiveDb=${cfg.allowDestructiveDb}.`

const j = (o) => JSON.stringify(o, null, 1)

// ===========================================================================
// The loop
// ===========================================================================
const completed = []
let stopReason = null
let tasksDone = 0

while (tasksDone < cfg.maxTasks && !stopReason) {

  // --- budget stop -------------------------------------------------------
  if (budget.total && budget.remaining() < cfg.budgetReserve) {
    stopReason = `execution budget reached (${Math.round(budget.remaining() / 1000)}k output tokens left, reserve ${Math.round(cfg.budgetReserve / 1000)}k)`
    break
  }

  // --- 1. SELECT + CLASSIFY ---------------------------------------------
  phase('Select')
  const sel = await agent(`${PREAMBLE}

You are selecting the next migration task. Do not implement anything.

1. Read docs/kb/execution/runner-state.md (KB-093). If its Status is STOP_REQUESTED, or a
   stop has been requested by a human, return stopRequested=true with the reason and stop.
2. Read docs/kb/execution/current-task.md (KB-089). If it holds a task that is NOT finished
   (its Run State shows an attempt in progress), return THAT task -- resume, never restart.
3. Otherwise apply the ready-task selection rule in
   docs/kb/execution/dependency-graph.md (section "Ready-task selection rule") against
   docs/kb/execution/task-tracker.md (KB-081), which is the status authority.

Selection rules that matter:
- NEVER return a task whose status is Completed. M0-00, M0-01-01 and M0-01-02 are finished
  history and re-opening one is a bug, not work.
- A prerequisite at "Needs Review" (committed, unmerged) still BLOCKS a merge-dependent
  successor. Do not treat Needs Review as Completed.
- Parent container tasks are never worked directly, only their children.
- If no task is dependency-ready, return taskId="" with the reason.
- If two candidates rank equally and are genuinely independent, return the higher-ranked by
  the documented tie-breaks and list both in tiedCandidates.

Then CLASSIFY the selected task per KB-091 section 4, deriving complexity and risk from its
frontmatter (task_type, estimate, depends_on, business_rules, source_files). An explicit
complexity/risk in the task's frontmatter wins over the derivation.

Set requiresHuman=true if the task needs a product decision, an architecture decision, DBA
access, credentials, or an environment that is unavailable. Set safetyStop=true if any
KB-091 section 8 condition prevents STARTING it -- for example a dirty working tree the task
requires to be clean, or a branch cut from an unexpected point.

Finally, WRITE docs/kb/execution/runner-state.md: Status RUNNING, the selected task, attempt
0, the classification and the models this run will use. That file is how a killed run
resumes, so write it before returning.`,
    { label: 'select+classify', phase: 'Select', model: 'sonnet', effort: 'medium', schema: SELECTION })

  if (!sel) { stopReason = 'selection agent failed to return a result'; break }
  if (sel.stopRequested) { stopReason = `stop requested: ${sel.stopReason || 'no reason recorded'}`; break }
  if (!sel.taskId) { stopReason = `no dependency-ready task remains: ${sel.rationale || ''}`.trim(); break }
  if (sel.safetyStop) { stopReason = `safety stop before starting ${sel.taskId}: ${sel.safetyReason || 'unspecified'}`; break }
  if (sel.requiresHuman && cfg.stopOnRequiresHuman) {
    stopReason = `${sel.taskId} requires a human: ${sel.humanReason || 'unspecified'}`
    break
  }

  const task = sel.taskId
  const complexity = sel.complexity
  const risk = sel.risk
  const maxRetries = Number.isInteger(sel.maxRetries) ? sel.maxRetries : cfg.maxRetries

  log(`${task} — ${sel.title || ''} | complexity ${complexity}, risk ${risk} | ${sel.classificationReason || ''}`)
  if (sel.tiedCandidates && sel.tiedCandidates.length > 1) {
    log(`tied candidates (owner may prefer another): ${sel.tiedCandidates.join(', ')}`)
  }

  // --- 2. INVESTIGATE ----------------------------------------------------
  phase('Investigate')
  const investigateModel = routeModel('investigate', complexity, risk, false)
  log(`investigate → ${investigateModel}`)

  const inv = await agent(`${PREAMBLE}

Investigate what task ${task} needs to know, READ-ONLY. You modify no application code.

Task specification: docs/kb/execution/tasks/${task}.md
Active task context: docs/kb/execution/current-task.md

BEFORE investigating anything, search docs/kb/investigation-registry.md (KB-003). If a
finding is Complete and not stale, REUSE it and cite the doc_id/INV id in
reusedInvestigations rather than re-deriving it -- re-derivation costs a model call and
produces a second answer that may not match the first. Investigate only a documented gap.

Use "git grep --untracked", never plain "git grep" -- plain git grep silently skips
V.SMART.Api/, which is largely untracked, and you will report "not found" for code that exists.

Report findings with file:line evidence, classified Confirmed / Inferred / Unknown. Record
negative results too. Write anything durable into the investigation registry.

In escalationTriggers, list any KB-091 section 6.3 trigger you observed: business rules
unclear or contradicted, an architecture decision required, two or more modules involved, or
legacy behaviour that cannot be determined from source. Report them; do not act on them.`,
    { label: `investigate:${task}`, phase: 'Investigate', agentType: 'migration-investigator',
      model: investigateModel, effort: EFFORT.investigate, schema: INVESTIGATION })

  // KB-091 section 6.3 triggers 1-4 are observable at investigation time: unclear business
  // rules, an architecture decision, multiple modules, indeterminate legacy behaviour. If the
  // investigator saw one, the work that follows is HIGH regardless of what the frontmatter
  // implied -- the classification was made before anyone had looked at the code.
  const preEscalated = !!(inv && inv.escalationTriggers && inv.escalationTriggers.length > 0)
  const workComplexity = preEscalated ? 'HIGH' : complexity
  if (preEscalated) {
    log(`escalation trigger at investigation → raising ${complexity} to HIGH: ${inv.escalationTriggers.join('; ')}`)
  }

  // --- 3-5. IMPLEMENT → VALIDATE → DIAGNOSE, bounded ---------------------
  let attempt = 0
  let escalations = 0
  let escalated = false
  let verdict = null
  let taskStop = null
  let lastDiagnosis = null

  while (attempt <= maxRetries) {
    const isRetry = attempt > 0

    // --- IMPLEMENT ------------------------------------------------------
    phase('Implement')
    const implModel = routeModel('implement', workComplexity, risk, escalated)
    log(`${task} attempt ${attempt + 1}/${maxRetries + 1} — implement → ${implModel}${escalated ? ' (ESCALATED)' : ''}`)

    const impl = await agent(`${PREAMBLE}
${cfgLine}

Implement EXACTLY task ${task}. Nothing adjacent, however tempting.

Task specification (binding): docs/kb/execution/tasks/${task}.md
Its "## Acceptance Criteria", "## Testing Requirements" and "## Scope" sections govern.
Treat its "Current Implementation" section as a HYPOTHESIS -- re-verify line numbers against
the code before editing or citing them; task files go stale as other tasks land.

Investigation result (already done -- do not repeat it):
${j(inv || { summary: 'investigation unavailable; verify what you need directly from source' })}
${isRetry ? `
THIS IS RETRY ${attempt} OF ${maxRetries}. The previous attempt FAILED validation.
Validator verdict:
${j(verdict)}
Diagnosis:
${j(lastDiagnosis)}
Apply the diagnosed fix. Do NOT repeat the previous approach. Do NOT weaken, widen or delete
an acceptance criterion or a failing check to make it pass -- if a criterion is wrong, that is
a finding for a human, not an edit.
` : ''}
Build what you changed using ONLY commands from
docs/kb/execution/prompt-template.md section "Verified repository commands". Prefer per-project
"dotnet build <path>.csproj". "dotnet test" finds NO test project until M0-12-01 creates one --
a green run there executed nothing, so do not use it.

Commit single-scope on branch migration/${task}-<slug> with message "${task}: <imperative
summary>". Do NOT merge and do NOT push.

Report honestly: a criterion you did not meet must appear in notDone. The validator checks
independently, and an overclaim costs the loop a full retry cycle to discover. If you cannot
proceed at all, set blocked=true with the reason.`,
      { label: `implement:${task}${isRetry ? `:retry${attempt}` : ''}`, phase: 'Implement',
        agentType: 'migration-implementer', model: implModel, effort: EFFORT.implement, schema: IMPLEMENTATION })

    if (!impl) { taskStop = 'implementer returned no result'; break }
    if (impl.blocked) { taskStop = `implementer blocked: ${impl.blockedReason || 'unspecified'}`; break }

    // --- VALIDATE -------------------------------------------------------
    phase('Validate')
    const valModel = routeModel('validate', workComplexity, risk, escalated)
    log(`${task} attempt ${attempt + 1} — validate → ${valModel}`)

    verdict = await agent(`${PREAMBLE}

Independently validate task ${task}. You are NOT the implementer and you must not assume it
was correct. Start from the acceptance criteria and the repository, never from the
implementer's account of its own work.

Task specification: docs/kb/execution/tasks/${task}.md
Check every criterion in its "## Acceptance Criteria" section, one at a time, quoting the
criterion and stating the evidence. A criterion you did not check is NOT met.

Re-run the required verification commands yourself -- a command someone else reported passing
is a claim, not a result. Use only commands from docs/kb/execution/prompt-template.md section
"Verified repository commands". "dotnet test" finds no test project until M0-12-01 exists; if
you run it and it reports success it ran NOTHING, and reporting that as a pass is a serious
failure. Read the actual diff (git diff, git status --porcelain).

For context only, the implementer reported:
${j({ implemented: impl.implemented, filesChanged: impl.filesChanged, notDone: impl.notDone, assumptions: impl.assumptions })}
Verify that independently. Where it claims a command result, re-run the command.

Check for: regressions, missing business rules the legacy Blazor code enforces, and whether
the diff strayed outside the task's authorised surface (schema changed without authorisation,
business logic reimplemented in TypeScript, unrelated modules touched, Blazor Server broken).

A check you could not run is "not checkable", never a pass -- name what would verify it.

PASS only if every criterion is objectively met against evidence you observed, the required
validation actually ran, no regression was found and the diff stayed in scope. Anything less
is FAIL, including "essentially fine".

On FAIL, choose failureCategory deliberately: it is load-bearing. "business-rule" and
"architecture" escalate to a stronger model instead of being retried, because they are never
just a bug. Then APPEND the attempt to docs/kb/execution/failure-log.md (KB-092) before
returning -- an attempt that is only in memory is lost if this run dies.`,
      { label: `validate:${task}:attempt${attempt + 1}`, phase: 'Validate',
        agentType: 'migration-validator', model: valModel, effort: EFFORT.validate, schema: VERDICT })

    if (!verdict) { taskStop = 'validator returned no result — validation could not be performed reliably'; break }
    if (verdict.verdict === 'PASS') break

    log(`${task} attempt ${attempt + 1} FAILED — ${verdict.failureCategory}: ${verdict.whatFailed || ''}`)

    if (attempt >= maxRetries) {
      taskStop = `retry budget exhausted after ${attempt + 1} attempts — last failure (${verdict.failureCategory}): ${verdict.whatFailed || 'unspecified'}`
      break
    }

    // --- DIAGNOSE -------------------------------------------------------
    phase('Diagnose')
    const hardCategory = verdict.failureCategory === 'business-rule' || verdict.failureCategory === 'architecture'
    const failureCount = attempt + 1
    const shouldEscalate = hardCategory || failureCount >= cfg.escalateAfterFailures
    const diagModel = routeModel('diagnose', workComplexity, risk, escalated || shouldEscalate)
    log(`${task} — diagnose → ${diagModel}${shouldEscalate ? ' (escalating)' : ''}`)

    const diag = await agent(`${PREAMBLE}
${cfgLine}

Validation of task ${task} FAILED on attempt ${failureCount}. Diagnose it.

Validator verdict:
${j(verdict)}

FIRST read docs/kb/execution/failure-log.md (KB-092) for THIS task's previous attempts. If the
fix you are about to try is already recorded there as tried, it is not a retry -- it is a loop.
Set triedBefore=true and disposition="escalate".

Reproduce the failure yourself before concluding anything. A failure you have not observed is
a story, not a bug.

Classify the root cause honestly:
- implementation-error -> fix it, this is the normal case
- missing-dependency   -> usually NOT fixable here; building the prerequisite inline is scope
                          creep and produces an unreviewable diff. disposition="blocked"
- business-rule        -> escalate. Never adjust a rule to make a check pass
- architecture         -> escalate. Needs a decision, not a patch
- legacy-behaviour     -> escalate unless the correct behaviour is confirmable from source at
                          file:line right now
- environment          -> not a code defect. disposition="blocked"; never work around it by
                          weakening the check
- unknown              -> say so. "unknown" is a legitimate answer and escalates; it is far
                          more useful than a confident guess that sends the loop down a wrong
                          path for another two attempts

Fix ONLY when the cause is a simple implementation error, the fix stays inside the task's
authorised scope, and you can validate it with a verified command. Do not fix when it would
change a business rule, change the schema, alter an architecture decision, touch a file the
task does not authorise, or weaken a failing check.

Making the validator pass is not the goal -- the ERP behaving correctly is.

Append your diagnosis to docs/kb/execution/failure-log.md before returning.`,
      { label: `diagnose:${task}:attempt${failureCount}`, phase: 'Diagnose',
        agentType: 'migration-debugger', model: diagModel, effort: EFFORT.diagnose, schema: DIAGNOSIS })

    if (!diag) { taskStop = 'debugger returned no result'; break }
    lastDiagnosis = diag

    if (diag.disposition === 'blocked') {
      taskStop = `blocked during diagnosis (${diag.causeClass}): ${diag.rootCause}`
      break
    }

    const mustEscalate = diag.disposition === 'escalate' || diag.triedBefore ||
      diag.causeClass === 'unknown' || hardCategory || failureCount >= cfg.escalateAfterFailures

    if (mustEscalate) {
      if (escalations >= cfg.maxEscalations) {
        taskStop = `escalation budget exhausted (${cfg.maxEscalations}) — ${diag.causeClass}: ${diag.rootCause}`
        break
      }
      escalations += 1
      escalated = true
      log(`${task} ESCALATED (${escalations}/${cfg.maxEscalations}) — ${diag.escalateReason || diag.causeClass}`)
    }

    attempt += 1
  }

  // --- 6. COMPLETE or HALT ----------------------------------------------
  const passed = !taskStop && verdict && verdict.verdict === 'PASS'
  phase(passed ? 'Complete' : 'Halt')

  const outcome = await agent(`${PREAMBLE}

Record the outcome of task ${task} in the repository. This is the close-out; do NOT implement
anything and do NOT start another task.

Outcome: ${passed ? 'validation PASSED' : `STOPPED — ${taskStop}`}
Final validator verdict:
${j(verdict || { verdict: 'none', note: 'validation did not complete' })}
${lastDiagnosis ? `Last diagnosis:\n${j(lastDiagnosis)}\n` : ''}
Attempts used: ${attempt + 1} of ${maxRetries + 1}. Escalations: ${escalations}.
Investigation summary:
${j(inv ? { summary: inv.summary, reused: inv.reusedInvestigations, unknowns: inv.unknowns } : {})}

Do all of this, per docs/kb/execution/review-templates.md (KB-084) "Session close-out":

1. Append "## Execution Record (<today>)" to docs/kb/execution/tasks/${task}.md -- what was
   actually done, what the commands actually printed, what differed from the plan and why.
   Update its frontmatter status and last_verified.
2. Update docs/kb/execution/task-tracker.md (KB-081) with the honest status.
   ${passed
     ? `Set REVIEW ("Needs Review") -- NOT Completed -- unless this task's Completion Conditions
   contain no human step at all. This project requires integration before Completed
   (KB-088 "Who may set COMPLETED"). Never record Completed for work a human still has to do.`
     : `Set BLOCKED with the reason above and a NAMED OWNER who can unblock it. Distinguish
   blocked-on-a-task from blocked-on-a-human.`}
3. Record discoveries where they belong (KB-088 section 4): investigation registry including
   negative results, business rules with file:line evidence, risks, open questions. Raise a
   question rather than guessing. Update architecture/decision documents only if observed
   behaviour actually contradicts them. Do not update a document that did not change --
   documentation noise costs every future session real context.
4. Update docs/kb/execution/runner-state.md (KB-093): Status ${passed ? 'RUNNING or STOPPED' : 'BLOCKED'},
   last validation result, attempts used, and ${passed ? 'the next task' : 'the exact stop reason'}.
${passed ? `5. Select the next dependency-ready task using the rule in
   docs/kb/execution/dependency-graph.md against the tracker, and REWRITE
   docs/kb/execution/current-task.md for it (replace it, do not append). Carry across anything
   this task discovered that the next one needs. Do NOT implement it. Return its id in
   nextTaskId, or "" if nothing is ready.`
  : `5. Leave docs/kb/execution/current-task.md pointing at ${task} with its Run State showing
   the failure, so a human or a later run resumes rather than restarts. Return nextTaskId="".`}

Nothing important may exist only in this run's memory. If a finding cannot be found in the
repository afterwards, it is lost.`,
    { label: passed ? `complete:${task}` : `halt:${task}`, phase: passed ? 'Complete' : 'Halt',
      model: 'sonnet', effort: 'medium', schema: COMPLETION })

  completed.push({
    task,
    complexity,
    risk,
    attempts: attempt + 1,
    escalations,
    verdict: verdict ? verdict.verdict : 'none',
    status: outcome ? outcome.status : 'unknown',
    stop: taskStop || null,
    next: outcome ? outcome.nextTaskId : '',
  })

  if (taskStop) { stopReason = `${task}: ${taskStop}`; break }

  tasksDone += 1
  log(`${task} → ${outcome ? outcome.status : 'unknown'} after ${attempt + 1} attempt(s)`)

  if (!outcome || !outcome.nextTaskId) {
    stopReason = 'no dependency-ready task remains'
    break
  }
  if (tasksDone >= cfg.maxTasks) {
    stopReason = `task budget reached (maxTasks=${cfg.maxTasks}); next ready task is ${outcome.nextTaskId}`
  }
}

// --- record the stop reason so the next run starts from disk ---------------
if (stopReason) {
  phase('Halt')
  log(`STOP — ${stopReason}`)
  await agent(`${PREAMBLE}

The autonomous migration runner has stopped. Record it and change nothing else.

Stop reason: ${stopReason}
Tasks processed this run: ${j(completed)}

Update docs/kb/execution/runner-state.md (KB-093) ONLY:
- Status: BLOCKED if a task is blocked or a retry budget was exhausted, otherwise STOPPED
- the exact stop reason above, verbatim
- the current task, its last validation result, attempts used and escalations
- the next dependency-ready task if one is known
- who can unblock it, when a human is required

Do not modify any other file. Do not start a task. Do not re-open a completed task.`,
    { label: 'record-stop', phase: 'Halt', model: 'haiku', effort: 'low' })
}

// tasksCompleted counts tasks that PASSED validation and were recorded.
// tasksAttempted includes any task that was opened and then blocked. Reporting
// one number for both would let a blocked run read as progress.
return {
  stopReason: stopReason || 'clean exit',
  tasksCompleted: tasksDone,
  tasksAttempted: completed.length,
  blocked: completed.filter((r) => r.stop).map((r) => ({ task: r.task, reason: r.stop })),
  results: completed,
  config: { maxTasks: cfg.maxTasks, maxRetries: cfg.maxRetries, maxEscalations: cfg.maxEscalations },
}
