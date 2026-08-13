---
doc_id: KB-005
title: Knowledge Base Index and RAG Strategy
module: meta
status: active
confidence: n/a
last_verified: 2026-08-12
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
| KB-050 | [React Architecture](frontend-new/react-architecture.md) | **proposal** | complete | — | 2026-08-12 |
| KB-051 | [Design System](frontend-new/design-system.md) | **proposal** | complete | — | 2026-08-12 |
| KB-052 | [Feature Mapping](frontend-new/feature-mapping.md) | **proposal** | complete | — | 2026-08-12 |
| KB-053 | [Page Map](frontend-new/page-map.md) | as-is + proposal | complete | confirmed (left column) | 2026-08-12 |
| KB-060 | [Technical Debt & Risk Register](risks/technical-debt-register.md) | as-is | complete | mixed | 2026-08-12 |
| KB-070 | [Migration Strategy](migration/migration-strategy.md) | **proposal** | complete | — | 2026-08-12 |
| KB-071 | [Milestone Tracker](migration/milestones.md) | **proposal** | active | — | 2026-08-12 |
| KB-080 | [Master Execution Plan](execution/README.md) | **proposal** | active | — | 2026-08-12 |
| KB-081 | [Master Progress Tracker](execution/task-tracker.md) | meta | active | — | 2026-08-12 |
| KB-082 | [Dependency Graph & Critical Path](execution/dependency-graph.md) | **proposal** | active | — | 2026-08-12 |
| KB-083 | [Fresh-Session Prompt Template](execution/prompt-template.md) | meta | active | — | 2026-08-12 |
| KB-084 | [Review & Handoff Templates](execution/review-templates.md) | meta | active | — | 2026-08-12 |
| TASK-* | [Task specifications + execution prompts](execution/tasks/) | **proposal** | active | — | 2026-08-12 |
| KB-085 | [M0-00 Version-Control Baseline Decision Log](execution/M0-00-baseline-decisions.md) | execution | active | — | 2026-08-12 |
| KB-102 | [Stored-Procedure Reference/DDL Reconciliation](architecture/stored-procedure-inventory.md) | architecture | complete | confirmed | 2026-08-13 |
| ADR-001 | [Preserve the existing backend](decisions/ADR-001-keep-existing-backend.md) | decision | accepted | — | 2026-08-12 |
| ADR-002 | [REST API layer & conventions](decisions/ADR-002-rest-api-layer.md) | decision | accepted | — | 2026-08-12 |
| ADR-003 | [React stack](decisions/ADR-003-react-stack.md) | decision | accepted | — | 2026-08-12 |
| ADR-004 | [Server-side authorization](decisions/ADR-004-server-side-authorization.md) | decision | accepted (**P0**) | — | 2026-08-12 |
| ADR-005 | [Reporting & printing](decisions/ADR-005-reporting-and-printing.md) | decision | accepted | — | 2026-08-12 |

## doc_id allocation

Ranges are reserved so that concurrent sessions cannot collide. **Before claiming an id,
`grep` this file for it.** Ids are never reused or renumbered.

| Range | Purpose | Allocated |
|---|---|---|
| KB-000 – KB-079 | Analysis knowledge base (as-is + proposals) | through KB-070 |
| KB-080 – KB-089 | Execution plan meta-documents | KB-080 … KB-084 |
| KB-085 | **Claimed 2026-08-12 by M0-00** — [M0-00-baseline-decisions.md](execution/M0-00-baseline-decisions.md) | allocated |
| KB-086 – KB-087 | *(proposed by M0-07/M0-15/M0-01-01 before this table existed — reconcile or re-map into KB-100+ when those tasks run; KB-085 is no longer free — see row above)* | provisional |
| **KB-100 +** | **Artefacts produced *by* tasks** — investigation outputs, `@code` triage reports, contract specs, decision briefs | **claimed:** KB-100/101 (M2-B12-01/02), KB-102 (M0-01-01), KB-110–113 (M2-B08…B11). **Next free: KB-103**, or KB-114+ |
| ADR-nnn | Architecture decisions | through ADR-005 |
| TASK-`<id>` | Task specification files under `execution/tasks/` | one per task |
| INV-nnn | Investigation registry rows | through INV-034 (030–033 reserved, next free INV-035 — see investigation-registry.md) |
| BR-`<AREA>`-nnn | Business rules | see [KB-030](business-rules/business-rule-inventory.md) |
| R-nn | Risks | through R-37 |
| Q-nn | Open questions | through Q-19 |

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
| Which stored procedures exist and which are missing DDL | KB-102 |
| Login, JWT, roles, screen rights, approval authority, QR login | KB-013 |
| Tenants, connection strings, host resolution | KB-014 |
| Blazor pages, routes, MudBlazor components, `@code` density, the Angular pilot | KB-015 |
| Which modules exist, what depends on what, migration order | KB-020 |
| A specific business rule and its evidence | KB-030 |
| What endpoints exist today | KB-040 |
| What endpoints must be built, contract conventions, error shape | KB-041, ADR-002 |
| React stack, state, data fetching, permission rendering, DocumentEditor | KB-050, ADR-003 |
| Colours, typography, components, layouts, accessibility | KB-051 |
| Which React screen replaces which Blazor screen, and how hard | KB-052 |
| Old route → new route | KB-053 |
| Risks, defects, severity, what to fix first | KB-060 |
| Timeline, phases, sequencing, rollback | KB-070 |
| What are we working on now, task-level checklist, exit gates | KB-071 |
| Is the backend ASP.NET Core Web API? | KB-071 (§ Backend platform), ADR-001, ADR-002 |
| The full executable plan: milestones → tasks → prompts | KB-080 |
| What should I work on next; is task X ready? | KB-081 |
| What blocks what; what can run in parallel; critical path | KB-082 |
| How do I write/regenerate a task execution prompt | KB-083 |
| How do I close a task or a milestone gate | KB-084 |
| The prompt for a specific task | `execution/tasks/<TASK-ID>.md` |
| Why a decision was made | `decisions/` |
| Whether something has already been investigated | KB-003 |
| What is still unknown | KB-004 |

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
