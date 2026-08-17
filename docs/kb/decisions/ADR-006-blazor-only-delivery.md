---
doc_id: ADR-006
title: Blazor-only delivery — drop the React rebuild
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-17
dependencies: [ADR-001, ADR-002, ADR-003, ADR-004, KB-020, KB-060, KB-070, KB-090]
---

# ADR-006 — Blazor-only delivery; the React rebuild is dropped

**Status:** Accepted · **Date:** 2026-08-17 · **Supersedes:** [ADR-003](ADR-003-react-stack.md)

## Context

[ADR-003](ADR-003-react-stack.md) selected a React 19 + Vite stack and
[KB-070](../migration/migration-strategy.md) planned an 11–14 month strangler-fig migration
of ~140 screens onto it, backed by a new REST surface of 60–80 controllers
([ADR-002](ADR-002-rest-api-layer.md)). That plan carried an explicit non-goal: **no new ERP
functionality during the migration.**

Since then the client has produced a 24-slide requirement set (`ERP_presentation.pptx`)
that is almost entirely *new functionality* and *defect elimination* — item de-duplication
with merge, item-code continuity end to end, DC-based GRN with later PO entry, GRN lock,
multi-document batch operations, a common sub-contract UI, consolidated multi-station job
orders, RGP/NRGP gate passes with online approval, and report performance. Their stated
priorities are operational, not architectural.

Delivering that list *and* a framework migration at the same time means paying for the
migration before the client sees any of what they asked for.

## Decision

**The product stays on Blazor Server (.NET 9). The React rebuild is cancelled. Requirements
are delivered module by module on the existing application.**

Specifically:

1. No React or Angular application is built. `frontend/vsmart-erp/` (Angular pilot) is
   archived, not converted.
2. The existing 364 Razor components are kept. Screens are refactored **only when a
   commissioned wave touches them**.
3. `V.SMART.Api` remains, but is no longer on the critical path for the UI. It stays for
   integrations, mobile, and any future external consumer. [ADR-002](ADR-002-rest-api-layer.md)
   is narrowed to that purpose, not withdrawn.
4. [ADR-004](ADR-004-server-side-authorization.md) still holds. Authorization moves out of
   the UI into the service layer regardless of which UI calls it — this was never a React
   requirement.
5. The `@code` → service extraction rule from KB-070 principle 3 still holds, but is now
   scoped per wave rather than programme-wide.
6. UI consistency is delivered by rolling out the shared component kit and the Zoho-style
   theme (`docs/ZOHO_UI_REDESIGN_PLAN.md`) inside Blazor, not by changing framework.

## Consequences

**Positive**

- ~1,047 engineering person-days (~74% of the core estimate) of pure re-platforming cost is
  removed and redirected to functionality the client actually asked for ([KB-090](../migration/blazor-only-estimate.md) §1).
- One stack, one deployment, one team. No dual-stack parity testing, no duplicate
  maintenance window, no feature-flag routing between two applications.
- First production release lands at ~month 3 instead of after a foundation phase.
- No API-contract drift risk, no OpenAPI codegen pipeline to own.
- The team's existing Blazor/.NET skill is used directly; no React hiring or ramp-up.

**Negative — accepted knowingly**

- **The framework debt stays.** ~184,000 LOC of logic in `@code` blocks is paid down only in
  touched modules. Untouched modules keep it indefinitely.
- Blazor Server's operational characteristics remain: circuit/WebSocket dependency, server
  memory per connected user, latency sensitivity for shop-floor and remote users. This
  constrains, in particular, the mobile DC requirement.
- The talent pool for Blazor is smaller than for React. Long-term hiring is harder.
- A future move off Blazor is not eliminated, only deferred — and the extraction work done
  per wave is exactly what would make that move cheaper later.

**Neutral**

- No change to the database, EF Core, AutoMapper, FastReport, or the 94 stored procedures.
- [ADR-005](ADR-005-reporting-and-printing.md) (reporting and printing) is unaffected.

## Alternatives rejected

| Alternative | Why not |
|---|---|
| Continue the React migration, defer the CRs | The client's operational pain (duplicate items, lost SCNs, slow reports) continues for 12+ months while paying for a re-skin |
| React for new modules, Blazor for old | Two stacks, two auth models, two component kits, permanently. The worst of both, and the CR list spans both halves |
| Rewrite from scratch | 128,518 LOC of working, tenant-proven business logic discarded. Not a serious option |
| Blazor now, revisit React after go-live | This *is* the decision — deferral, not exclusion, and the per-wave extraction keeps that door open |

## Revisit when

- Blazor Server circuit behaviour becomes the top user complaint at production scale, or
- a public/customer-facing portal is commissioned (no SEO/SSR need exists today), or
- shop-floor and field mobile usage grows past what responsive Blazor serves well.
