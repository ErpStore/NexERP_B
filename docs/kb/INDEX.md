---
doc_id: KB-005
title: Knowledge Base Index and RAG Strategy
module: meta
status: active
confidence: n/a
last_verified: 2026-08-24
---

# Knowledge Base Index and RAG Strategy

## Document registry

| doc_id | Document | Kind | Status | Confidence | Verified |
|---|---|---|---|---|---|
| KB-000 | [README](README.md) | meta | active | — | 2026-08-12 |
| KB-001 | [Executive Summary](00-executive-summary.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-002 | [Source-of-Truth Rules](source-of-truth-rules.md) | meta | active | — | 2026-08-12 |
| KB-003 | [Investigation Registry](investigation-registry.md) | meta | active | — | 2026-08-12 |
| KB-004 | [Open Questions](open-questions.md) | meta | active | — | 2026-08-12 |
| KB-005 | this index | meta | active | — | 2026-08-12 |
| KB-010 | [System Overview](architecture/system-overview.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-011 | [Backend Architecture](architecture/backend-architecture.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-012 | [Database Architecture](architecture/database-architecture.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-013 | [Auth & Permissions](architecture/auth-and-permissions.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-014 | [Multi-Tenancy](architecture/multi-tenancy.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-015 | [Existing UI Architecture](architecture/frontend-architecture-existing.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-020 | [Module Inventory & Dependency Graph](modules/module-inventory.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-030 | [Business Rule Inventory](business-rules/business-rule-inventory.md) | as-is | **partial** | mixed | 2026-08-12 |
| KB-040 | [Existing API Surface](api/api-overview.md) | as-is | complete | confirmed | 2026-08-12 |
| KB-041 | [API Readiness Assessment](api/api-readiness-assessment.md) | **proposal** | complete | — | 2026-08-12 |
| KB-050 | [Frontend Architecture — Angular](frontend-new/react-architecture.md) *(filename kept; rewritten for Angular 2026-08-20 by M2-C00)* | **proposal** | complete | — | 2026-08-20 |
| KB-051 | [Design System](frontend-new/design-system.md) | **proposal** | complete | — | 2026-08-12 |
| KB-052 | [Feature Mapping](frontend-new/feature-mapping.md) | **proposal** | complete | — | 2026-08-12 |
| KB-053 | [Page Map](frontend-new/page-map.md) | as-is + proposal | complete | confirmed (left column) | 2026-08-12 |
| KB-060 | [Technical Debt & Risk Register](risks/technical-debt-register.md) | as-is | complete | mixed | 2026-08-21 |
| KB-061 | [Delete-Guard Audit — every `(bool CanDelete, string Message)` guard (INV-025)](risks/delete-guard-audit.md) | as-is | complete | mixed | 2026-08-21 |
| KB-070 | [Migration Strategy](migration/migration-strategy.md) | **proposal** | complete | — | 2026-08-12 |
| KB-071 | [Milestone Tracker](migration/milestones.md) | **proposal** | active | — | 2026-08-12 |
| KB-080 | [Master Execution Plan](execution/README.md) | **proposal** | active | — | 2026-08-12 |
| KB-081 | [Master Progress Tracker](execution/task-tracker.md) | meta | active | — | 2026-08-12 |
| KB-082 | [Dependency Graph & Critical Path](execution/dependency-graph.md) | **proposal** | active | — | 2026-08-12 |
| KB-083 | [Execution Prompt, Generation Rules & Verified Commands](execution/prompt-template.md) | meta | active | — | 2026-08-16 |
| KB-084 | [Review & Handoff Templates](execution/review-templates.md) | meta | active | — | 2026-08-16 |
| TASK-* | [Task specifications](execution/tasks/) | **proposal** | active | — | 2026-08-12 |
| KB-085 | [M0-00 Version-Control Baseline Decision Log](execution/M0-00-baseline-decisions.md) | execution | active | — | 2026-08-12 |
| KB-086 | [Build and Toolchain Baseline](execution/M0-15-build-baseline.md) | execution | complete | confirmed | 2026-08-17 |
| KB-087 | [CI Pipeline](execution/ci-pipeline.md) | execution | complete | confirmed | 2026-08-17 |
| KB-088 | [Repository-Driven Execution Workflow](execution/workflow.md) | meta | active | — | 2026-08-16 |
| KB-089 | [**Current Task**](execution/current-task.md) | meta | active | — | 2026-08-16 |
| KB-090 | [Task File Template](execution/task-template.md) | meta | active | — | 2026-08-16 |
| KB-091 | [Autonomous Runner — Agents, Model Routing, State Machine](execution/autonomous-runner.md) | meta | active | — | 2026-08-16 |
| KB-092 | [Validation Failure and Diagnosis Log](execution/failure-log.md) | execution | active | — | 2026-08-16 |
| KB-093 | [Autonomous Runner State](execution/runner-state.md) | execution | active | — | 2026-08-16 |
| ADR-001 | [Preserve the existing backend](decisions/ADR-001-keep-existing-backend.md) | decision | accepted | — | 2026-08-12 |
| ADR-002 | [REST API layer & conventions](decisions/ADR-002-rest-api-layer.md) | decision | accepted | — | 2026-08-12 |
| ADR-003 | [React stack](decisions/ADR-003-react-stack.md) | decision | **superseded by ADR-007** | — | 2026-08-12 |
| ADR-004 | [Server-side authorization](decisions/ADR-004-server-side-authorization.md) | decision | accepted (**P0**) | — | 2026-08-12 |
| ADR-005 | [Reporting & printing](decisions/ADR-005-reporting-and-printing.md) | decision | accepted | — | 2026-08-12 |
| ADR-007 | [Angular stack](decisions/ADR-007-angular-stack.md) | decision | **accepted** — supersedes ADR-003 | — | 2026-08-20 |
| KB-102 | [Stored-Procedure Inventory Reconciliation](architecture/stored-procedure-inventory.md) | as-is | complete | confirmed | 2026-08-13 |
| KB-103 | [Stored-Procedure Drift Across Tenant Databases (Q-14)](architecture/stored-procedure-drift.md) | as-is | partial | mixed | 2026-08-17 |
| KB-107 | [Milestone Review — M0 Stabilise (Gate G0)](execution/M0-milestone-review.md) | execution | active | — | 2026-08-19 |
| KB-105 | [Server-Side Screen-Right Authorization — Implementation Spec](architecture/server-side-authorization-spec.md) | **proposal** | complete | — | 2026-08-18 |
| KB-108 | [Row-Level Scoping and Account Gates (INV-028, Q-05…Q-08)](architecture/row-scope-and-account-gates.md) | architecture | complete | confirmed | 2026-08-20 |
| KB-113 | [Observability — Health Checks, Structured Logging and the Audit Trail](architecture/observability.md) | as-is | complete | confirmed | 2026-08-21 |
| KB-114 | [**Controller Conventions — the frozen API contract**](api/controller-conventions.md) | api | active (**frozen at M2-B03**) | confirmed | 2026-08-24 |
| KB-112 | [**OpenAPI Contract and Generated TypeScript Client**](api/generated-client.md) | api | active | confirmed | 2026-08-24 |

## doc_id allocation

Ranges are reserved so that concurrent sessions cannot collide. **Before claiming an id,
`grep` this file for it.** Ids are never reused or renumbered.

> ### ⚠ Id collision — half resolved 2026-08-19, half still live
>
> Two unmerged branches each claimed the same ids, because `grep`-before-claim only sees
> *merged* work and cannot see a sibling branch. **`M2-A01-01` renumbered and merged; `M0-06`
> has not, and must renumber before it does.**
>
> | Id claimed by both | `M2-A01-01` — **renumbered to** | `M0-06` — **must still renumber to** |
> |---|---|---|
> | `KB-104` | **`KB-105`** ✔ merged | `KB-106` — *note it is also cited in an `ApplicationDbContext.cs` source comment, which must change with it* |
> | `INV-035` | **`INV-037`** ✔ merged | `INV-038` |
> | `Q-22` `Q-23` `Q-24` | **`Q-27` `Q-28` `Q-29`** ✔ merged | n/a — `M0-06` holds `Q-25`/`Q-26`, which do not collide |
> | footnote `¹³` | **`¹⁸`** ✔ merged | `¹⁶` — does not collide, `master` skips 16 |
>
> **The collision was wider than first recorded.** It was found as two ids; it was actually
> **six** — three open questions and a tracker footnote were also duplicated, and merging blind
> would have silently overwritten three of `master`'s open questions with three different ones
> bearing the same numbers.
>
> **This is a process defect, not a mistake by either session.** `grep`-before-claim only works
> against what is *merged*; it cannot see a sibling branch. Two branches allocating from one
> registry will collide again. Until the allocation rule accounts for in-flight branches —
> reserving on branch creation, or partitioning ranges per workstream — **check
> `git branch --no-merged master` for competing claims before allocating an id**, not just this
> file. `M0-06`'s runbook already cites KB-104 in a source comment
> (`ApplicationDbContext.cs`), so renumbering that side means editing that comment too.

| Range | Purpose | Allocated |
|---|---|---|
| KB-000 – KB-079 | Analysis knowledge base (as-is + proposals) | through KB-070. **KB-061 claimed 2026-08-21 by M0-10** — [risks/delete-guard-audit.md](risks/delete-guard-audit.md). It sits in the `KB-06x` *risks* decade by design rather than taking a `KB-1xx` task-artefact id, because the `M0-10` task file named it and the decade grouping is the point of this range. `KB-123` was held in reserve as a fallback in case `KB-061` proved taken; **it was free, so `KB-123` remains unallocated.** |
| KB-080 – KB-099 | Execution plan meta-documents *(range extended 2026-08-16 — 080–089 was full)* | KB-080 … KB-093 (contiguous — KB-087 claimed 2026-08-17 by M0-07). **Next free: KB-094** |
| KB-085 | **Claimed 2026-08-12 by M0-00** — [M0-00-baseline-decisions.md](execution/M0-00-baseline-decisions.md) | allocated |
| KB-086 | **Claimed 2026-08-17 by M0-15** — [M0-15-build-baseline.md](execution/M0-15-build-baseline.md) | allocated |
| KB-087 | **Claimed 2026-08-17 by M0-07** — [ci-pipeline.md](execution/ci-pipeline.md) | allocated |
| KB-088 – KB-090 | **Claimed 2026-08-16** — [workflow.md](execution/workflow.md), [current-task.md](execution/current-task.md), [task-template.md](execution/task-template.md) | allocated |
| KB-091 – KB-093 | **Claimed 2026-08-16** — [autonomous-runner.md](execution/autonomous-runner.md), [failure-log.md](execution/failure-log.md), [runner-state.md](execution/runner-state.md) | allocated |
| **KB-100 +** | **Artefacts produced *by* tasks** — investigation outputs, `@code` triage reports, contract specs, decision briefs | **claimed:** KB-100/101 (M2-B12-01/02), KB-102 (M0-01-01, [stored-procedure-inventory.md](architecture/stored-procedure-inventory.md)), KB-103 (M0-02, [stored-procedure-drift.md](architecture/stored-procedure-drift.md)), KB-110–112 reserved (M2-B08…B10); **KB-113 claimed and USED 2026-08-21 by M2-B11** ([observability.md](architecture/observability.md)), KB-105 (M2-A01-01, [server-side-authorization-spec.md](architecture/server-side-authorization-spec.md)). **KB-108 claimed 2026-08-20 by M2-A08** ([row-scope-and-account-gates.md](architecture/row-scope-and-account-gates.md)). **KB-109 claimed 2026-08-24** ([q28-r65-decision-brief.md](decisions/KB-109-q28-r65-decision-brief.md)). **KB-114 claimed and USED 2026-08-24 by M2-B03** ([controller-conventions.md](api/controller-conventions.md)). **KB-112 claimed and USED 2026-08-24 by M2-B10** ([generated-client.md](api/generated-client.md)) — it was the id reserved for that task, and it was still free. **KB-115 claimed 2026-08-25** ([owner-action-list.md](execution/owner-action-list.md)). **Next free: KB-116** only if `M2-B08` releases its reservation, otherwise **KB-115+** — KB-107 claimed 2026-08-19 by the M0 milestone review; `M0-06`'s unmerged branch still claims `KB-104`, which must become `KB-106` before it merges (its id is also cited in an `ApplicationDbContext.cs` source comment) — but `M0-06`'s unmerged branch still claims `KB-104`, which must become `KB-106` before it merges (its id is also cited in an `ApplicationDbContext.cs` source comment) |
| ADR-nnn | Architecture decisions | ADR-001…ADR-005 and **ADR-007** (Angular stack, 2026-08-20, supersedes ADR-003). **ADR-006 is RESERVED by `M0-11`** for `ADR-006-fifo-under-issue.md` and must not be reused — ADR-007 skipped it deliberately, having checked `M0-11.md:185` first. **Next free: ADR-008** |
| TASK-`<id>` | Task specification files under `execution/tasks/` | one per task |
| INV-nnn | Investigation registry rows | through INV-040 (030–033 reserved; **INV-036** claimed 2026-08-19 by M0-13; **INV-037** by M2-A01-01, renumbered from 035 on merge; **INV-039** by M2-B07 (merged `ffbb1dd`); **INV-040** by M2-A06 (merged `76eca5d`) — *corrected 2026-08-20: this row previously credited INV-040 to M2-B07 as well, which was wrong. M2-B07 claimed INV-039 only. The registry was right and this row was not; M2-A06 read the registry, so no collision resulted*). **Next free: INV-051** — *this row had gone stale at INV-042; the* Reserved ids *table in [KB-003](investigation-registry.md) is the sole authority and had already reached INV-049.* **INV-050 claimed 2026-08-24 by M2-B03** (service-method → REST-verb mapping). INV-041 claimed 2026-08-20 by M2-B02 (sort delivery to services with hardcoded ordering). |
| BR-`<AREA>`-nnn | Business rules | see [KB-030](business-rules/business-rule-inventory.md) |
| R-nn | Risks | through R-37, **plus R-60…R-64 claimed 2026-08-21 by M0-10** (delete-guard audit — the surviving R-08 instance, unreachable guards, stub guards, missing guards, advisory guards). The gap R-38…R-59 is deliberate: the block was reserved for M0-10 so it could not collide with a sibling branch. **Next free: R-38**, and R-65+ |
| Q-nn | Open questions | through Q-29, **with a gap**: Q-20 (M0-07), Q-21 (M0-12-01 close-out), Q-22 (M0-12-01 push authority), Q-23/Q-24 (M0-12-02), **Q-27/Q-28/Q-29 (M2-A01-01, renumbered from 22–24 on merge)**. **`Q-25` and `Q-26` are claimed by `M0-06`'s unmerged branch — do not reuse them.** **Next free: Q-37** — Q-36 claimed 2026-08-20 by M2-B02 (the `CurrencyList.razor` `Status` filter key has no builder case, so that dropdown filters nothing). Previously — Q-34 and Q-35 claimed 2026-08-20 by M2-A06 (refusal-tuple 404/500 semantics; the 503-for-unresolved-tenant and ignore-caller-header design choices). Previously — Q-30 claimed by M2-C01, Q-31 by M2-B07, Q-32 by the M0 milestone-review correction, Q-33 by M2-C04-01 (`UserThemePreference.IsDarkMode` cannot represent `system`). **Q-60…Q-64 claimed 2026-08-21 by M0-10** (delete-guard audit: guards-in-transaction, null-handling convention, the upstream-only integrity asymmetry, the three commented-out Cash Flow guards, the guards that cannot refuse). The gap Q-37…Q-59 is deliberate — the block was reserved for M0-10 so it could not collide with a sibling branch, and Q-36…Q-40, Q-45…Q-48 and Q-55 are held on branches `grep` cannot see. **Next free after the reserved block: Q-65.** |

A task that produces a durable document allocates the next free **KB-1xx** id, adds its row
to the registry above, and records the id in the task's *Documentation Updates* section.
Task files themselves use `TASK-<id>` and never consume a KB number.

## Question → document routing

Use this table before searching the repository.

| If the question is about… | Read |
|---|---|
| What is this system, how big, what stack | KB-001, KB-010 |
| Projects, hosting, DI, config, deployment | KB-010 |
| Services, repositories, transactions, calculation engine, reporting, integrations, logging | KB-011 |
| Entities, tables, DbContexts, migrations, seed data, `Ref*SubId` chains, stored procedures | KB-012 |
| Login, JWT, roles, screen rights, approval authority, QR login | KB-013 |
| Tenants, connection strings, host resolution | KB-014 |
| Health checks, structured logging, the user-action audit trail, log retention, credential redaction in logs | KB-113 |
| Blazor pages, routes, MudBlazor components, `@code` density, the Angular pilot | KB-015 |
| Which modules exist, what depends on what, migration order | KB-020 |
| A specific business rule and its evidence | KB-030 |
| What endpoints exist today | KB-040 |
| What endpoints must be built, contract conventions, error shape | KB-041, ADR-002 |
| **How do I write a new controller?** (route, attributes, paging, errors, `[ProducesResponseType]`, the conformance checklist) | **KB-114 — [`api/controller-conventions.md`](api/controller-conventions.md). Frozen at M2-B03; a change to it is a breaking, versioned API change** |
| **How do I regenerate the API contract and the TypeScript client?** Which generator, why not the others, how the CI drift check works, what `decimal` becomes on the wire | **KB-112 — [`api/generated-client.md`](api/generated-client.md)** |
| Frontend stack, state, data fetching, permission rendering, DocumentEditor | KB-050, **ADR-007** (ADR-003 is superseded — do not implement from it) |
| Colours, typography, components, layouts, accessibility | KB-051 |
| Which Angular screen replaces which Blazor screen, and how hard | KB-052 |
| Old route → new route | KB-053 |
| Risks, defects, severity, what to fix first | KB-060 |
| **Can this document be deleted? Which guard enforces it? Is that guard reached, and does it run in the transaction?** | **KB-061** — the full 79-guard inventory, including every guard judged correct. **Read it before writing any `DELETE` endpoint**, and before re-reading any `CanDelete…` method |
| Where are the delete paths with **no** guard at all | KB-061 § 5.1 |
| Why a `CanDelete…` grep gives the wrong answer (the `Async` and `CanDelete`-prefix traps; `UserRight.CanDelete` noise) | KB-061 § 1.1–1.2, § 1.5 |
| Timeline, phases, sequencing, rollback | KB-070 |
| What are we working on now, task-level checklist, exit gates | KB-071 |
| Is the backend ASP.NET Core Web API? | KB-071 (§ Backend platform), ADR-001, ADR-002 |
| **What am I working on right now** | **KB-089 — `execution/current-task.md`** |
| **How a session runs: lifecycle, procedure, completion, handover** | **KB-088 — `execution/workflow.md`** |
| **How the autonomous runner works: agents, model routing, retries, escalation, safety stops** | **KB-091 — `execution/autonomous-runner.md`** |
| Why a task failed validation, and what was already tried | KB-092 — `execution/failure-log.md` |
| Is a run live, on what, and why did it stop | KB-093 — `execution/runner-state.md` |
| What CI runs, why a CI build failed, how to change the warning baseline | KB-087 — `execution/ci-pipeline.md` |
| Invariant project context an AI session needs first | `CLAUDE.md` at the repository root |
| The full executable plan: milestones → tasks → gates | KB-080 *(55 KB — deep-link, do not read whole)* |
| Status of every task; is task X ready? | KB-081 |
| What blocks what; what runs in parallel; critical path; **how the next task is chosen** | KB-082 |
| Which build/test commands are actually verified | KB-083 § Verified repository commands |
| How do I write or regenerate a task file | KB-090, with the rules in KB-083 |
| How do I close a task, hand over, or pass a milestone gate | KB-084 |
| The specification for a specific task | `execution/tasks/<TASK-ID>.md` |
| Why a decision was made | `decisions/` |
| Whether something has already been investigated | KB-003 |
| What is still unknown | KB-004 |
| Which stored procedures exist and which are missing | KB-102 |
| Do stored procedures differ between tenants? (Q-14 drift check, method + tooling) | KB-103 |
| **How is `[RequireScreen]`/`[RequireRight]` meant to behave** — the deny truth table, screen-name matching, duplicate rows, the `403`/`401` bodies, the rights cache key and TTL | **KB-105** |
| Which exact screen-name strings may a controller declare? (the 152 seeded names) | KB-105 § Appendix A |
| Does an `Administrator` bypass screen rights? | KB-105 § D-5 — **no**, and why |
| **Which rows may a user see** — `User.StateCodesCsv`, why it scopes `Leads` and nothing else, and the API mechanism | **KB-108** |
| Why an API login can fail with `403` rather than `401` (expired trial, device, platform) | **KB-108** §4, [KB-040](api/api-overview.md) |

## RAG strategy

### Frontmatter as the retrieval index

Every document carries structured metadata designed for filtered retrieval:

```yaml
doc_id:            stable identifier — cite this, never a file path
title:
module:            architecture | api | frontend-new | risks | migration | decisions | meta | <erp module>
source_files:      [ repo-relative paths the document was derived from ]
entities:          [ EF entity names ]
api_endpoints:     [ "METHOD /path" ]
database_tables:   [ table names ]
business_rules:    [ BR-xxx-nnn ids ]
status:            complete | partial | proposal | active
confidence:        confirmed | inferred | mixed | n/a
last_verified:     YYYY-MM-DD
dependencies:      [ doc_ids ]
```

**Retrieval recipe**

1. **Filter first, embed second.** Most questions name an entity (`MfgPo`), a table, an
   endpoint, or a module. Metadata filtering on `entities` / `database_tables` /
   `api_endpoints` / `module` narrows to 1–3 documents before any semantic search runs.
2. **Chunk on `##` headings**, carrying the parent frontmatter into every chunk. Headings
   in this KB are written to be self-describing for exactly this reason.
3. **Never mix as-is with to-be.** Filter on `status`: `complete`/`partial` = current
   system; `proposal` = plan. Answering "how does X work?" from a proposal document is the
   single worst failure mode for this knowledge base.
4. **Surface `confidence` in the answer.** An `inferred` claim must be reported as
   inferred.
5. **Surface `last_verified`.** If the cited `source_files` have changed since that date,
   warn that the document may be stale rather than answering confidently.
6. **Follow `dependencies`** to pull adjacent context (e.g. KB-013 → KB-012 for the
   underlying tables).

### Evidence format for new findings

```
Finding:      Sales Orders cannot be deleted once any downstream document exists.
Evidence:     V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs:465-565
Business rule: BR-SO-001
Confidence:   Confirmed
Last verified: 2026-08-12
```

Always cite `file:line`, not just a file. Line numbers are what make a claim
re-verifiable — and what make staleness detectable.

### Anti-repetition protocol

Before any repository investigation:

1. Search KB-003 (Investigation Registry) for the topic.
2. If `Complete` and not stale → reuse, cite the doc_id, **stop**.
3. If `Partial` → read the stated gap; investigate only the gap.
4. If absent or stale → investigate, then **add the row and the document**.
5. Record negative results too. "Grepped for `IHostedService` across the solution, found
   none" (INV-022) is a finding that must never be re-derived.

### Maintenance

- Update `last_verified` whenever a document is re-checked against code.
- When code contradicts a document, **the code wins**: update the document, note the
  delta, bump the date, and update the registry row.
- ADRs are immutable once accepted. Superseding an ADR means writing a new one that says
  which it supersedes.
- Keep `business_rules` ids stable forever. Add; never renumber.
