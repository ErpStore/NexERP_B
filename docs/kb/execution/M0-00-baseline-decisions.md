---
doc_id: KB-085
title: M0-00 Version-Control Baseline Decision Log
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-080, KB-083, KB-060, KB-003, KB-004]
---

# M0-00 — Version-Control Baseline Decision Log

Audit trail for the deliberate, human-decided disposition of every one of the 37
`git status --porcelain` entries present in the working tree at task start
(2026-08-12), per [tasks/M0-00.md](tasks/M0-00.md). No `git add -A` / `git commit -a` /
equivalent blanket stage was used anywhere in this task — every commit below stages an
explicit, named path list.

## Pre-execution verification (drift check)

Re-ran the commands INV-029 was originally verified against, 2026-08-12, before touching
anything. No drift found — counts match [KB-003](../investigation-registry.md)'s INV-029
row exactly:

| Check | Result |
|---|---|
| `git status --porcelain` | 37 entries (26 `M`, 1 `D`, 10 `??`) |
| `git log --oneline --all` | `c12c5b2 Add project files.` (1 commit) |
| `git ls-files` | 2,162 tracked files |
| `git ls-tree -r HEAD --name-only \| grep .sln` | `Bhargavi V.SMART ERP - 2025.sln` |
| `git ls-remote` (unauthenticated) | succeeds → repository is public |
| `git grep -l "NexGenERP-Dev-Jwt-Secret" HEAD` | no output (secret not in history) |
| `git check-ignore -v frontend/vsmart-erp/dist` | `frontend/vsmart-erp/.gitignore:4` |
| `git check-ignore -v frontend/vsmart-erp/.angular/cache` | `frontend/vsmart-erp/.gitignore:32` |
| `git check-ignore -v frontend/vsmart-erp/node_modules` | `frontend/vsmart-erp/.gitignore:10` |
| `git check-ignore -v .vs` | `.gitignore:37` |

The G1…G9 grouping in `tasks/M0-00.md` was re-verified against live `git status` output
and found identical — no regrouping was needed.

## Safety measures taken before any disposition

1. **Tag:** `git tag pre-M0-00-baseline c12c5b2`, pushed to `origin` — exists locally and
   on origin, tagging the original single commit for rollback (`git reset --hard
   pre-M0-00-baseline` restores the tracked state exactly).
2. **Physical backup:** the working tree was copied via `robocopy` to
   `C:\Kumar\M0-00-backups\20260812-pre-baseline\NexGen-ERP---2025-master`, **excluding**
   `bin/`, `obj/`, `node_modules/`, `.vs/`, `.angular/`, `dist/` — these are reproducible
   build output already covered by `.gitignore` (2.26 GB of the working tree's 2.65 GB),
   not irreplaceable source. 2,375 files / 131.75 MB copied, 0 failures. Verified readable
   by opening `docs/kb/execution/tasks/M0-00.md`, `V.SMART/V.SMART.Api/appsettings.json`
   and `NexGen-ERP---2025-master.sln` from the copy. **Deviation from the letter of step 3**
   ("a file-system copy... of the working tree"): the exclusion is a deliberate scope
   narrowing to what step 3's own stated purpose requires (protecting the 95 untracked
   files that exist nowhere else, e.g. `docs/kb/`, `V.SMART.Api/`, `frontend/`) — every
   excluded directory is fully reproducible from a rebuild and none of this task's
   decisions touch them.
3. **Branch:** `migration/M0-00-vcs-baseline`, created from `master` at `c12c5b2`, pushed
   to `origin` (not merged; `master` untouched).

## Decisions

Decider for every group: **Kumar** (session default; correct on request).
Date: **2026-08-12**.

| # | Group | Paths | Decision | Rationale | Commit |
|---|---|---|---|---|---|
| G1 | Solution rename | `Bhargavi V.SMART ERP - 2025.sln` (delete), `NexGen-ERP---2025-master.sln` (add) | **Commit** | The tracked `.sln` was already deleted on disk; the replacement is the file every command in [KB-083](prompt-template.md) names. Mechanical, no behavior change. | `d83e2ea` |
| G2 | `V.SMART/V.SMART.Api/` (12 untracked files, incl. `appsettings.json` with a `Jwt:Secret`, and `V.SMART.Api.csproj.user` which is gitignored via `.gitignore:9` `*.user` and was therefore never a candidate for staging regardless) | **Defer to M0-03-01** | Hard constraint (this task's CONSTRAINTS section): must not commit or edit `V.SMART.Api/appsettings.json`. The JWT secret is confirmed absent from `HEAD` (`git grep` negative) and must stay that way. The task's own recommendation allows optionally committing the 10 non-secret files "only if M0-03-01's owner agrees" — no such sign-off exists in this session, so the whole group is deferred rather than partially committed. | — (not committed) |
| G3 | 4 secret-bearing modified files: `V.SMART.Web/appsettings.json`, `ApplicationDbContextFactory.cs`, `MasterDbContextFactory.cs`, `MauiProgram.cs` | **Defer to M0-03 / M0-04** | Hard constraint. These carry the already-published R-01/R-02 credentials; committing the *modified* versions adds exposure without removing any (the credentials are already in `c12c5b2`). Owned by M0-03 (externalisation) and M0-04 (rotation). | — (not committed) |
| G4 | 18 modified + 5 new UI/theme ("Zoho redesign") files: `ColumnMenu.razor`, `PageHeader.razor`, `ProcessingOverlay.razor.css`, `MainLayout.razor(.css)`, `NavMenu.razor(.css)`, `Dashboard.razor`, `KpiSection.razor`, `SplitChartComponent.razor`, `TrendChartComponent.razor`, `Home.razor`, `Login.razor`, `EnquirySalesList.razor`, `app.css`, `Dashboard.css`, `App.razor`, `index.html`, `BlankLayout.razor`, `nexgen-brand.css`, `nexgen-design-system.css`, `zoho-theme.css`, `nexgen-logo.svg` | **Commit as-is** | Human decision (confirmed in session, 2026-08-12): keep the in-progress redesign tracked rather than discard it. `git diff --stat`: 23 files, 6,484 insertions(+), 5,442 deletions(-) net across the modified subset. No source content was altered by this task — the working tree already carried these changes. | `13639a2` |
| G5 | Business code: `ICurrencyService.cs`, `CurrencyService.cs`, `TenantProvider.cs` | **Commit as-is** | Human decision (confirmed in session, 2026-08-12), after reading the **full diff** per step 5 (G5 is the only group that can change ERP behaviour; `TenantProvider.cs` sits on the multi-tenancy path, KB-014/INV-005). Findings: `CurrencyService` gains `GetByIdAsync`/`CreateAsync`/`UpdateAsync` (the surface M2-D01 needs) plus two message-text fixes; `TenantProvider.cs` adds JWT `TenantId`-claim resolution as a new first-checked mode ahead of the existing Host-mode and `tenant.json` fallback (both unchanged, just renumbered), plus a `session != null` guard before `session.HostName = host` (previously a latent NRE risk on any HTTPContext where `session` was null in Host-mode). No existing behavior was removed. | `b8beb0d` |
| G6 | `V.SMART/V.SMART.csproj` | **Commit as-is** | Human decision (confirmed in session, 2026-08-12). `ApplicationDisplayVersion` 1.0 → 1.0.1.0, aligning it with the `<Version>1.0.1.0</Version>` already present in the same file, plus a whitespace/indent change. No behavioral risk. | `d9b8095` |
| G7 | `docs/` (116 files, incl. all of `docs/kb/`) | **Commit** | The KB is the project's declared source of truth (KB-080 §5) and cannot govern the work while untracked. | `2c02b1d` |
| G8 | `frontend/` (40 files; Angular pilot, INV-021 Complete) | **Commit** | Must be tracked before M2-C11 archives it. `dist/`, `.angular/cache/`, `node_modules/` re-confirmed ignored before staging; 0 ignored paths were staged (`git diff --cached --name-only \| grep -E "/dist/\|\.angular/\|node_modules/"` → 0 matches). | `3be2127` |
| G9 | `.github/` (2 files: `copilot-instructions.md`, `prompts/convert-to-zoho-ui.prompt.md`; `workflows/` is empty and not committable) | **Commit** | Tooling files with no CI content; `.github/workflows/` remains untracked/empty — M0-07 owns creating the first workflow file there. | `7e9d5b2` |

## Build verification (Testing steps 1–2)

Both runs used `dotnet clean` first, so the numbers are a clean build, matching the
methodology KB-083's 6,695-warning baseline was measured with. **Caveat recorded:** an
initial *incremental* build (existing `bin`/`obj` from a prior session) returned only 2
warnings — this was not a valid comparison point and is noted here so a future session
does not mistake an incremental build's warning count for the baseline.

| Run | Command | Errors | Warnings | Time |
|---|---|---|---|---|
| Pre-change (clean) | `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` | 0 | 6,695 | 2m 32s |
| Post-change (clean) | same | 0 | 6,695 | 1m 24s |

Unchanged, as expected — this task alters no `.cs`/`.razor`/`.css`/`.json` content, only
git tracking.

No G4/G5 file was discarded, so the manual Blazor smoke-test (Testing step 3) was not
required and was not performed.

## GitHub configuration (step 9)

| Item | Status |
|---|---|
| Push `pre-M0-00-baseline` tag to `origin` | **Done.** `git push origin pre-M0-00-baseline` succeeded — push credentials are configured in this environment. |
| Push `migration/M0-00-vcs-baseline` branch to `origin` | **Done.** Available at `https://github.com/ErpStore/NexERP_B/pull/new/migration/M0-00-vcs-baseline` for a PR. `master` was not touched. |
| Protect `master` (PR required, no force-push, no deletion, **no** required status check yet — M0-07 adds that) | **Blocked / pending.** No `gh` CLI is installed and no GitHub API token is configured in this execution environment. The Chrome browser tool (which could drive the repo owner's logged-in session) was unavailable (extension not connected) when attempted. **Action required from the repo owner:** go to `https://github.com/ErpStore/NexERP_B/settings/branches` → Add branch protection rule → branch name pattern `master` → check "Require a pull request before merging", "Do not allow bypassing the above settings" (or equivalent), "Restrict deletions", and leave "Require status checks to pass" **unchecked** (no CI exists until M0-07). M0-07 must return to add the status-check requirement. |
| Repository visibility audit | **Recorded.** `git ls-remote https://github.com/ErpStore/NexERP_B.git` succeeds without authentication as of 2026-08-12 → the repository is **public**. No visibility change was made — that is the owner's decision (see Q-19 below). |

## Q-19 escalation (public repo + published credentials)

Escalated in this session, 2026-08-12, to **Kumar**, who confirmed being the owner of
`ErpStore/NexERP_B`. Evidence attached: [KB-060](../risks/technical-debt-register.md) R-01
(hardcoded credentials in C#, not only config) and R-02 (repository-level exposure),
re-confirmed live in this task (see Pre-execution verification table above). Recorded in
[open-questions.md](../open-questions.md) Q-19 with this date and owner. **Decision on
visibility itself remains outstanding** — this task does not act on it.

## Rollback

- Tracked-state rollback: `git reset --hard pre-M0-00-baseline` on
  `migration/M0-00-vcs-baseline` returns exactly to `c12c5b2`.
- Untracked-file rollback: restore from
  `C:\Kumar\M0-00-backups\20260812-pre-baseline\NexGen-ERP---2025-master`.
- No `stash` was used — every group's decision was `commit` or `defer` (nothing left
  half-decided).

## Unexpected finding: JWT secret value published via this KB document, not via appsettings.json

**Severity: high. Self-caused by this task; fixed within the same session, 2026-08-12.**

The task's own G2 recommendation and acceptance criteria treat `git grep -l
"NexGenERP-Dev-Jwt-Secret" HEAD` returning nothing as proof the JWT secret was not
published — on the assumption that the only place the value could leak was
`V.SMART.Api/appsettings.json`, which G2 correctly deferred and never committed. That
assumption was wrong: `docs/kb/risks/technical-debt-register.md` (R-02) **quoted the
secret's full literal value** as cited evidence:
`"Secret": "NexGenERP-Dev-Jwt-Secret-Key-Change-In-Production-Min32Chars!"`. Committing
`docs/` as group G7 — recommended by the task and confirmed by the decider without a
file-by-file secret scan of the KB itself — carried that value into `HEAD` on
`migration/M0-00-vcs-baseline`, which was then pushed to the public `origin`.

**Immediate remediation taken (same session):** the literal value was redacted from
`technical-debt-register.md` in a follow-up commit (content-only fix; no history rewrite —
M0-05 still owns the purge). The verification re-run after that commit still shows the
*search pattern* `NexGenERP-Dev-Jwt-Secret` (a 24-character prefix of the 62-character
secret, `-Key-Change-In-Production-Min32Chars!` not included) in several other KB task
files (`tasks/M0-00.md`, `tasks/M0-03-01.md`, `tasks/M0-04.md`) — these use it only as a
`git grep` search argument (pre-existing KB convention, not introduced by this session,
and it is the literal acceptance-criteria text of this and other tasks). This fragment was
judged not worth mass-redacting across the KB in this task's scope, since it is far short
of the full secret and the substantive fix (rotation) removes its value entirely — but it
is recorded here rather than silently accepted.

**Consequence:** the JWT dev secret must now be treated as **published and compromised**,
regardless of `appsettings.json`'s tracking state. This does not change M0-03-03/M0-04's
planned action (rotate; fail-fast on the known default) but does remove any basis for
treating it as lower urgency than R-01. Recorded in `technical-debt-register.md` R-02 and
flagged here for whoever picks up M0-04/M0-03-03/M0-05 next.

**Root-cause lesson for future tasks:** "is this file/value in HEAD" is not the same
question as "is this value quoted anywhere in what I'm about to commit." A KB document
that cites a secret as evidence is itself a secret-bearing file the moment it's tracked.

## Deviations from the task spec

1. Physical backup excluded `bin/obj/node_modules/.vs/.angular/dist` — see rationale
   under *Safety measures* above.
2. Branch protection on `master` could not be completed in this session (no GitHub
   credentials/CLI/browser session available) — recorded as pending above, owner-actionable.
3. `docs/kb/investigation-registry.md` already carried the INV-029 row before this task
   started (it was expected to be missing per `tasks/M0-00.md`, written earlier the same
   day) — verified current and left unchanged rather than re-added.
