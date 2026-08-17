---
doc_id: KB-092
title: Validation Failure and Diagnosis Log
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-16
dependencies: [KB-081, KB-089, KB-091]
---

# Validation Failure and Diagnosis Log

**Append-only.** Every validation that returned `FAIL`, every diagnosis of that failure, and
every safety stop. One entry per attempt.

This exists so that the autonomous runner ([`autonomous-runner.md`](autonomous-runner.md),
KB-091) can answer three questions after the conversation that produced them is gone:

1. **Has this been tried?** A retry that repeats a recorded fix is a loop, not a retry.
2. **How many attempts has this task had?** The retry budget is meaningless if the count
   lives in a chat window.
3. **Why did it stop?** A `BLOCKED` task with no recorded reason costs the next session the
   whole diagnosis again.

## Rules

- **Append; never edit or delete an entry.** A wrong diagnosis that was later corrected is
  the most useful thing in this file — it stops the next session reaching for it again.
- Write the entry **before** the next attempt starts. An attempt that is only in memory is
  lost if the session dies.
- Cite `file:line` for evidence, per
  [`source-of-truth-rules.md`](../source-of-truth-rules.md) (KB-002). "The build failed" is
  not evidence; the error id and the failing line are.
- Record **safety stops** here too, not only failures. "Stopped: needs DBA access" is a
  result.
- A failure that reveals a business rule, a risk, or an open question is *also* recorded in
  its proper home (KB-030, KB-060, KB-004). This log records the **attempt**; those record
  the **knowledge**.
- When a task finally passes, the entries stay. The task file's `## Execution Record`
  summarises the outcome and links here for the detail.

## Entry format

````markdown
### <TASK-ID> · attempt <n> · <YYYY-MM-DD>

| Field | Value |
|---|---|
| Runner state | FAILED / DIAGNOSING / ESCALATED / BLOCKED |
| Model in use | haiku / sonnet / opus |
| Validator verdict | FAIL |
| Failure category | build / test / acceptance-criterion / regression / business-rule / architecture / environment |

**What failed** — the acceptance criterion or command, quoted, with what it actually printed.

**Root cause** — one sentence, or `unknown` (which is itself an escalation trigger, KB-091 §6.3).

**Evidence** — `path:line`, command output, or the legacy behaviour it contradicts.

**Disposition** — `fixed` / `retry` / `escalate` / `blocked`, and why.

**Next attempt routed to** — model, and which KB-091 §6.3 trigger applied, if any.
````

The `Failure category` is not decoration: `business-rule` and `architecture` escalate
immediately rather than being retried at the same model, because they are never "just a bug"
([KB-091 §6.3](autonomous-runner.md#63-escalation-triggers)).

---

## Log

*Entries below, newest last.*

The three tasks completed before this framework existed — **M0-00**, **M0-01-01** and
**M0-01-02** — were executed under the human-in-the-loop workflow and are recorded in their
own task files' `## Execution Record` sections and in
[`task-tracker.md`](task-tracker.md) (KB-081). They are **not** backfilled here: this log
records attempts made by the runner, and inventing retrospective entries for work it did not
do would make it lie.

---

### M0-03-01 · attempt 1 · 2026-08-17

| Field | Value |
|---|---|
| Runner state | FAILED |
| Model in use | opus (implementation), opus (validation) |
| Validator verdict | FAIL |
| Failure category | architecture |

**What failed** — acceptance criterion 8 of
[`tasks/M0-03-01.md:364-365`](tasks/M0-03-01.md): *"`V.SMART.Api` starts successfully with
configuration supplied from user-secrets, and fails with the existing explicit message when
`Jwt:Secret` is removed."* The first half holds; the second does not. Re-run independently by
the validator with the user-secrets provider excluded (`ASPNETCORE_ENVIRONMENT=Production`,
`dotnet run --project V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-build --no-launch-profile
--urls http://localhost:5189`), which printed:

```
Unhandled exception. System.ArgumentException: IDX10703: Cannot create a
'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey', key length is zero.
   at Microsoft.IdentityModel.Tokens.SymmetricSecurityKey..ctor(Byte[] key)
   at Program.<Main>$(String[] args) in ...\V.SMART\V.SMART.Api\Program.cs:line 58
```

not `InvalidOperationException: Jwt:Secret is missing from configuration.`

**Root cause** — the task's own *Target Implementation* mandates `Jwt:Secret` be `""` in
`V.SMART/V.SMART.Api/appsettings.json:12`, and `""` is not `null`, so the `?? throw` guard at
`V.SMART/V.SMART.Api/Program.cs:56-57` can never fire. The criterion is **unsatisfiable
alongside the same task's mandated end state**; it is a specification contradiction, not an
implementation defect. Every other criterion is objectively met.

**Evidence** — `V.SMART/V.SMART.Api/appsettings.json:12` (`"Secret": ""`);
`V.SMART/V.SMART.Api/Program.cs:56-58`; the Production-environment run output quoted above.
Positive half confirmed: the same command under `ASPNETCORE_ENVIRONMENT=Development` printed
`Now listening on: http://localhost:5188`, proving the value resolves from the user-secrets
store `a2a4232e-feff-49cc-90d8-d7c6d15f7657`.

**Disposition** — `escalate`. A same-spec retry is a guaranteed loop: the three ways out are
(a) amend the criterion, (b) omit the `Jwt:Secret` key from `appsettings.json` entirely, which
contradicts the "declare configuration *shape*" convention this task establishes
(`docs/CONFIGURATION.md`), or (c) harden the guard to reject empty/default values — which
`tasks/M0-03-01.md:198-199` and :704 explicitly reserve for **M0-03-03**. Choosing between
them is a design decision spanning two task specifications, not a bug fix.

**Next attempt routed to** — `opus`, KB-091 §6.3 trigger 2 (an architecture/design decision is
required) and trigger 7 (validator `FAIL` category `architecture`). Recommended resolution for
the orchestrator: amend M0-03-01's criterion 8 to the fail-fast property actually achievable
here ("the host refuses to start"), and let **M0-03-03** own the explicit diagnostic — the
second copy of the same null-only guard at `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:20-21`
is already recorded under R-02 in KB-060.

**Also noted, not a failure in itself** — `tasks/M0-03-01.md:57-60` asserts that
`git grep -l "<the SA password>" HEAD` "now returns **nine** files" and that "Nine is the
correct post-task expectation". Re-run by the validator on this branch it returns **eight**
(three C# files plus five KB documents); the ninth, `V.SMART/V.SMART.Web/appsettings.json`,
left `HEAD` with this task's own commit `a43e18d`. The count was evidently taken before the
commit. The claim should be corrected to eight when the task file is next touched.

---

### M0-03-01 · attempt 1 · diagnosis · 2026-08-17

*(Diagnosis pass over the validator's `FAIL` above — written by the debugger per
[KB-091 §7](autonomous-runner.md#7-persistent-state--what-is-written-where). No fix applied.)*

| Field | Value |
|---|---|
| Runner state | ESCALATED |
| Model in use | opus (diagnosis) |
| Validator verdict | FAIL |
| Failure category | architecture (confirmed — not re-classified) |

**Reproduced** — yes, independently, on branch `migration/M0-03-01-appsettings-secrets`
(HEAD `a43e18d`), against the existing build output:

```
$ ASPNETCORE_ENVIRONMENT=Production dotnet run --project V.SMART/V.SMART.Api/V.SMART.Api.csproj \
      --no-build --no-launch-profile --urls http://localhost:5191
Unhandled exception. System.ArgumentException: IDX10703: Cannot create a
'Microsoft.IdentityModel.Tokens.SymmetricSecurityKey', key length is zero.
   at Microsoft.IdentityModel.Tokens.SymmetricSecurityKey..ctor(Byte[] key)
   at Program.<Main>$(String[] args) in ...\V.SMART\V.SMART.Api\Program.cs:line 58

$ ASPNETCORE_ENVIRONMENT=Development dotnet run ... --urls http://localhost:5192
info: Microsoft.Hosting.Lifetime[14]  Now listening on: http://localhost:5192
info: Microsoft.Hosting.Lifetime[0]   Application started.
info: Microsoft.Hosting.Lifetime[0]   Hosting environment: Development
```

The positive half of criterion 8 holds; the negative half fails exactly as the validator
reported.

**Root cause** — confirmed as a *specification contradiction internal to M0-03-01*, and it is
sharper than "the guard is null-only". Three of this task's own statements cannot all be true
at once while `Program.cs` stays read-only:

| # | Statement | Where |
|---|---|---|
| 1 | `Jwt:Secret` must be `""` in `V.SMART/V.SMART.Api/appsettings.json` | `tasks/M0-03-01.md:233`, criterion at :356-357 |
| 2 | The `?? throw` guard is left exactly as it is — hardening it is **M0-03-03** | `tasks/M0-03-01.md:198-199` |
| 3 | The host must fail with `InvalidOperationException("Jwt:Secret is missing from configuration.")` when the secret is removed | criterion at :364-365 |

`""` is not `null`, so (1) + (2) make the guard at `V.SMART/V.SMART.Api/Program.cs:56-57`
unreachable and (3) unattainable. Startup instead dies one line later at
`Program.cs:58` inside `SymmetricSecurityKey`.

Decisively, the behaviour criterion 8 demands is **already M0-03-03's deliverable**:
`tasks/M0-03-03.md:163` (`Jwt:Secret` — "null, empty, or whitespace"), :214 ("Api with
`Jwt:Secret` set to `\"\"` → throws") and :286-287 (its acceptance criterion). M0-03-01
criterion 8 therefore asserts an outcome that another task in the same milestone owns and
that this task's own scope boundary forbids implementing.

**Evidence** — `V.SMART/V.SMART.Api/appsettings.json:12` (`"Secret": ""`);
`V.SMART/V.SMART.Api/Program.cs:56-58`; duplicate null-only guard at
`V.SMART/V.SMART.Api/Auth/JwtTokenService.cs:20-21`; scope reservation at
`tasks/M0-03-01.md:198-199`; ownership at `tasks/M0-03-03.md:163,214,286-287`; the two runs
quoted above.

**Why no fix was applied** — every available repair is outside this task's authorisation:

- Hardening the guard to `string.IsNullOrWhiteSpace` edits
  `V.SMART/V.SMART.Api/Program.cs`, which `current-task.md:114` lists **read only** and
  `tasks/M0-03-01.md:198-199` reserves for M0-03-03. It would also put two task scopes on one
  branch.
- Deleting the `Jwt:Secret` key from `appsettings.json` would satisfy criterion 8 but break
  criterion 3 (:356-357) and implementation step 5 (:233), and contradict the
  "declare the configuration *shape*" convention this task establishes in `docs/CONFIGURATION.md`.
- Amending criterion 8 is a task-specification change; a debugger editing the criterion that
  failed is precisely the "silently adjusted check" this workflow forbids.

No code change was made. The only file written by this pass is this log.

**Disposition** — `escalate`, agreeing with attempt 1's entry above. Note that a same-spec
retry is now a *loop* under [KB-091 §6.4](autonomous-runner.md#64-retry-rules): the fix would
have to be one of the three moves listed, all of which are already recorded here as rejected.

**Decision the orchestrator needs to take** (one of, not for the debugger to choose):

- **A** — amend M0-03-01 criterion 8 to the property actually achievable in this task's scope:
  *"the Api host refuses to start when `Jwt:Secret` is not supplied"*, and leave the explicit
  diagnostic message to M0-03-03, which already owns it. Lowest-risk; no code moves.
- **B** — pull the empty/short/default validation forward from M0-03-03 into M0-03-01. Then
  M0-03-03 loses most of its content and the two task files must be re-cut together.
- **C** — drop the `Jwt:Secret` key from `appsettings.json` and restate the declare-the-shape
  convention in `docs/CONFIGURATION.md`.

Option A is the recommendation, unchanged from attempt 1's entry, and it is now supported by
`tasks/M0-03-03.md:214` naming the `""` case explicitly as M0-03-03's own acceptance test.

**Residual risk** — criteria 1-7 and 9-11 remain objectively met and the branch is otherwise
sound, so whichever option is chosen the code on `migration/M0-03-01-appsettings-secrets`
likely needs no change. Two unrelated hygiene items stay open: (i) the eight-vs-nine count at
`tasks/M0-03-01.md:57-60`, and (ii) **two branches exist for this one task id** —
`migration/M0-03-01-appsettings-secrets` (used, carries `a43e18d`) and
`migration/M0-03-01-externalise-appsettings-secrets` (named by `current-task.md:34`). The
branch actually used does **not** match the name the task file mandates.

**Next attempt routed to** — `opus`, KB-091 §6.3 trigger 2 (an architecture/design decision is
required) and trigger 7 (validator `FAIL` category `architecture`). Not a code retry — a
specification decision first.
