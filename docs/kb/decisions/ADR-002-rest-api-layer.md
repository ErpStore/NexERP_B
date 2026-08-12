---
doc_id: ADR-002
title: REST API layer, contract conventions, and tenant resolution for the SPA
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-014, KB-040, KB-041]
---

# ADR-002 — REST API layer and contract conventions

**Status:** Accepted · **Date:** 2026-08-12

## Context

The SPA needs an HTTP surface. Today there are 6 endpoints across 2 controllers, with
inconsistencies already visible in that small sample: two different 400 body shapes from
one endpoint, untyped `Dictionary<string, object>` filters, no route/body id validation, no
versioning. Replicating those across 60–80 controllers would bake them in permanently.

Separately, tenant resolution currently depends on the request host — which breaks when the
SPA is served from its own origin.

## Decision

### 1. REST over GraphQL/gRPC

REST with resource-oriented routes. The domain is document-CRUD with explicit workflow
commands; the existing service methods map to it almost one-to-one. GraphQL would require
a resolver layer with no corresponding benefit; gRPC does not fit browser-first delivery
or the file/PDF payloads this app needs.

### 2. Contract conventions (mandatory for every controller)

```
GET    /api/v1/{resource}?pageNumber&pageSize&sort&<typed filters>
       → { items, totalCount, pageNumber, pageSize }
GET    /api/v1/{resource}/{id}
POST   /api/v1/{resource}                 → 201 + Location
PUT    /api/v1/{resource}/{id}            → 200
DELETE /api/v1/{resource}/{id}            → 204 | 409 (business rule) | 404
POST   /api/v1/{resource}/{id}/{command}  → workflow commands
GET    /api/v1/{resource}/{id}/print      → application/pdf
```

- Resources are **plural kebab-case**: `/api/v1/sales-orders`, `/api/v1/purchase-orders`.
- Payloads are the **existing `…VM` ViewModels, unchanged** — no parallel DTO hierarchy.
- Filters are **typed query DTOs**, not `Dictionary<string, object>`.
- Controllers are thin: bind → authorize → one service call → map. No business logic.

### 3. Workflow commands are server-side and atomic

Anything that is a business operation — cancel, short-close, approve, reject, release,
post — is a single `POST /{id}/{verb}` that runs the **entire** sequence server-side.
The client collects input (e.g. the mandatory cancellation reason of BR-SO-003) and calls
one endpoint. **The client never orchestrates a multi-step business operation.**

This directly addresses the current situation where `MfgPOUpsert.razor` sequences
validation → transaction check → quantity revert → status update from the UI.

### 4. Error contract: `application/problem+json` everywhere

| Status | Meaning | Body |
|---|---|---|
| 400 | model binding / `DataAnnotations` | `errors` dictionary keyed by field |
| 401 | unauthenticated / token expired | — |
| 403 | screen right or approval authority denied | which screen + right |
| 404 | not found | — |
| **409** | **business-rule refusal** | `title` carries the **service's existing message verbatim** |
| 500 | unhandled | `traceId` only |

The 409 rule matters: strings like *"Cannot delete this Sales Order as a Sales DC
transaction exists."* are product UX written by the domain team. They are surfaced
unchanged, never replaced with generic text.

### 5. Tenant resolution for the SPA

**Tenant is supplied in the login request** and carried in the JWT thereafter.

```
POST /api/v1/auth/login  { tenant, username, password }
  → resolve tenant by Name/Hostname from MasterDb
  → authenticate
  → JWT with the existing TenantId claim
Every subsequent request → tenant from the JWT claim (existing resolution step 1, unchanged)
```

The SPA derives `tenant` from its own subdomain where available, falling back to a picker.
`TenantProvider`, `TenantDbContextFactory`, `TenantInfo`, and the database-per-tenant model
are **unchanged**.

CORS moves from the hardcoded `http://localhost:4200` to a per-environment configured
origin list.

### 6. Versioning and generation

- All routes under `/api/v1`.
- OpenAPI is the contract; the **TypeScript client is generated in CI**, never hand-written.
  A contract change that breaks the client fails the build.

## Consequences

**Positive.** One template makes 60–80 controllers mechanical and parallelisable. The
generated client eliminates drift. Workflow commands push orchestration where it belongs
and make the `@code` extraction goal concrete. Tenant isolation stays exactly as sound as
it is today.

**Negative.** The existing `CurrencyController` must be reworked to the convention (small,
do it first as the template). `/api/auth/login` gains a `tenant` field — a breaking change
to the Angular pilot, which is being archived anyway.

**Neutral.** Some service signatures fit REST awkwardly (e.g. dynamic-filter dictionaries);
those get a typed DTO at the controller and an adapter into the existing service, without
changing the service.
