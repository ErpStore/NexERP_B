---
doc_id: KB-000
title: Knowledge Base Entry Point
module: meta
source_files: []
status: active
confidence: n/a
last_verified: 2026-08-12
---

# V.SMART / NexGen ERP — Repository Knowledge Base

This is the **persistent source of truth** for how the existing ERP works and how the new
React frontend will be built. It exists so that the same parts of the repository are not
investigated twice.

## How to use this knowledge base

1. **Before investigating anything**, read [`INDEX.md`](INDEX.md) and
   [`investigation-registry.md`](investigation-registry.md).
2. If a relevant investigation exists and is not stale, **reuse its findings**. Cite the
   doc_id.
3. Re-investigate only when: no entry exists, the entry is marked `stale`, or current
   source code contradicts the recorded finding.
4. When you complete a new investigation, add a registry row and a document with the
   standard frontmatter.

## Rules that govern every document here

See [`source-of-truth-rules.md`](source-of-truth-rules.md). Summary:

- Every claim is tagged **Confirmed** (traced in source), **Inferred** (reasoned from
  code but not directly stated), or **Unknown** (needs investigation).
- **Current source code wins** over any prose documentation, including the pre-existing
  `docs/ARCHITECTURE.md`, `docs/FRONTEND_MIGRATION_ANGULAR_REACT.md`, and
  `.github/copilot-instructions.md`.
- Existing architecture and proposed architecture are never mixed in one document.
  Files under `architecture/` and `modules/` describe **what exists**. Files under
  `frontend-new/` and `migration/` describe **what is proposed**.

## Map

| Area | Directory | Describes |
|---|---|---|
| Executive summary | [`00-executive-summary.md`](00-executive-summary.md) | Existing system, one page |
| Existing architecture | [`architecture/`](architecture/) | **As-is** system, backend, data, auth, tenancy, UI |
| Existing modules | [`modules/`](modules/) | **As-is** ERP module inventory |
| Existing business rules | [`business-rules/`](business-rules/) | **As-is** rules with file:line evidence |
| Existing + planned API | [`api/`](api/) | Current controllers, readiness gap |
| Proposed frontend | [`frontend-new/`](frontend-new/) | **To-be** React stack, design system, page map |
| Risks | [`risks/`](risks/) | Technical debt register, severity-classified |
| Plan | [`migration/`](migration/) | Phased migration strategy |
| Decisions | [`decisions/`](decisions/) | ADRs |
| Meta | [`investigation-registry.md`](investigation-registry.md), [`open-questions.md`](open-questions.md) | What has been investigated; what is still unknown |

## Repository layout (physical)

The working directory `C:\Kumar\NexGen-ERP---2025-master` is a wrapper. The actual git
repository is the nested `NexGen-ERP---2025-master\` folder. All paths in this knowledge
base are **relative to the nested repository root** unless stated otherwise.
