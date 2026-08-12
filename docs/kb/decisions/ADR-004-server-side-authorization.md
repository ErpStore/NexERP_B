---
doc_id: ADR-004
title: Move permission enforcement from the UI to the server
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-013, KB-040, KB-060]
---

# ADR-004 — Server-side authorization

**Status:** Accepted · **Priority: P0 blocker** · **Date:** 2026-08-12

## Context

**Confirmed finding:** permission enforcement in the existing system exists **only in the
UI**. `Shared/BaseUserRightsComponent.cs` loads `UserRight` rows and exposes `CanView`,
`CanCreate`, `CanEdit`, `CanDelete`, `IsHidden`; 296 of 333 pages inherit it. Grepping
`BusinessLayer/`, `Repository/`, and `Services/` finds **no** permission check — every
`CanDelete…Async` match there is a referential-integrity guard, not authorization.

Under Blazor Server this is tolerable: the C# runs on the server and a user can only invoke
a service through a page the server rendered for them.

Under a REST API it is not. `CurrencyController` carries a bare `[Authorize]`. Concretely,
today: **a user whose `UserRight` row for `Currency` has all rights `false` — for whom the
Blazor UI hides the screen entirely — can create, edit, and delete currencies through the
API.** Every controller built on that template inherits the hole.

## Decision

**Permission enforcement moves to the server. Client-side checks remain, but only as UX
affordances.**

### 1. Authorization filter

An ASP.NET Core authorization filter resolves the caller's `UserRight` for the screen a
controller declares, and the right an action requires:

```csharp
[Authorize]
[RequireScreen("Sales Order")]                       // controller declares the screen
public class SalesOrdersController : ControllerBase
{
    [HttpGet]                  [RequireRight(Right.View)]   …
    [HttpPost]                 [RequireRight(Right.Create)] …
    [HttpPut("{id:int}")]      [RequireRight(Right.Edit)]   …
    [HttpDelete("{id:int}")]   [RequireRight(Right.Delete)] …
    [HttpPost("{id:int}/cancel")] [RequireRight(Right.Edit)] …
}
```

Semantics preserved from `RightsHelper`:
- **Deny by default** — a missing `UserRight` row means no right (`?? false`).
- `IsHide` hides the screen from navigation; it does not, on its own, grant or revoke
  operations.

### 2. Rights are resolved per request from the database, cached briefly

Rights are **not** embedded in the JWT. A JWT lives up to 8 hours; a permission change must
take effect sooner. Rights are loaded per request via
`GetUserRightsWithScreensAsync(userId)` with a short (≈60 s) per-user memory cache,
invalidated when `UserRight` rows are written.

### 3. `GET /api/v1/me` returns the full right set for rendering

The client receives user, tenant, role, and the complete screen-rights map at login, so it
can filter navigation and gate controls. **This is presentation only.** The server
re-checks independently on every request.

### 4. Approval authority is enforced server-side too

`UserAuthority` (12 document-type × level pairs) is checked inside the approval endpoints,
not only in the `/approval` page.

### 5. Tenant isolation is an authorization concern

Every request resolves its tenant from the JWT `TenantId` claim (ADR-002). Because the
tenant determines the connection string, cross-tenant access is structurally impossible —
but `{id}` route parameters must still be validated against the resolved tenant's data to
prevent IDOR within a tenant. Covered by the mandatory permission test suite.

### 6. Testing is a merge gate

An automated suite exercises **every endpoint × every right combination**, asserting 403
where the right is absent. No controller merges without its row in that suite.

## Consequences

**Positive.** Closes a complete authorization bypass before it reaches production. The
permission model itself (`UserRight` × `Screens`, 152 screens × 5 rights) is unchanged, so
existing tenant configuration keeps working with no data migration. The same enforcement
serves any future client (mobile, integrations).

**Negative.** Every endpoint needs correct screen and right annotations — a per-controller
correctness obligation, mitigated by the mandatory test suite. Screen names are free-text
strings matched against the seeded `Screens` table; a typo silently denies. Mitigation:
generate a `ScreenNames` constants class from the seed data and forbid string literals in
attributes (analyzer rule). One extra query per request, mitigated by the short cache.

**Neutral.** The Blazor UI keeps its `BaseUserRightsComponent` checks unchanged during the
strangler period. The two mechanisms read the same tables and agree.

## Non-negotiable

No controller ships without `[RequireScreen]` + `[RequireRight]` and its permission-matrix
test. This is the one rule in this knowledge base with no exceptions.
