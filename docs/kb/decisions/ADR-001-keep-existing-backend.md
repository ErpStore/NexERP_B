---
doc_id: ADR-001
title: Preserve the existing backend; add an HTTP layer rather than rewriting
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-011, KB-041]
---

# ADR-001 — Preserve the existing backend

**Status:** Accepted · **Date:** 2026-08-12

## Context

The product goal is a new React frontend. A frontend replacement often invites a backend
rewrite. The measured facts argue strongly against that here:

- ~128,500 LOC of business services encoding a decade of manufacturing-ERP domain rules,
  including GST tax computation, FIFO stock allocation, multi-level approvals, and
  statutory e-Invoice/e-Way integration.
- Services already speak ViewModels, not entities, and are UI-framework-agnostic in ~97%
  of files (14 of 285 reference `Pages`, 19 reference Blazor/MudBlazor types).
- Explicit transactions are already used (302 sites).
- **Zero automated tests exist** — a rewrite would be unverifiable.
- `CurrencyController` already proves an untouched service can be exposed over HTTP.

## Decision

**Keep the existing backend. Add a REST API layer over the existing business services.
Do not rewrite business logic, do not change the database schema, do not replace EF Core,
AutoMapper, FastReport, or the stored procedures.**

Backend changes are limited to:
1. New controllers (thin: bind → authorize → call one service → map).
2. Server-side authorization (ADR-004).
3. Cross-cutting infrastructure: error contract, refresh tokens, tenant resolution,
   file/report endpoints.
4. Decoupling the 14 files that reference the UI namespace.
5. **Relocating business logic out of Razor `@code` into services** — this is a *move*,
   not a rewrite.

Any change to a business rule requires its own ADR stating the reason.

## Consequences

**Positive.** Domain knowledge is preserved. Risk concentrates in the frontend, where
mistakes are visible and cheap. The Blazor app keeps working throughout, enabling a
strangler-fig rollout with no data migration and instant rollback. Existing password
hashes, permissions, and tenant data continue to work unchanged.

**Negative.** Existing defects are inherited (R-07, R-08, R-12, R-16) and must be fixed
deliberately rather than disappearing in a rewrite. Existing architectural choices
(Repository/UoW over EF, `IQueryable` leakage, magic screen codes) persist. The API layer
must adapt to the services' shapes rather than the reverse — occasionally producing a
less-than-ideal endpoint.

**Neutral.** Two UIs run concurrently for 9–14 months, requiring feature discipline: any
change to a migrating module must land in the service layer so both UIs get it.

## Alternatives rejected

| Alternative | Why rejected |
|---|---|
| Rewrite the backend (e.g. clean architecture + CQRS) | 128k LOC of untested domain logic; would take longer than the frontend and risk the business |
| Keep Blazor Server and just restyle | Does not meet the goal of a new product; SignalR-circuit model already causes logout-on-refresh and does not suit mobile/offline |
| Wrap Blazor pages in an SPA shell | Preserves the worst property of the system (logic in `@code`) |
| Build a new API from scratch talking to the same DB | Duplicates 128k LOC of rules; the two implementations would diverge |
