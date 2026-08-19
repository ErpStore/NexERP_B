---
doc_id: KB-105
title: Server-Side Screen-Right Authorization — Implementation Specification
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Shared/RightsHelper.cs
  - V.SMART/V.SMART.Shared/Shared/BaseUserRightsComponent.cs
  - V.SMART/V.SMART.Shared/Data/Master/Admin_Module/UserRight.cs
  - V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/Screens.cs
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRightsRepository.cs
  - V.SMART/V.SMART.Shared/Repository/IRepository/IMasterRepository/IAdmins/IUserRightsRepository.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserRightService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/HRMasterService/EmployeeService.cs
  - V.SMART/V.SMART.Shared/Services/CurrentUserService.cs
  - V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantProvider.cs
  - V.SMART/V.SMART.Shared/Migrations/ApplicationDbContextModelSnapshot.cs
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Api/Auth/JwtTokenService.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
entities: [UserRight, Screens, User]
api_endpoints: []
database_tables: [UserRights, Screens]
business_rules: [BR-AUTH-002]
status: proposal
confidence: n/a
last_verified: 2026-08-18
dependencies: [ADR-004, ADR-002, KB-013, KB-030, KB-060, KB-040]
---

# Server-Side Screen-Right Authorization — Implementation Specification

**Produced by task `M2-A01-01`.** This document turns [ADR-004](../decisions/ADR-004-server-side-authorization.md)
from a decision into a buildable design. It is the sole input to `M2-A01-02`
(implement `[RequireScreen]` / `[RequireRight]`) and `M2-A01-03` (per-request rights
resolution and caching).

**It produces no code.** Every type below is a specification, not an implementation.

> **A second reader should be able to implement `M2-A01-02` from this document alone.** If
> you find yourself needing to re-open `RightsHelper.cs` to answer a question, that is a
> defect in this document — record it rather than working around it.

---

## 0. Document status and scope

| Field | Value |
|---|---|
| Kind | **Proposal** — a plan for code that does not exist yet. It is not an as-is description. |
| Governing decision | [ADR-004](../decisions/ADR-004-server-side-authorization.md) (Accepted, P0 blocker) |
| Contract constraints | [ADR-002](../decisions/ADR-002-rest-api-layer.md) §2 (conventions), §4 (error contract), §6 (versioning) |
| As-is model reused, not re-derived | [KB-013](auth-and-permissions.md) (INV-004, Complete 2026-08-12) |
| As-is API surface reused | [KB-040](../api/api-overview.md) (INV-008, Complete 2026-08-12) |
| Implements | `BR-AUTH-002` ([KB-030](../business-rules/business-rule-inventory.md)) |
| Closes | `R-03` (API authorization bypass, [KB-060](../risks/technical-debt-register.md)) |
| Written under | A **deliberate G0 gate exception** — see §12 |

Every claim below is tagged **Confirmed** (traced to `file:line` observed in this session),
**Inferred** (reasoned, with the reasoning shown) or **Unknown** (recorded, never guessed),
per [KB-002](../source-of-truth-rules.md).

### Note on this document's `doc_id`

`tasks/M2-A01-01.md` § *Target Result* proposed `KB-016`, qualified with *"or the next free
`KB-0xx` … verify against `docs/kb/INDEX.md` before assigning"*. That verification was
performed and returns a different answer: [KB-005](../INDEX.md) § *doc_id allocation* states
that **"a task that produces a durable document allocates the next free KB-1xx id"**, and
reserves `KB-100 +` for *"artefacts produced by tasks — investigation outputs, `@code` triage
reports, **contract specs**, decision briefs"*. Sibling M2 tasks already follow this:
`KB-110`–`KB-113` are claimed by `M2-B08`…`M2-B11`, and `KB-100`/`KB-101` by `M2-B12-01/02`.
`KB-016` is genuinely unclaimed, but claiming it would put a task artefact inside the
analysis range against the allocation rule. **This document therefore takes `KB-105`, the
next free `KB-1xx`.** The `M2-A01-01` task file predates the `KB-1xx` convention
(`last_verified: 2026-08-12`); its instruction to verify against the INDEX is what produced
this outcome, so this is compliance with that instruction, not a deviation from it.

---

## 1. What is being reproduced

The existing system enforces permissions **only in the Blazor UI**. Reproducing it on the
server means reproducing five specific behaviours exactly. Getting any of them wrong yields
either a silent bypass (R-03 reopened) or a silent lockout across 152 screens.

### 1.1 The as-is evaluation path, end to end

```
Razor page (296 of 333)
  → BaseUserRightsComponent.LoadRightsAsync()          BaseUserRightsComponent.cs:29-40
      → CurrentUserService.GetUserIdAsync()            CurrentUserService.cs:50-66
      → IUnitOfWork.UserRights
          .GetUserRightsWithScreensAsync(userId)       UserRightsRepository.cs:22-28
            → ApplicationDbContext (tenant DB)
              Set<UserRight>().Include(r => r.Screens)
                              .Where(r => r.UserId == userId)
                              .ToListAsync()            ← MATERIALISES HERE
  → RightsHelper.HasXxxRight(userRights, ScreenName)   RightsHelper.cs:7-20
      → rights.FirstOrDefault(r => r.Screens.ScreenName == screenName)?.CanXxx ?? false
```

**The materialisation point is the single most important fact in this document.** The
repository calls `ToListAsync()` (`UserRightsRepository.cs:27`) *before* `RightsHelper` ever
runs, so the `ScreenName` comparison at `RightsHelper.cs:8` is **LINQ-to-Objects, not SQL**.
It is therefore an ordinal, case-sensitive, culture-insensitive `string.Equals` — never a
collation-dependent SQL comparison. **Confirmed.** Decision **D-1** turns on this.

### 1.2 Behaviours that must be preserved

| # | Behaviour | Evidence | Confidence |
|---|---|---|---|
| B-1 | **Deny by default.** A missing `UserRight` row for a screen means no right. | `RightsHelper.cs:7-20` — all five helpers terminate in `?? false` | **Confirmed** |
| B-2 | Five rights per `(user, screen)`: `CanView`, `CanCreate`, `CanEdit`, `CanDelete`, `IsHide`. | `UserRight.cs:25-33` | **Confirmed** |
| B-3 | Screens are matched by the free-text `Screens.ScreenName`, never by `Id` or `ScreenCode`. | `RightsHelper.cs:8,11,14,17,20` | **Confirmed** |
| B-4 | The **first** matching row wins (`FirstOrDefault`); duplicates resolve by query order. | `RightsHelper.cs:8,11,14,17,20` | **Confirmed** |
| B-5 | `IsHide` is read through a **separate** helper and does **not** gate the four operations. | `RightsHelper.cs:19-20` read only by `BaseUserRightsComponent.cs:27`, independently of `:23-26` | **Confirmed** |
| B-6 | Rights are loaded for the current user only, `Screens` eager-loaded, from the **tenant** database. | `UserRightsRepository.cs:22-28`; `_db` is `ApplicationDbContext` (`:12,15-19`) | **Confirmed** |
| B-7 | The rights query applies **no `OrderBy`**, so "first" under B-4 is whatever the server returns. | `UserRightsRepository.cs:24-27` — no ordering clause | **Confirmed** |
| B-8 | No permission check exists anywhere in `BusinessLayer/`, `Repository/` or `Services/`. | INV-004 → [KB-013](auth-and-permissions.md); ADR-004 *Context* | **Confirmed** (reused, not re-derived) |

### 1.3 The screen catalogue

| Fact | Evidence | Confidence |
|---|---|---|
| Exactly **152** `Screens` rows are seeded. | `ApplicationDbContext.cs:1151` `HasData(` — 152 `new Screens` initialisers follow | **Confirmed** |
| All 152 `ScreenName` values are **unique**, and remain unique under case-insensitive comparison. | Extracted and de-duplicated in this session; zero collisions either way | **Confirmed** |
| `Id == ScreenCode` for **all 152** rows. | Extracted and compared in this session; zero mismatches | **Confirmed** |
| No `ScreenName` carries leading or trailing whitespace. | Checked in this session; none found | **Confirmed** |
| **No code path anywhere writes a `Screens` row.** The catalogue is seed-only at runtime. | Grepped `Screens.CreateAsync\|CreateRangeAsync\|UpdateAsync\|DeleteAsync` and `new Screens`/`Screens.Add` across `V.SMART/` excluding migrations and the seed — the only hit, `CommonService.cs:1388`, is a `Select` projection, not an insert | **Confirmed (negative result)** |
| A "Screen Management" screen exists in the catalogue (`Id = 17`) but no writer backs it. | Row present at `ApplicationDbContext.cs:1168`; see the negative result above | **Confirmed** |

The full list is **Appendix A**. It is the exact set of strings `[RequireScreen]` may carry.

---

## 2. Types to create

All under `V.SMART/V.SMART.Api/`. Nothing in `V.SMART.Shared` changes — the domain library is
shared with the Blazor and MAUI hosts, which must keep their existing behaviour unchanged
during the strangler period (ADR-004 *Consequences*, "Neutral").

| Type | File | Namespace |
|---|---|---|
| `Right` (enum) | `Authorization/Right.cs` | `V.SMART.Api.Authorization` |
| `RequireScreenAttribute` | `Authorization/RequireScreenAttribute.cs` | `V.SMART.Api.Authorization` |
| `RequireRightAttribute` | `Authorization/RequireRightAttribute.cs` | `V.SMART.Api.Authorization` |
| `NoScreenRightAttribute` | `Authorization/NoScreenRightAttribute.cs` | `V.SMART.Api.Authorization` |
| `ScreenRightAuthorizationFilter` | `Authorization/ScreenRightAuthorizationFilter.cs` | `V.SMART.Api.Authorization` |
| `IUserRightsProvider` | `Authorization/IUserRightsProvider.cs` | `V.SMART.Api.Authorization` |
| `UserRightsProvider` | `Authorization/UserRightsProvider.cs` | `V.SMART.Api.Authorization` |
| `ScreenRightSet` | `Authorization/ScreenRightSet.cs` | `V.SMART.Api.Authorization` |
| `ScreenCatalogue` | `Authorization/ScreenCatalogue.cs` | `V.SMART.Api.Authorization` |
| `ScreenRightStartupValidator` | `Authorization/ScreenRightStartupValidator.cs` | `V.SMART.Api.Authorization` |

### 2.1 `Right`

```csharp
public enum Right
{
    View,
    Create,
    Edit,
    Delete
}
```

**Four members, not five.** `IsHide` is deliberately absent: per **B-5** it is a navigation
affordance, never an operation gate, so it must not be expressible as a required right.

### 2.2 `RequireScreenAttribute`

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequireScreenAttribute : Attribute
{
    public RequireScreenAttribute(string screenName);
    public string ScreenName { get; }

    /// <summary>
    /// Set to false only for a screen that is deliberately absent from the seeded
    /// catalogue. Requires written justification at review — see D-6.
    /// </summary>
    public bool Seeded { get; init; } = true;
}
```

`AllowMultiple = false`: a controller declares exactly one screen. A controller that needs
two screens is two controllers.

### 2.3 `RequireRightAttribute`

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireRightAttribute : Attribute
{
    public RequireRightAttribute(Right right);
    public Right Right { get; }
}
```

`AllowMultiple = false`: one action requires exactly one right. Two required rights on one
action would need an AND/OR rule that `RightsHelper` has no equivalent for, so it is not
expressible (see **D-4**).

### 2.4 `NoScreenRightAttribute`

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class NoScreenRightAttribute : Attribute
{
    public NoScreenRightAttribute(string justification);
    public string Justification { get; }
}
```

The **explicit, auditable** opt-out for an authenticated endpoint that legitimately has no
screen. It exists because `GET /api/v1/me` (`M2-A07`) must be reachable by every
authenticated user regardless of screen rights — the client cannot render navigation without
it, and gating it on a screen right would deadlock login. The mandatory `justification`
argument means every opt-out is self-documenting and greppable. **Not** a substitute for
`[AllowAnonymous]`: `[NoScreenRight]` still requires authentication.

### 2.5 `ScreenRightSet`

```csharp
public sealed record ScreenRightEntry(
    string ScreenName,
    bool CanView,
    bool CanCreate,
    bool CanEdit,
    bool CanDelete,
    bool IsHide);

public sealed class ScreenRightSet
{
    public IReadOnlyList<ScreenRightEntry> Entries { get; }

    /// <summary>Ordinal, first-match-wins. Mirrors RightsHelper.cs:7-20 exactly.</summary>
    public bool Has(string screenName, Right right);

    /// <summary>Number of entries matching screenName. Used only for the D-2 warning.</summary>
    public int MatchCount(string screenName);
}
```

An **immutable projection**, never EF-tracked `UserRight` entities. Caching tracked entities
whose `DbContext` is scoped per request is a lifetime bug: the cached graph would outlive
its context and either throw on lazy access or hand a later request another request's
tracker state. `ScreenRightSet` is a detached snapshot with no EF dependency.

`Entries` preserves the order the repository returned, so **B-4** and **B-7** are reproduced
including their non-determinism (see **D-2**).

### 2.6 `IUserRightsProvider` — the caching seam

```csharp
public interface IUserRightsProvider
{
    Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct);
}
```

`M2-A01-02` implements this by calling
`IUnitOfWork.UserRights.GetUserRightsWithScreensAsync(userId)` directly — **no cache**.
`M2-A01-03` adds caching **behind this interface only** (§8). The filter never changes
between the two tasks. `tenantId` is a parameter, not ambient, so the cache key cannot
accidentally omit it (§8.1).

### 2.7 `ScreenCatalogue`

```csharp
public static class ScreenCatalogue
{
    /// <summary>The 152 seeded ScreenName values. Ordinal set.</summary>
    public static IReadOnlySet<string> SeededScreenNames { get; }
}
```

A compile-time copy of Appendix A, used **only** by `ScreenRightStartupValidator` (**D-6**).
It is never consulted at request time — the authorization decision always comes from the
tenant database, never from this list. `M2-B05` replaces this hand-copied set with a
generated one (R-10); until then, Appendix A and this type must be updated together.

### 2.8 `ScreenRightAuthorizationFilter`

```csharp
public sealed class ScreenRightAuthorizationFilter : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context);
}
```

Registered globally (§6). Evaluation order, in full:

1. If the endpoint allows anonymous access → **return, no decision**. Authentication is
   `[Authorize]`'s job, not this filter's.
2. If the endpoint or its controller carries `[NoScreenRight]` → **return, no decision**.
3. Resolve `UserId` from the `"UserId"` claim. Missing, unparseable, or `<= 0` → **401**
   (**D-3**).
4. Resolve `TenantId` from the `"TenantId"` claim. Missing, unparseable, or `<= 0` → **401**
   (**D-3**).
5. Read `[RequireScreen]` from the controller and `[RequireRight]` from the action. Either
   absent → **403** (**D-4**).
6. `var rights = await provider.GetAsync(tenantId, userId, ct);`
7. If `rights.MatchCount(screen) > 1` → log a warning (**D-2**). Do not change the outcome.
8. `if (!rights.Has(screen, right))` → **403** with the §7 body.
9. Otherwise → **return, no decision.** Allow.

Steps 1 and 2 return without setting `context.Result`, which lets other filters and the
endpoint proceed. Steps 3–8 short-circuit by assigning `context.Result`.

---

## 3. Truth table

One row per case. Every row cites the `RightsHelper` line it mirrors. `screen` is the
controller's `[RequireScreen]` value; `right` is the action's `[RequireRight]` value.

| # | Case | `RightsHelper` today | Filter must | Evidence |
|---|---|---|---|---|
| T-1 | No `UserRight` row matches `screen` | `FirstOrDefault` → `null` → `?? false` | **403** | `RightsHelper.cs:7-20` |
| T-2 | Row matches, required flag is `false` | `false` | **403** | `RightsHelper.cs:8,11,14,17` |
| T-3 | Row matches, required flag is `true` | `true` | **allow** | `RightsHelper.cs:8,11,14,17` |
| T-4 | Row matches, `IsHide == true`, required flag `true` | Operation **allowed** — `IsHidden` is a separate helper read independently | **allow** | `RightsHelper.cs:19-20`; `BaseUserRightsComponent.cs:23-27` |
| T-5 | `screen` is not present in the `Screens` table at all | No row matches → `?? false` | **403** | `RightsHelper.cs:7-20` |
| T-6 | User has **zero** `UserRight` rows | Empty list → `FirstOrDefault` → `null` → `?? false` | **403** on every annotated endpoint | `RightsHelper.cs:7-20`; `UserRightsRepository.cs:22-28` |
| T-7 | Two rows match `screen`, first grants, second denies | First wins → **granted** | **allow**, and log the ambiguity | `RightsHelper.cs:8` (`FirstOrDefault`); **D-2** |
| T-8 | Two rows match `screen`, first denies, second grants | First wins → **denied** | **403**, and log the ambiguity | `RightsHelper.cs:8` (`FirstOrDefault`); **D-2** |
| T-9 | `screen` differs from the seeded name only by case (`"currency"`) | Ordinal comparison in memory → no match → `?? false` | **403** | `RightsHelper.cs:8` + materialisation at `UserRightsRepository.cs:27`; **D-1** |
| T-10 | `UserId` claim missing / unparseable / `<= 0` | *No equivalent* — Blazor falls back to `0` and denies incidentally | **401**, not 403 — a stated divergence | `CurrentUserService.cs:59-65`; **D-3** |
| T-11 | Action has `[RequireRight]`, controller has no `[RequireScreen]` | *No equivalent* | **403** at request time, **and startup throws** | **D-4** |
| T-12 | Controller has `[RequireScreen]`, action has no `[RequireRight]` and no `[NoScreenRight]` | *No equivalent* | **403** at request time, **and startup throws** | **D-4** |
| T-13 | Caller's role is `Administrator`, no `UserRight` row for `screen` | No bypass exists — `RightsHelper` never reads a role | **403** | `RightsHelper.cs:7-20`; `UserRole.cs:3-7`; **D-5** |

**T-4 is the row most likely to be implemented wrongly.** The instinct is that a hidden
screen should refuse operations. It must not: `BaseUserRightsComponent.cs:23-27` exposes
`IsHidden` as a sibling of the four operation properties, and no page consulted in INV-004
combines it into an operation check. `IsHide` is presentation. Treating it as a gate would
lock users out of screens they can currently use through the UI.

---

## 4. The eight decisions

Each is numbered, decided, and justified. None is deferred.

### D-1 — Screen-name comparison is **ordinal and case-sensitive**, evaluated **in memory**

**Decision.** The provider materialises the user's rights with the existing
`GetUserRightsWithScreensAsync(userId)` query and the filter matches `ScreenName` in memory
using `StringComparison.Ordinal`. **The screen name is never placed in a SQL predicate.**

**Justification.**

1. *It is what happens today.* `UserRightsRepository.cs:27` calls `ToListAsync()` before
   `RightsHelper.cs:8` runs, so today's comparison is LINQ-to-Objects — ordinal,
   case-sensitive. **Confirmed.** Any SQL-side comparison would instead obey the column
   collation and, under SQL Server's common `*_CI_AS` default, would match case-insensitively
   — silently *widening* access relative to Blazor. That is the R-03 direction.
2. *The column cannot be indexed anyway.* `Screens.ScreenName` is `nvarchar(max)`
   (`ApplicationDbContextModelSnapshot.cs:9141-9143`; `InitialCreate.cs:569`). SQL Server
   cannot use a `max` type as an index key, so a SQL-side predicate buys a scan and no index.
   There is no performance argument for pushing it down. **Confirmed.**
3. *The set is small.* At most 152 rows per user (§1.3), bounded by the seeded catalogue.
   In-memory matching over ≤152 entries is not a cost worth engineering around.

**Residual risk — `Unknown`.** The tenant databases' collation is not in source control: no
`HasCollation`/`UseCollation` appears in `ApplicationDbContext.cs` or the initial migration
(grepped, none found — **Confirmed negative result**), so each database inherits whatever
default it was created with. This is *why* D-1 is the safe choice rather than a problem for
it: because no comparison reaches SQL, collation cannot influence the authorization outcome.
It remains relevant to `M2-A01-02`'s tests, which must not assume a collation.

**Consequence to state in the controller template (`M2-B03`).** `[RequireScreen("...")]`
strings must match Appendix A **byte for byte**, including the seed's own typos — `Id = 82`
is `"Sub-Contrect GRN"`, not `"Sub-Contract GRN"`. **D-6**'s startup check exists precisely
to make that class of error loud.

### D-2 — Duplicate rows: **mirror `FirstOrDefault`, and log the ambiguity**

**Decision.** When more than one `UserRight` row matches the screen, use the first in the
order the repository returned, exactly as `RightsHelper.cs:8` does. Emit a warning-level log
entry naming tenant, user and screen. **Do not** change the outcome, and **do not** deny on
ambiguity.

**Justification.**

- ADR-004 requires preserved semantics; `FirstOrDefault` is the semantic.
- *Deny-on-ambiguity* would be **stricter** than Blazor: the UI would show the screen and
  the API would refuse it. A divergence that produces support tickets and, worse, teaches
  people that 403s are noise.
- *Grant-if-any-row-grants* would be **looser** than Blazor — the R-03 direction. Rejected.
- Logging is not a behaviour change, and it converts an invisible data condition into an
  operational signal.

**The condition is possible, not proven.**

| Fact | Evidence | Confidence |
|---|---|---|
| No unique constraint or alternate key on `UserRight` in the EF model. | `ApplicationDbContextModelSnapshot.cs:4676-4680` — `HasIndex("ScreenId")` and `HasIndex("UserId")`, **neither unique**; no `HasKey` beyond `Id` | **Confirmed** |
| No `HasIndex`/`HasAlternateKey` for `UserRight` or `Screens` in `ApplicationDbContext.cs`. | Grepped: 5 occurrences total, all on `MfgQuote` (`:581`), history (`:586,589`) and assembly (`:594,617`) | **Confirmed (negative result)** |
| The deployed indexes are EF's automatic FK indexes, non-unique. | `InitialCreate.cs:9796-9803` — `IX_UserRights_ScreenCode`, `IX_UserRights_UserId` (the column was later renamed to `ScreenId`; current model at snapshot `:4676-4680`) | **Confirmed** |
| `SyncRightsForUserAsync` **cannot** create a duplicate — it excludes existing `ScreenId`s. | `UserRightService.cs:47-54` | **Confirmed** |
| `UserRights.razor:462` and `UserService.cs:464` create rows with no equivalent visible guard. | Both call `CreateAsync` inside a loop over screens; `UserRights.razor:446` updates when a row is found, `:462` creates otherwise | **Confirmed** |
| Whether duplicates actually exist in live tenant databases. | Not determinable from the repository — no database access in this session | **Unknown** — see **Q-27** |
| Duplicates cannot arise from the catalogue itself: all 152 seeded names are unique, and no code writes `Screens`. | §1.3 | **Confirmed** |

**Non-determinism is inherited deliberately.** Per **B-7**, `GetUserRightsWithScreensAsync`
applies no `OrderBy` (`UserRightsRepository.cs:24-27`), so "first" is whatever SQL Server
returns — which is not guaranteed stable across plans. Today's Blazor behaviour under
duplicates is therefore already non-deterministic, and the filter reproduces that faithfully
rather than papering over it with an invented ordering. **Adding an `OrderBy` would be a
behaviour change, and adding a unique index would be a schema change — both are out of scope
for M2-A01** (`CLAUDE.md`: do not change the database schema unless the task authorises it).
Raised as **Q-27**.

### D-3 — Missing or unusable identity claims return **401**, never 403, and never fall back to `0`

**Decision.** After `[Authorize]` has passed, the filter reads `"UserId"` and `"TenantId"`.
If either is absent, unparseable as `int`, or `<= 0`, the filter returns **401** with the
§7.2 body and does not query rights.

**Justification.**

- ADR-002 §4 assigns 401 to "unauthenticated / token expired" and 403 to "screen right or
  approval authority denied". A token that carries no usable subject is a malformed
  credential, not a permission outcome. Returning 403 would tell the caller their *rights*
  are wrong when their *token* is wrong — actively misleading, and it would pollute the
  `M2-A03` permission matrix with rows that are really auth failures.
- **This is a deliberate divergence from `CurrentUserService`.**
  `CurrentUserService.GetUserIdAsync():59-65` returns **`0`** when the claim is missing or
  unparseable (**Confirmed**). In Blazor that fails safe only by accident: user `0` owns no
  rows, so the list is empty and everything denies. The filter must not inherit that
  accident. A silent `0` would query rights for a non-existent user on every request and
  would grant real access the moment any tenant ever holds a `UserId` of `0`.
- The `<= 0` guard, not merely "parses as int", is what makes the divergence total.

**In practice this should be unreachable.** `JwtTokenService.CreateToken` always writes both
claims (`JwtTokenService.cs:31-35`, **Confirmed**), and the bearer options validate issuer,
audience, lifetime and signing key (`Program.cs:70-80`, **Confirmed**). The guard exists
because "should be unreachable" is not "is unreachable", and because the alternative failure
mode is silent.

**`M2-A01-02` must not call `CurrentUserService` for this.** It is registered in the API
(`Program.cs:104`) and would reintroduce the `0` fallback. Read the claims directly from
`context.HttpContext.User`.

### D-4 — Missing annotations **fail closed at request time and fail loudly at startup**

**Decision.** Both directions are errors:

- an action with `[RequireRight]` on a controller with no `[RequireScreen]`;
- an action with neither `[RequireRight]` nor `[NoScreenRight]`, on a controller that
  requires authentication.

At **request time** both produce **403** (§7.1). At **startup** both throw
`InvalidOperationException` naming every offending `Controller.Action`, listed together in
one message rather than one-at-a-time.

**Justification.**

- ADR-004's *Non-negotiable*: *"No controller ships without `[RequireScreen]` +
  `[RequireRight]` and its permission-matrix test."* A forgotten annotation is exactly how
  R-03 comes back, and it is silent — the endpoint simply works.
- Startup is the right place because the fault is static: it is a property of the assembly,
  not of any request. Catching it at request time only tells you after someone has been
  denied.
- **The house pattern already exists.** `Program.cs:25` calls
  `StartupConfigurationValidator.Validate(...)` before anything consumes a secret, throwing
  `InvalidOperationException` naming the key and the remediation (M0-03-03, **Confirmed**).
  `ScreenRightStartupValidator` is the same shape applied to annotations.
- Request-time 403 is kept as well, rather than trusting startup alone, because dynamically
  registered application parts and any future plugin controller would bypass the startup
  sweep. Two independent fail-closed paths, not one.

**Scope of the startup sweep.** Every controller in the API's application parts, excluding
actions and controllers marked `[AllowAnonymous]` (so `AuthController.Login`,
`AuthController.cs:39-41`, is exempt — **Confirmed**) and those marked `[NoScreenRight]`.

**One right per action, deliberately.** `RequireRightAttribute` sets `AllowMultiple = false`
because `RightsHelper` offers no combining rule, so any AND/OR semantics would be invented
rather than preserved. An operation genuinely needing two rights is a design question for
`M2-B03`, not something to smuggle in through attribute multiplicity.

### D-5 — **No `Administrator` bypass.** None. Anywhere.

**Decision.** The filter never reads `ClaimTypes.Role`. An `Administrator` with no
`UserRight` row for a screen receives **403**, exactly as any other user does (**T-13**).

**Justification.**

- ADR-004 grants no bypass, and this specification is not the place to invent one.
- `RightsHelper.cs:7-20` contains no role check of any kind. **Confirmed.**
- `UserRole.cs:3-7` holds only `Administrator` and `User`. **Confirmed.**
- The existing mechanism for "the administrator can do everything" is **data, not code**:
  `Login.razor:345-349` detects `user.UserId == 1` and calls
  `UserRightService.SyncRightsForUserAsync`, which *inserts rows* with
  `CanView = CanCreate = CanEdit = CanDelete = true` for every screen the user is missing
  (`UserRightService.cs:62-75`). **Confirmed.** It grants by writing rows, never by skipping
  the check. A code bypass would be a genuinely new privilege path with no counterpart in
  Blazor — and, being outside the filter, it would be invisible to the `M2-A03`
  permission-matrix suite, which is the one control ADR-004 relies on.

**Flagged for review — this decision has a real operational consequence.** The rights-seeding
that makes an administrator work today runs **only on the Blazor login path**.
`AuthController.Login` (`AuthController.cs:39-59`) does **not** call
`SyncRightsForUserAsync` — **Confirmed**. So a user, including `UserId == 1`, who has only
ever authenticated through the API can hold **zero** `UserRight` rows and will receive 403
from every annotated endpoint (**T-6**). Three further facts sharpen this:

| Fact | Evidence | Confidence |
|---|---|---|
| The Blazor sync is gated on `user.UserId == 1` — no other user is ever auto-granted. | `Login.razor:345-349` | **Confirmed** |
| New users created through `UserService` get rights rows **only when `vm.IsViewOnly` is true**, and then with `CanView = true` and every other flag `false`. | `UserService.cs:442-464` | **Confirmed** |
| So the two seeding paths use **opposite** defaults: `SyncRightsForUserAsync` grants everything (`UserRightService.cs:67-71`), `UserService` grants view-only — and for a non-`IsViewOnly` user, nothing at all. | Both cited above | **Confirmed** |

**This is not M2-A01-01's to fix.** It is a pre-existing inconsistency in the legacy seeding
logic that server-side enforcement makes *visible* rather than creates. It must be settled
before `M2-A02` applies the filter to `CurrencyController`, or the first thing the vertical
slice will demonstrate is an administrator locked out of Currency. Raised as **Q-28** and
flagged to `M2-A07` (`GET /api/v1/me`), which is where an empty right set will first be
observable to the client.

### D-6 — Screen names stay **string literals** for now, made safe by a startup check

**Decision.** `[RequireScreen("Currency")]` carries a literal for `M2-A01-02`, `M2-A02` and
any controller landing before `M2-B05`. `ScreenRightStartupValidator` additionally verifies
that **every declared screen name exists in `ScreenCatalogue.SeededScreenNames`** (Appendix A,
ordinal comparison) and throws naming the controller and the offending string otherwise.
`Seeded = false` on the attribute is the only exemption, and requires written justification
at review.

**Justification.**

- ADR-004's *Consequences* calls for a generated `ScreenNames` constants class and an
  analyzer forbidding literals; **`M2-B05` owns that** (R-10). Building it here would be
  scope theft and would duplicate work `M2-B05` is already specified to do.
- ADR-004 names the real hazard: *"a typo silently denies."* The startup check converts that
  from silent to fatal, at essentially zero cost — the reflection sweep for **D-4** is
  already walking every action.
- Validating against the **compiled** catalogue rather than the database is forced, not
  chosen: `Screens` lives in the **tenant** database (`UserRightsRepository.cs:12`), which is
  resolved per request from the JWT (`TenantProvider.cs:33-44`, **Confirmed**). There is no
  tenant context at startup, and there may be many tenants. A compile-time list is the only
  thing available in `Program.cs`.
- Throwing rather than warning on an unseeded name is safe **because no code path writes
  `Screens`** (§1.3, Confirmed negative result), so the seeded 152 is the whole catalogue
  every tenant has. `Seeded = false` exists for the day that stops being true; it must not be
  used to silence a typo.

**Migration path.** `M2-B05` generates `ScreenNames` from the same seed data, replaces the
literals, and takes over `ScreenCatalogue`. **The startup check stays** — defence in depth
against a constant that drifts from the seed. Until then, Appendix A and `ScreenCatalogue`
are updated in the same change or not at all.

### D-7 — The `403` body is `application/problem+json` naming the screen and the right

Specified in full in **§7**. Summary of the decision: RFC 7807, `Content-Type:
application/problem+json`, extension members `screen` and `right`, and a `traceId`.
**Denials are indistinguishable from one another** — T-1, T-2, T-5 and T-13 all produce a
byte-identical body for the same screen and right. That is deliberate: the response must not
disclose whether a screen exists, and it happens to be exactly what `RightsHelper` does,
since all four cases collapse to the same `?? false`.

### D-8 — Cache: keyed by **tenant and user**, 60 s **absolute**, TTL is the real bound

Specified in full in **§8**. Summary of the decision: key
`"screenrights:v1:{tenantId}:{userId}"`, 60-second **absolute** expiration, value a
`ScreenRightSet`, explicit invalidation on API-side writes — of which there are currently
none. The finding that shapes it: **three of the five `UserRight` write sites are in the
Blazor host, a different process**, so an in-process cache in the API cannot be invalidated
by them and the TTL is the only real staleness bound.

---

## 5. Non-goals

Explicitly **out of scope** for this specification and for `M2-A01-*`:

| Not doing | Why | Owner |
|---|---|---|
| Any change to the `Screens` or `UserRight` tables, entities or seed data | ADR-004 *Consequences*: the permission model is unchanged so existing tenant configuration keeps working with no data migration | — |
| Adding a unique index on `(UserId, ScreenId)` | Schema change; `CLAUDE.md` forbids it without explicit task authorisation | **Q-27** |
| Adding an `OrderBy` to `GetUserRightsWithScreensAsync` | Behaviour change to a method the Blazor host also calls (`BaseUserRightsComponent.cs:34`) | **Q-27** |
| Embedding rights in the JWT | **ADR-004 §2 forbids it.** A JWT lives up to 8 hours (`JwtTokenService.cs:37` defaults `Jwt:ExpiresMinutes` to 480, **Confirmed**); a permission change must take effect sooner | — |
| `UserAuthority` approval-authority checks (12 document-type × level pairs) | ADR-004 §4 — a separate mechanism | `M2-B08`, `M3-4` |
| Row-level scoping (`StateCodesCsv`), account gates, `QrExpiryDate`, `TrialDays`, device binding | Q-05…Q-08 | `M2-A08` |
| Generated `ScreenNames` constants and the literal-forbidding analyzer | R-10 | `M2-B05` |
| The permission-matrix test harness and its merge gate | ADR-004 §6 | `M2-A03` |
| `GET /api/v1/me` and its rights payload | ADR-004 §3 | `M2-A07` |
| Any change to `BaseUserRightsComponent`, `RightsHelper`, or Blazor behaviour | ADR-004 *Consequences* "Neutral": the Blazor UI keeps its checks unchanged during the strangler period | — |
| Refresh tokens, revocation, CORS, cross-origin tenant resolution | — | `M2-A04`, `M2-A05` |

---

## 6. Pipeline placement and DI registration

**Line numbers in `V.SMART/V.SMART.Api/Program.cs` are current as of 2026-08-18 and drift.**
`tasks/M2-A01-01.md` cites `UseAuthentication()` at `:114` and `UseAuthorization()` at `:115`;
they are now at **`:121`** and **`:122`** (**Confirmed**, re-read this session). `M2-A01-02`
must re-read the file rather than trust either number — this is the shared composition root
that several tasks touch.

### 6.1 Ordering

```
app.UseCors("AngularDev");        // Program.cs:120  (M2-A05 replaces the policy)
app.UseAuthentication();          // Program.cs:121  ← identity established here
app.UseAuthorization();           // Program.cs:122
app.MapControllers();             // Program.cs:123  ← filter runs inside MVC, per endpoint
```

`ScreenRightAuthorizationFilter` is an **MVC filter, not middleware**. It must run after
`UseAuthentication()` has populated `HttpContext.User` — which it necessarily does, since MVC
filters execute during endpoint invocation, downstream of both calls. **No change to the
middleware order in `Program.cs` is required.** This matters: `M2-A06` (exception middleware)
and `M2-A05` (real CORS) both edit this region, and an unnecessary reordering here would
collide with them.

### 6.2 Registration

Global registration, so an unannotated controller is still swept by **D-4** rather than
silently skipped:

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ScreenRightAuthorizationFilter>();
});
```

This replaces the bare `builder.Services.AddControllers();` at `Program.cs:27`.

Service lifetimes:

| Service | Lifetime | Why |
|---|---|---|
| `ScreenRightAuthorizationFilter` | Scoped | Resolves `IUserRightsProvider`, which reaches a scoped `ApplicationDbContext` |
| `IUserRightsProvider` → `UserRightsProvider` | Scoped | `IUnitOfWork` is scoped (`Program.cs:107`) |
| `IMemoryCache` (added by `M2-A01-03`) | Singleton | Must outlive the request to be a cache at all — §8 |

**`M2-B07` owns where these lines live.** The shared `AddVSmartDomain()` extension is a hard
prerequisite for every controller because `V.SMART.Api/Program.cs` registers only
`ICurrencyService` (`:109`) while `V.SMART.Web` registers 242 services
([KB-080 §9](../execution/README.md#9-m2--foundation)). The authorization registrations
belong **alongside** `AddVSmartDomain()`, in a sibling `AddVSmartApiAuthorization()`
extension — authorization is API-host policy, not shared domain, and must not be pulled into
`V.SMART.Shared` where the Blazor and MAUI hosts would also receive it.

`M2-A01-02` may land before `M2-B07` by writing the registrations inline in `Program.cs`;
`M2-B07` then moves them. It must not wait, and it must not create a second DI extension in
`V.SMART.Shared`.

### 6.3 Claims available to the filter

From `JwtTokenService.CreateToken` (`JwtTokenService.cs:29-35`, **Confirmed**):

| Claim type | Source | Filter's use |
|---|---|---|
| `ClaimTypes.Name` | `user.UserName` | None — logging only |
| `"UserId"` | `user.UserId` | **Required** — the rights lookup key (**D-3**) |
| `"TenantId"` | `tenantId` | **Required** — the cache key's tenant component (**D-3**, §8.1) |
| `ClaimTypes.Role` | `user.Role?.ToString() ?? ""` | **Deliberately unused** (**D-5**) |

`TenantProvider` reads the same `"TenantId"` claim to resolve the tenant and therefore the
connection string (`TenantProvider.cs:33-44`, **Confirmed**). The filter reads the claim
directly rather than calling `ITenantProvider`, so the cache key cannot depend on tenant
resolution succeeding — but the two always agree, because they read the same claim.

---

## 7. Response bodies

Per ADR-002 §4, `application/problem+json` (RFC 7807) everywhere.

### 7.1 `403` — screen right denied

```json
{
  "type": "https://api.v-smart.local/problems/screen-right-denied",
  "title": "Screen right denied.",
  "status": 403,
  "detail": "You do not have the 'Edit' right for the 'Sales Order' screen.",
  "instance": "/api/v1/sales-orders/1042",
  "screen": "Sales Order",
  "right": "Edit",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

- `Content-Type: application/problem+json`.
- `screen` and `right` are RFC 7807 **extension members**, required by ADR-002 §4's "which
  screen + right".
- `right` is the `Right` enum name (`View`/`Create`/`Edit`/`Delete`), not an integer.
- `traceId` is `Activity.Current?.Id ?? HttpContext.TraceIdentifier`, matching ADR-002 §4's
  500 contract so correlation works across every status.
- **The body is identical for T-1, T-2, T-5 and T-13** (no row; row denies; screen does not
  exist; caller is an administrator). It must not become distinguishable — `detail` is
  composed from the *required* screen and right, never from what was found.
- `M2-A06` (`ProblemDetails` middleware) later owns the shared serialisation. Until it lands,
  `M2-A01-02` writes the body itself. Same shape either way; `M2-A06` must not change it.

### 7.2 `401` — unusable identity claim (**D-3**)

```json
{
  "type": "https://api.v-smart.local/problems/invalid-token",
  "title": "The access token is missing a required claim.",
  "status": 401,
  "detail": "The token does not carry a usable 'UserId' claim.",
  "instance": "/api/v1/sales-orders/1042",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

- **No `screen` or `right` member** — this is not a permission outcome (**D-3**).
- `detail` names which claim (`UserId` or `TenantId`) failed. That is safe: the claim *names*
  are public contract, and nothing about the caller's rights is disclosed.
- ADR-002 §4 lists no body for 401. Sending one is a deliberate, compatible extension — RFC
  7807 permits a body on any status, and a bare 401 here would be indistinguishable from an
  expired token, which is the single most confusing failure a client can get.

### 7.3 What the filter never returns

| Status | Why not |
|---|---|
| `404` in place of `403` | Resource-hiding is a defensible pattern but it is **not** what Blazor does, and it would make the `M2-A03` matrix untestable — the suite asserts 403 where a right is absent (ADR-004 §6) |
| `500` on a rights-query failure | An exception propagates to `M2-A06`'s handler. The filter never swallows one and never converts a fault into a denial — a database outage must not read as "no rights" |

---

## 8. Cache specification — input to `M2-A01-03`

ADR-004 §2: rights are loaded per request via `GetUserRightsWithScreensAsync(userId)` with a
short (≈60 s) per-user memory cache, invalidated when `UserRight` rows are written. This
section makes that buildable and records one finding ADR-004 does not account for.

### 8.1 Key

```
screenrights:v1:{tenantId}:{userId}
```

**The tenant component is mandatory and ADR-004 omits it.** The deployment is
database-per-tenant ([KB-014](multi-tenancy.md); `TenantDbContextFactory` resolves the
connection string per request, `Program.cs:87,92-96`, **Confirmed**), so `UserId = 7` in
tenant A and `UserId = 7` in tenant B are **different people with different rights**. A
per-user key alone would serve one tenant's rights to another tenant's user — a
cross-tenant authorization leak, and one that only appears under concurrent multi-tenant
load, which is exactly when it would not be noticed. `IUserRightsProvider.GetAsync` takes
`tenantId` as an explicit parameter (§2.6) so this cannot be forgotten.

The `v1` segment allows `ScreenRightSet`'s shape to change without a stale-entry hazard
across a rolling deploy.

### 8.2 Expiration

| Setting | Value | Why |
|---|---|---|
| Kind | **Absolute** | Sliding expiration would let an active user hold stale rights indefinitely, which defeats ADR-004 §2's entire rationale — that a permission change must take effect sooner than the 8-hour JWT |
| TTL | **60 seconds** | ADR-004 §2's "≈60 s". §8.4 explains why it must not be raised |
| Size limit | Entries counted, cap configurable | A tenant with many concurrent users must not grow the cache unbounded; ≤152 entries per user bounds each value |

Configurable via `Authorization:RightsCacheSeconds`, defaulting to `60`. A configured value
above 300 must fail startup — see §8.4.

### 8.3 Value

A `ScreenRightSet` (§2.5) — an immutable, detached projection. Never EF-tracked entities
(§2.5 explains the lifetime bug).

### 8.4 Invalidation — and the finding that limits it

Every site that writes a `UserRight` row, found by grepping `UserRights.` across `V.SMART/`:

| # | Site | Operation | Host process |
|---|---|---|---|
| W-1 | `UserRightService.cs:77` (`SyncRightsForUserAsync`) | `CreateRangeAsync` | Called from `Login.razor:348` → **Blazor** |
| W-2 | `UserService.cs:464` | `CreateAsync` (only when `vm.IsViewOnly`, `:442`) | **Blazor** |
| W-3 | `EmployeeService.cs:191` | `DeleteAsync`, in a loop over the user's rights | **Blazor** |
| W-4 | `EmployeeUpsert.razor:921` | `DeleteAsync` | **Blazor** |
| W-5 | `UserRights.razor:446` / `:462` | `UpdateAsync` / `CreateAsync` — the admin rights-editing screen | **Blazor** |

All **Confirmed**. Reads, for completeness: `BaseUserRightsComponent.cs:34`,
`UserRightService.cs:47`, `EmployeeService.cs:185`, `UserRights.razor:299`,
`EmployeeUpsert.razor:918`.

> **Finding — every one of the five write sites executes in the Blazor host, and none in the
> API.** `V.SMART.Web` and `V.SMART.Api` are separate processes
> ([KB-010](system-overview.md)). An in-process `IMemoryCache` in the API therefore **cannot
> be invalidated by any write that exists today**. Rights are edited through
> `UserRights.razor` — the Blazor admin screen — which the API will never observe.
> **Confidence: Confirmed** for the write-site inventory and their host; **Inferred** that no
> future API-side writer exists, since `M2-A01-*` adds none.

Consequences, which `M2-A01-03` must implement and not soften:

1. **The 60-second TTL is the only real staleness bound.** Explicit invalidation is
   infrastructure for writers that do not yet exist. Build it — `IUserRightsProvider` gains
   `void Invalidate(int tenantId, int userId)` — and call it from any future API-side write,
   but do not claim it bounds staleness today.
2. **The TTL must not be raised without a distributed cache.** Startup fails if
   `Authorization:RightsCacheSeconds` exceeds 300. Someone will eventually try to "reduce
   database load" by raising it; the guard makes that a conversation instead of a silent
   security regression.
3. **Document the 60-second window as product behaviour.** An administrator who revokes a
   right through the Blazor UI must be told it takes effect within a minute, not instantly.
4. **A distributed cache is the fix, and it is not M2-A01-03's.** Redis with pub/sub
   invalidation, or a `UserRights` change token both hosts poll, would close the gap. Neither
   is in M2's scope. Raised as **Q-29**.

### 8.5 What is not cached

The negative authorization decision is never cached — only the rights *set* is. Caching
`(screen, right) → denied` would multiply the invalidation surface by 152 × 4 for no
measurable gain over a ≤152-entry in-memory scan (**D-1**, justification 3).

---

## 9. Verification for `M2-A01-02`

Not tests to write here — the acceptance shape `M2-A01-02` must meet. There is no test
project yet (INV-023, **Confirmed**); `M0-12-01` creates the first at
`tests/V.SMART.Shared.Tests/` and is currently **Blocked**. `M2-A01-02` must state which of
these it could actually execute.

1. Every row of §3's truth table, T-1 through T-13, exercised against
   `ScreenRightAuthorizationFilter`.
2. T-9 (case divergence) asserted explicitly — it is the row that silently changes if anyone
   later pushes the comparison into SQL.
3. T-4 asserted explicitly — `IsHide == true` with the operation right `true` must **allow**.
4. The startup validator throws for both **D-4** directions and for an unseeded screen name
   (**D-6**), with the offending `Controller.Action` in the message.
5. A `[NoScreenRight]` endpoint is reachable by an authenticated user with zero rights rows.
6. `403` and `401` bodies match §7 byte for byte, including `Content-Type`.
7. Two different tenants with the same `UserId` do not share a cache entry (§8.1) — the one
   test that would have caught the omitted tenant key.

---

## 10. Findings raised by this task

Recorded here because they were discovered while specifying, and because a finding that
lives only in a chat message is lost ([KB-088 §4](../execution/workflow.md)).

| # | Finding | Confidence | Consequence |
|---|---|---|---|
| F-1 | `Screens.ScreenName` is `nvarchar(max)` — unindexable as a SQL Server key column, and no unique constraint is possible on it as declared. | **Confirmed** — snapshot `:9141-9143`, `InitialCreate.cs:569` | Reinforces **D-1**; removes any performance argument for a SQL-side name match |
| F-2 | `UserRight` has **no** unique constraint on `(UserId, ScreenId)`; only non-unique FK indexes exist. | **Confirmed** — snapshot `:4676-4680` | **D-2**; **Q-27** |
| F-3 | `GetUserRightsWithScreensAsync` has no `OrderBy`, so `FirstOrDefault` under duplicates is non-deterministic **today**, in Blazor. | **Confirmed** — `UserRightsRepository.cs:24-27` | **D-2**; **Q-27** |
| F-4 | `CurrentUserService.GetUserIdAsync()` silently returns `0` for a missing or unparseable claim. | **Confirmed** — `CurrentUserService.cs:59-65` | **D-3**; `M2-A01-02` must not use this service |
| F-5 | `AuthController.Login` does **not** call `SyncRightsForUserAsync`; the Blazor login path does, and only for `UserId == 1`. | **Confirmed** — `AuthController.cs:39-59`; `Login.razor:345-349` | **D-5**; **Q-28** — an API-only administrator can hold zero rights |
| F-6 | The two rights-seeding paths use opposite defaults: `SyncRightsForUserAsync` grants all four operations; `UserService` grants view-only, and only when `IsViewOnly`. | **Confirmed** — `UserRightService.cs:67-71`; `UserService.cs:442-464` | **Q-28** |
| F-7 | All five `UserRight` write sites are in the Blazor host; none in the API. An in-process API cache cannot be invalidated by any of them. | **Confirmed** (inventory); **Inferred** (no future API writer in M2-A01) | §8.4; **Q-29** |
| F-8 | The seed contains at least one misspelling that is nonetheless the canonical matching string: `Id = 82`, `"Sub-Contrect GRN"`. | **Confirmed** — `ApplicationDbContext.cs`, Appendix A | **D-6**'s startup check; `M2-B05`'s generator must not "correct" it |
| F-9 | `Program.cs` line numbers in `tasks/M2-A01-01.md` are stale: `UseAuthentication()`/`UseAuthorization()` are at `:121`/`:122`, not `:114`/`:115`. | **Confirmed** — re-read 2026-08-18 | §6; consistent with the `CLAUDE.md` warning about this file |
| F-10 | No code path writes a `Screens` row; the 152 seeded rows are the entire runtime catalogue. | **Confirmed (negative result)** | Makes **D-6**'s throw-on-unseeded-name safe |
| F-11 | No collation is configured in the EF model or the initial migration, so tenant databases inherit their creation default — unrecorded. | **Confirmed (negative result)** | **D-1**'s residual risk; irrelevant to the outcome *because* of **D-1** |

---

## 11. Open questions raised

Added to [`open-questions.md`](../open-questions.md) (KB-004).

| ID | Question | Blocks |
|---|---|---|
| **Q-27** | Do duplicate `(UserId, ScreenId)` rows exist in live tenant databases, and should a unique index plus a deterministic `OrderBy` be added? | Whether **D-2**'s first-match-wins is a faithful reproduction or a reproduction of a latent bug |
| **Q-28** | How does a user who authenticates only through the API acquire `UserRight` rows, given that seeding runs on the Blazor login path and only for `UserId == 1`? | **`M2-A02`** — applying the filter to `CurrencyController` without answering this locks out the vertical slice |
| **Q-29** | Is a 60-second staleness window on revoked rights acceptable, given that no cross-process invalidation exists between the Blazor and API hosts? | §8.4; the scope of `M2-A01-03` |

None of the three blocks `M2-A01-02`: the filter can be built and unit-tested against all of
§3 without them. **Q-28 does block `M2-A02`.**

---

## 12. Gate exception under which this document was written

`tasks/M2-A01-01.md` declares `depends_on: [G0]`, and
[KB-080 §9](../execution/README.md#9-m2--foundation) states *"Gate G0 must have passed. Not
negotiable."* **G0 has not passed** — zero of seven exit criteria are ticked as of
2026-08-18.

This task was nonetheless executed, on **2026-08-18**, by the explicit decision of the
repository owner (**Vivek**), on this reasoning: `M2-A01-01` produces documentation only. The
two things G0 exists to guarantee — a reproducible environment from stored-procedure DDL, and
characterisation tests proving behaviour preservation — are prerequisites for *changing
behaviour*. This document changes none. Every input it needed already exists in the working
tree.

**The exception does not extend to `M2-A01-02` or anything else in M2.** The moment code is
written against this specification, G0's rationale applies in full: without characterisation
tests there is no way to prove the filter preserves `RightsHelper`'s semantics, and §9 lists
verification that cannot even run until `M0-12-01` creates a test project. Recorded in
[KB-081](../execution/task-tracker.md) so the deviation is visible rather than silent.

---

## Appendix A — the 152 seeded screen names

Extracted programmatically from `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs`
starting at `:1151` (`builder.Entity<Screens>().HasData(`), not transcribed by hand.
**Confirmed** 2026-08-18: 152 rows; all `ScreenName` values unique, and still unique under
case-insensitive comparison; `Id == ScreenCode` for every row; no leading or trailing
whitespace.

`[RequireScreen]` must carry one of these strings **exactly** — ordinal comparison (**D-1**).
`P` marks `IsPrintRequired = true` (3 rows: 36, 39, 136); it is unrelated to authorization
and is listed only so this appendix is a faithful copy of the seed.

| Id | ScreenName | Id | ScreenName |
|---|---|---|---|
| 1 | `User` | 77 | `Purchase GRN` |
| 2 | `Category` | 78 | `Purchase SCN` |
| 3 | `UOM` | 79 | `Purchase Invoice` |
| 4 | `State` | 80 | `Manufacturing Invoice` |
| 5 | `Currency` | 81 | `Sub-Contract DC-Out` |
| 6 | `User Rights` | 82 | `Sub-Contrect GRN` |
| 7 | `Store` | 83 | `Sub-Contract SCN` |
| 8 | `Raw Material` | 84 | `Sub-Contract Invoice` |
| 9 | `Factors` | 85 | `Export Invoice` |
| 10 | `Process` | 86 | `MasterInspection` |
| 11 | `Machine` | 87 | `FinalInspection` |
| 12 | `Grouping` | 88 | `IncomingInspection` |
| 13 | `Item` | 89 | `InspectionSettings` |
| 14 | `HSN Master` | 90 | `DefectInfo` |
| 15 | `Customer` | 91 | `Assembly Requirement Analysis` |
| 16 | `Expense` | 92 | `Labour GRN` |
| 17 | `Screen Management` | 93 | `Labour SCN` |
| 18 | `Vendor` | 94 | `Labour Delivery Challan` |
| 19 | `BOM` | 95 | `Labour Invoice` |
| 20 | `Income` | 96 | `Route Card Release` |
| 21 | `Currency Today` | 97 | `Excel Upload` |
| 22 | `Bank` | 98 | `Credit Note` |
| 23 | `Company` | 99 | `Printing Map` |
| 24 | `Correspondences` | 100 | `LabourCostManagement` |
| 25 | `Master Upload` | 101 | `BOMLabourCost` |
| 26 | `Holiday List` | 102 | `Sales Track Report` |
| 27 | `Staff` | 103 | `Debit Note` |
| 28 | `LeaveType` | 104 | `Tags` |
| 29 | `Employee Leave Balance` | 105 | `Labour Track Report` |
| 30 | `Leave Application` | 106 | `Payments` |
| 31 | `Project Type Master` | 107 | `Advaceadjustment` |
| 32 | `Cost-Center` | 108 | `Receipts` |
| 33 | `User Level Authorization` | 109 | `Fundtransactions` |
| 34 | `Shift Allocation` | 110 | `Dashboard` |
| 35 | `Item Rate-Updation` | 111 | `Stock Ledger` |
| 36 | `Manufacturing Quotation` **P** | 112 | `Stock Analysis` |
| 37 | `Terms and Conditions` | 113 | `ViewTallyDc-In-Out` |
| 38 | `Leads` | 114 | `Bill Pending List` |
| 39 | `Enquiry Sales` **P** | 115 | `Bill Paid List` |
| 40 | `Authorization` | 116 | `Service Bills` |
| 41 | `Sales Order` | 117 | `Po Pendings` |
| 42 | `Manufacturing DC` | 118 | `Pending Statements` |
| 43 | `General Settings` | 119 | `ToolCribIssue Summary` |
| 44 | `Store Map` | 120 | `ItemHistory` |
| 45 | `Performa Invoice` | 121 | `Confirmation Of Accounts` |
| 46 | `Stock-Add` | 122 | `Stock Position(Internal & External)` |
| 47 | `Material Requisition` | 123 | `TaxDetails Report` |
| 48 | `Material Issue-Note` | 124 | `Profit & Loss Accounts` |
| 49 | `Enquiry Purchase` | 125 | `Item Modification` |
| 50 | `Material Requirement Analysis` | 126 | `RejectionMaster` |
| 51 | `Print Management` | 127 | `Day Book` |
| 52 | `Enquiry Feasibility` | 128 | `Labour Pending` |
| 53 | `Job Order` | 129 | `View Po Track` |
| 54 | `Production Issue WO Assembly` | 130 | `Production Pending Summary` |
| 55 | `Production Return GRN Assembly` | 131 | `Rejection Analysis` |
| 56 | `Production SCN Assembly` | 132 | `GSTITC04` |
| 57 | `Tool-Crib Issue` | 133 | `HRMaster` |
| 58 | `Tool-Crib Return` | 134 | `Biometric Excel Set` |
| 59 | `Instant Search` | 135 | `Salary Head Print Setting` |
| 60 | `Route Card` | 136 | `Salary` **P** |
| 61 | `Production Log Setting` | 137 | `Attendance` |
| 62 | `Daily Production Log` | 138 | `StaffLoan` |
| 63 | `Process Flow-RC` | 139 | `BOM Labour` |
| 64 | `MaintenanceSchedule` | 140 | `Ratings` |
| 65 | `MaintenanceProcess` | 141 | `Purchase Sales Track` |
| 66 | `BreakdownMaintenance` | 142 | `Estimation` |
| 67 | `CalibrationHistoryAndMaintenance` | 143 | `Route Card Analysis` |
| 68 | `Production Issue WO Component` | 144 | `Stock Issue-Request` |
| 69 | `Inter Store Transfer` | 145 | `CreditDebit Summary Report` |
| 70 | `Production Return Component` | 146 | `TDSummary Report` |
| 71 | `Production SCN Component` | 147 | `HSNSummary Report` |
| 72 | `Contract Review` | 148 | `PR PO Rating Report` |
| 73 | `Contract Review CheckList` | 149 | `Candidate` |
| 74 | `Stock Position` | 150 | `Offer Letter` |
| 75 | `Purchase-Quotation` | 151 | `Appointment Letter` |
| 76 | `Purchase Order` | 152 | `Joborder Track` |

Names to read carefully when annotating a controller — each is a live trap for **D-1**:

- `Sub-Contrect GRN` (82) — misspelt in the seed, and therefore correct (**F-8**).
- `Advaceadjustment` (107) — misspelt, one word, no space.
- `Stock Position` (74) and `Stock Position(Internal & External)` (122) — distinct screens;
  the second has **no space** before `(`.
- `Sub-Contract DC-Out` (81), `Sub-Contract SCN` (83), `Sub-Contract Invoice` (84) are spelt
  correctly; only 82 is not.
- Concatenated names (`HRMaster`, `MasterInspection`, `RejectionMaster`, `BOMLabourCost`,
  `LabourCostManagement`, `MaintenanceSchedule`, `Fundtransactions`) carry no spaces, while
  their near-neighbours (`HSN Master`, `BOM Labour`, `Labour Pending`) do.
- `Profit & Loss Accounts` (124) and `Stock Position(Internal & External)` (122) contain `&`.

---

## Appendix B — investigation record

The narrow investigation `tasks/M2-A01-01.md` § *Investigation Requirements* asked for,
registered as **INV-037**. Negative results are recorded as findings, per
[KB-003](../investigation-registry.md).

**Reused, not re-derived:** INV-004 (Complete, 2026-08-12 → [KB-013](auth-and-permissions.md))
for the permission model, enforcement location and claim shapes; INV-008 (Complete,
2026-08-12 → [KB-040](../api/api-overview.md)) for the existing API surface. Both were
confirmed `Complete` in the registry before reuse, and nothing observed in this session
contradicts either.

### B.1 Do duplicate `(UserId, ScreenId)` rows occur, and is `ScreenName` unique?

| Sub-question | Answer | Confidence |
|---|---|---|
| Are the 152 seeded `ScreenName` values unique? | **Yes** — zero collisions, and still zero under case-insensitive comparison | **Confirmed** |
| Can `Screens` gain rows at runtime, creating a name collision? | **No writer exists** (§1.3) | **Confirmed (negative result)** |
| Can duplicate `(UserId, ScreenId)` rows exist? | **Yes, nothing prevents it** — F-2 | **Confirmed** |
| Do they exist in live tenant databases? | Not determinable without database access | **Unknown** — **Q-27** |
| Would duplicates make the current behaviour order-dependent? | **Yes, and non-deterministically** — F-3 | **Confirmed** |

### B.2 Is there any uniqueness constraint or index on `UserRight` or `Screens`?

**No.** `grep -n "HasIndex\|HasAlternateKey" ApplicationDbContext.cs` returns exactly five
matches, none on either entity: `:581` (`MfgQuote(QuoteNo, Suffix)`), `:586`, `:589`
(history), `:594`, `:617` (assembly). The model snapshot confirms `UserRight` carries only
`HasIndex("ScreenId")` and `HasIndex("UserId")`, **neither unique**
(`ApplicationDbContextModelSnapshot.cs:4676-4680`), and `Screens` carries **no index at all**
beyond its primary key (`:9145`). **Confirmed (negative result).**

Incidental: the initial migration created the FK column as `ScreenCode`
(`InitialCreate.cs:7210,7226-7230`) and a later migration renamed it to `ScreenId`; the
current model is `ScreenId`. Both FKs are `OnDelete: Cascade`
(`ApplicationDbContextModelSnapshot.cs:25935-25945`) — so deleting a `User` or a `Screens`
row deletes its rights rows. **Confirmed**, and relevant to `M2-A01-03`: a cascade delete
does not pass through any of the five W-sites in §8.4 and so would not fire an explicit
invalidation.

### B.3 Where do `UserRight` rows get written?

Five sites, all in the Blazor host — the full table is **§8.4**, and the cross-process
consequence is **F-7**. Search performed: `grep -rn "UserRights\." --include=*.cs
--include=*.razor V.SMART/`, excluding the repository and interface definitions.
