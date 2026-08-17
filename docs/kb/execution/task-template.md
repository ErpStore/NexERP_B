---
doc_id: KB-090
title: Task File Template
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-083, KB-088, KB-091]
---

# Task File Template

The skeleton for every new `tasks/<TASK-ID>.md`. It replaces the older pattern in which each
task file carried a ~150-line self-contained execution prompt restating the project
objective, architecture, source-of-truth rules and constraints.

**That preamble now lives once, in [`CLAUDE.md`](../../../CLAUDE.md).** A task file states
only what is *true of this task*.

Binding generation rules — no placeholders, never invent a path, cite `file:line`, classify
every claim `Confirmed`/`Inferred`/`Unknown`, regenerate rather than patch — remain
[KB-083 § Generation rules](prompt-template.md#generation-rules).

**Reference the knowledge base; never duplicate it.** A task file that restates a business
rule, an ADR or an architecture section instead of linking to it will go stale silently, and
then it lies.

**A task file contains no history of previous tasks.** What earlier tasks did is in their own
files and in KB-081.

---

## Skeleton

````markdown
---
doc_id: TASK-<ID>
title: <short title>
module: execution
task_id: <ID>
milestone: <M#>
task_type: Backend | Frontend | Database | Security | Testing | DevOps | Investigation | Migration | Documentation | Architecture | Product Decision
priority: P0 | P1 | P2
status: PLANNED
estimate: <n d>
depends_on: [<task ids>]
source_files: [<repo-relative paths this task reads or writes>]
business_rules: [<BR ids, or empty>]
confidence: n/a
last_verified: YYYY-MM-DD
# Optional — omit unless the derivation in KB-091 gets this task wrong.
# complexity: LOW | MEDIUM | HIGH
# risk: LOW | MEDIUM | HIGH
# preferred_model: haiku | sonnet | opus
# escalation_model: opus
# max_retries: 2
---

# <ID> — <title>

| Field | Value |
|---|---|
| Task ID | |
| Milestone | |
| Type | |
| Priority | |
| Status | |
| Estimate | |
| Gate | |

## Objective

One paragraph. What is true after this task that is not true now.

## Scope

What this task changes. Be specific enough that a reviewer can reject an out-of-scope diff.

## Out of Scope

What a reasonable engineer might think belongs here but does not — and which task owns it
instead. This section prevents scope creep more effectively than any instruction.

## Dependencies

| Dependency | Class | Note |
|---|---|---|

Classes: Hard · Soft · Information · Testing · Deployment
([KB-082](dependency-graph.md)). Name downstream consumers too — a task that blocks others
should say so.

## Relevant Documentation

| doc_id | Path | Why |
|---|---|---|

Only what is genuinely needed. Every entry costs the executing session context, so an
unnecessary row is a real cost. Link to a *section*, not a whole large document.

## Relevant Existing Code

Verified paths only, each with why it matters. Mark anything not yet existing
`TO BE CREATED`. State what must **not** change.

## Business Rules

`BR-xxx-nnn` + statement + `file:line` evidence — or **"None — this task does not touch
business behaviour."**

Never invent a rule. If the rule is not yet extracted, this task's prerequisite is the
investigation that extracts it.

## Implementation Requirements

Numbered steps, each independently checkable. Where a step's outcome is unknown in advance
(a measurement, an investigation), say what will be recorded rather than predicting a result.

## Acceptance Criteria

- [ ] Objectively verifiable statements only.

"Works correctly" is not a criterion. "`GET /api/v1/currencies` returns 403 for a user
without `CURRENCY_VIEW`, asserted by a test" is.

## Testing Requirements

What must be tested, and the **exact commands** — drawn only from
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
If a required command is not yet verified, that is a finding, not an assumption.

Testing belongs to the task that introduces the behaviour. Never defer it.

## Documentation Updates

| Document | Change | Frontmatter |
|---|---|---|

Only documents that will genuinely change. Claim a `doc_id` from
[INDEX.md § doc_id allocation](../INDEX.md#doc_id-allocation) and `grep` for it first.

## Completion Conditions

What must be true for this task to leave `REVIEW` for `COMPLETED` — explicitly including any
step only a human can perform (a merge, a rotation, a DBA drill, a product decision), with
the owner named. If such a step exists, an execution session's honest end state is `REVIEW`
or `BLOCKED`, never `COMPLETED`.

## Git Strategy

- **Branch:** `migration/<ID>-<slug>`
- **Commit:** `<ID>: <imperative summary>`
- **Rollback:** <how this task is reverted>
- Do not merge. Do not push. Leave the branch for review.
````

---

## The optional autonomous-execution keys

The five commented keys in the skeleton exist for the autonomous runner
([`autonomous-runner.md`](autonomous-runner.md), KB-091). **Leave them out.**

KB-091 §4 derives `complexity` and `risk` from `task_type`, `estimate`, `depends_on`,
`business_rules` and `source_files` — metadata every task file already carries. That is why
the 105 existing files needed no edit when the runner was introduced, and why a new file
needs none either.

Add a key only to **correct** a derivation that is wrong for this specific task — a
Documentation task that is genuinely hard, a Backend task that is genuinely trivial — and say
in the *Objective* why. An override with no stated reason is indistinguishable from a
leftover, and the next person to read it cannot tell whether to trust it.

**Acceptance criteria and validation requirements stay as sections**, never as frontmatter.
`## Acceptance Criteria` and `## Testing Requirements` are already binding
([KB-088 §5](workflow.md#5-validation)); copying them into YAML would create a second copy
that drifts, which is the failure mode this knowledge base exists to prevent.

## Sections deliberately not in this template

| Removed | Why | Where it lives now |
|---|---|---|
| Role / project objective / current architecture preamble | Identical in all 105 task files | [`CLAUDE.md`](../../../CLAUDE.md) |
| Source-of-truth authority order | Invariant | `CLAUDE.md`, [KB-002](../source-of-truth-rules.md) |
| Anti-repetition clause | Invariant | `CLAUDE.md`, [KB-088 §2](workflow.md) |
| Standing constraints list | Invariant | `CLAUDE.md` |
| Execution procedure | Invariant | [KB-088 §2](workflow.md#2-starting-a-session) |
| 15-item final report format | Invariant | [KB-084](review-templates.md) |
| "Fresh-Session Execution Prompt" block | Superseded — the session prompt is now three lines | [KB-083](prompt-template.md) |

Roughly 150 lines per task file, across ~105 files and growing, all of it read into context
every time a task is opened. Removing it is the single largest context saving available in
this workflow.

## Existing task files

The 105 files already written are **not** being rewritten to this template — that is churn
against files that are otherwise accurate, and several already carry Execution Records worth
preserving. Their trailing *Fresh-Session Execution Prompt* blocks are simply **obsolete**:
`CLAUDE.md` plus `current-task.md` now supply everything those blocks restated.

When you open an existing task file, read the specification sections and **skip the prompt
block**. If you are regenerating a task file for an unrelated reason, regenerate it to this
template and drop the block then.
