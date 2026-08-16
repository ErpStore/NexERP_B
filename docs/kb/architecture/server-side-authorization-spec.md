---
doc_id: KB-103
title: Server-Side Authorization — Implementation Specification
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Shared/RightsHelper.cs
  - V.SMART/V.SMART.Shared/Shared/BaseUserRightsComponent.cs
  - V.SMART/V.SMART.Shared/Data/Master/Admin_Module/UserRight.cs
  - V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/Screens.cs
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRightsRepository.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserRightService.cs
  - V.SMART/V.SMART.Shared/Data/Enum/UserRole.cs
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor
entities: [UserRight, Screens, User]
api_endpoints: []
database_tables: [UserRights, Screens]
business_rules: [BR-AUTH-002]
status: complete
confidence: mixed
last_verified: 2026-08-13
dependencies: [ADR-004, ADR-002, KB-013, KB-040, KB-060]
---

# Server-Side Authorization — Implementation Specification

Implementation specification for [ADR-004](../decisions/ADR-004-server-side-authorization.md),
produced by task [M2-A01-01](../execution/tasks/M2-A01-01.md). **This document contains no
C# code that has been written** — it specifies what [M2-A01-02](../execution/tasks/M2-A01-02.md)
must build. A second reader should be able to implement the filter from this document alone,
without re-reading `RightsHelper.cs`.

> **Sequencing note — read this first.** This specification was produced **ahead of gate
> G0**, which M2's stated prerequisite forbids ("M2 does not start before G0",
> [KB-080 §9](../execution/README.md)). That was a deliberate, approved deviation, recorded
> here rather than left implicit. Rationale: G0's content is repository *safety* (credential
> rotation, CI, characterisation tests, database rebuild) and does not inform the
> authorization design, which derives entirely from ADR-004 and KB-013/INV-004 — both
> `Complete` and cited throughout. This is a document; it costs little to revise if G0
> surfaces something unexpected. **No code was written and no gate was marked passed.**
> M2-A01-02 (which does write code) should still wait for G0.

## 0. Confidence summary and the one structural gap

Every claim below is tagged **Confirmed** (verified against source in this session),
**Carried forward** (Confirmed by a prior investigation, not re-verified here, with the
reason), or **Unknown**.

**The gap:** `V.SMART/V.SMART.Api/` **does not exist in this checkout** — absent from both
the working tree and the git index (`ls` and `git ls-files` both return nothing; M0-00
group G2 deliberately deferred committing it to M0-03-01 because it carries a JWT secret).
Task steps 10–11 asked for `Program.cs:60-116` pipeline ordering and
`Auth/JwtTokenService.cs:25-31` claim types to be read directly. **They could not be.** Those
two areas are therefore *Carried forward* from KB-013 (INV-004, Complete 2026-08-12) and
flagged individually below. **M2-A01-02 must re-verify §6 and §7 against the real files once
M0-03-01 brings that project into source control.** Nothing else in this document depends on
them.

## 1. Types to create

All paths below are **prospective** — they live in `V.SMART/V.SMART.Api/`, which is not yet
in source control (see §0). Namespace `V.SMART.Api.Authorization` throughout.

| # | Type | File | Signature |
|---|---|---|---|
| T-1 | `Right` (enum) | `V.SMART/V.SMART.Api/Authorization/Right.cs` | `public enum Right { View, Create, Edit, Delete }` |
| T-2 | `RequireScreenAttribute` | `V.SMART/V.SMART.Api/Authorization/RequireScreenAttribute.cs` | `[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)] public sealed class RequireScreenAttribute : Attribute { public string ScreenName { get; } public RequireScreenAttribute(string screenName); }` |
| T-3 | `RequireRightAttribute` | `V.SMART/V.SMART.Api/Authorization/RequireRightAttribute.cs` | `[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)] public sealed class RequireRightAttribute : Attribute { public Right Right { get; } public RequireRightAttribute(Right right); }` |
| T-4 | `ScreenRightAuthorizationFilter` | `V.SMART/V.SMART.Api/Authorization/ScreenRightAuthorizationFilter.cs` | `public sealed class ScreenRightAuthorizationFilter : IAsyncAuthorizationFilter { public Task OnAuthorizationAsync(AuthorizationFilterContext context); }` |
| T-5 | `IUserRightsProvider` | `V.SMART/V.SMART.Api/Authorization/IUserRightsProvider.cs` | `public interface IUserRightsProvider { Task<IReadOnlyList<UserRight>> GetRightsAsync(int tenantId, int userId, CancellationToken ct = default); }` |
| T-6 | `UserRightsProvider` | `V.SMART/V.SMART.Api/Authorization/UserRightsProvider.cs` | `public sealed class UserRightsProvider : IUserRightsProvider` — **uncached** in M2-A01-02; M2-A01-03 adds caching behind this same interface |
| T-7 | `ScreenRightRegistrationValidator` | `V.SMART/V.SMART.Api/Authorization/ScreenRightRegistrationValidator.cs` | `IHostedService` (or startup call) that fails fast on the misannotation in **D-4** |
| T-8 | `AuthorizationServiceCollectionExtensions` | `V.SMART/V.SMART.Api/Authorization/AuthorizationServiceCollectionExtensions.cs` | `public static IServiceCollection AddScreenRightAuthorization(this IServiceCollection services)` |

**T-5's signature is the load-bearing one.** It takes **`tenantId` as well as `userId`** —
see **D-8**, which explains why omitting it is a cross-tenant authorization defect rather
than a style preference.

**DI registration.** `AddScreenRightAuthorization()` registers T-4 (scoped), T-5→T-6
(scoped), and T-7. Per [M2-B07](../execution/tasks/M2-B07.md), it is **called alongside**
`AddVSmartDomain()` in the API host's composition root, not folded into it — the domain
extension is shared with the Blazor and MAUI hosts, which must not acquire an MVC filter
dependency.

## 2. Truth table

Each row cites the `RightsHelper.cs` line whose behaviour the filter must reproduce. All
five helpers have the identical shape `rights.FirstOrDefault(r => r.Screens.ScreenName ==
screenName)?.<Flag> ?? false`.

| # | Case | `RightsHelper` today | Filter must | Evidence |
|---|---|---|---|---|
| TT-1 | No `UserRight` row for the screen | `FirstOrDefault` → `null` → `?? false` | **403** | `RightsHelper.cs:7-20` (all five end `?? false`) |
| TT-2 | Row exists, required flag `false` | `false` | **403** | `RightsHelper.cs:8,11,14,17` |
| TT-3 | Row exists, required flag `true` | `true` | **allow** | `RightsHelper.cs:8,11,14,17` |
| TT-4 | Row exists, `IsHide == true`, required flag `true` | operation allowed — `IsHidden` is a *separate* helper never consulted by the four operation checks | **allow** | `RightsHelper.cs:19-20` vs `:7-17`; `BaseUserRightsComponent.cs:23-27` exposes them as five independent properties |
| TT-5 | Screen name not present in `Screens` at all | no row matches → `?? false` | **403** | `RightsHelper.cs:8` |
| TT-6 | Multiple rows match the screen name | first in query order wins | **mirror it** (+ log) — see **D-2** | `RightsHelper.cs:8` (`FirstOrDefault`) |
| TT-7 | Action has `[RequireRight]`, controller has no `[RequireScreen]` | n/a — no Blazor analogue | **403 at request time; fail at startup** — see **D-4** | — |
| TT-8 | `UserId` claim missing or unparseable | n/a — no Blazor analogue | **401**, not 403 — see **D-3** | — |

**TT-4 is the row most likely to be implemented wrong.** `IsHide` is a navigation-visibility
flag. A user with `IsHide == true` and `CanEdit == true` **can edit**. Do not `&&` it into
the operation check.

## 3. Numbered decisions

ADR-004 left these open. Each is decided here, with justification. None is "TBD".

### D-1 — Screen-name comparison is **ordinal (case-sensitive)**

**Decision:** compare with `string.Equals(a, b, StringComparison.Ordinal)`.

**Justification — and a correction to the task brief.** M2-A01-01's own framing asks for the
behaviour "under EF's client-side evaluation" and calls the database collation `Unknown`.
**That framing is wrong, and the real answer is cleaner.** The comparison never reaches the
database:

- `UserRightsRepository.GetUserRightsWithScreensAsync` filters **only by `UserId`** and
  materialises immediately — `_db.Set<UserRight>().Include(r => r.Screens).Where(r => r.UserId == userId).ToListAsync()`
  (`UserRightsRepository.cs:22-28`). **Confirmed.**
- `RightsHelper` then operates on a `List<UserRight>` (`RightsHelper.cs:7`), i.e.
  **LINQ-to-Objects over an already-materialised list**. `r.Screens.ScreenName == screenName`
  is therefore `System.String.operator ==` → `String.Equals` → **ordinal, case-sensitive,
  culture-independent**. **Confirmed.**

So database collation is **irrelevant to this comparison** — it governs only the `UserId`
predicate. Choosing `StringComparison.Ordinal` reproduces today's behaviour exactly.

**Residual risk (accepted):** a typo or case difference in a `[RequireScreen]` literal
silently denies rather than erroring. That is fail-closed — the safe direction — but
confusing to diagnose. Mitigated by **D-4**'s startup validation (which catches unknown
screen names at boot, not at request time) and ultimately by **D-6**'s generated constants.

**Verified, and worth recording as a negative result:** across the 152 seeded rows there is
**no pair of names differing only by case** and no leading/trailing whitespace (checked
mechanically — see §5, F-1). So ordinal and case-insensitive matching resolve *identically*
against seed data today; ordinal is chosen to preserve semantics exactly if a tenant later
adds a row that differs only by case.

### D-2 — Duplicate matching rows: **mirror `FirstOrDefault`, and log**

**Decision:** take the first matching row in the order the repository returns, exactly as
`RightsHelper.cs:8` does. Do **not** reject, do **not** OR the flags together, do **not**
take the most-permissive row.

**Justification.** ADR-004's *Consequences* states the permission model is unchanged so
"existing tenant configuration keeps working with no data migration". Any other rule would
change the effective permissions of an existing tenant silently — the exact failure mode
(silent lockout or silent bypass) this spec task exists to prevent.

**Additive, non-semantic safeguard:** when the provider observes more than one row matching
the screen name for a user, log a warning naming the tenant, user and screen. This makes an
invisible data condition visible without altering the decision. Logging is not a behaviour
change.

**Why this is a live concern (F-2, §5):** there is **no uniqueness constraint or index on
`UserRight` or `Screens`** anywhere in `ApplicationDbContext.cs` — Confirmed by grep; the
only `HasIndex` calls in the file target unrelated entities. Nothing at the database level
prevents duplicate `(UserId, ScreenId)` rows, and `Screens.ScreenName` has no unique
constraint either (`Screens.cs:14-15` is `[Required]` only). Whether duplicates actually
occur in live tenant databases is **Unknown** — see **Q-20**.

### D-3 — Missing or unparseable `UserId` claim → **401**, not 403

**Decision:** return `401 Unauthorized` with an empty body, per ADR-002 §4's 401 row.

**Justification.** 403 means "we know who you are and you may not do this". A token that
carries no parseable `UserId` does not establish who the caller is — that is an
authentication failure. `[Authorize]` should have rejected it upstream; reaching the filter
in that state means a malformed or hand-crafted token, and answering 403 would leak that the
endpoint exists and what it requires. ADR-002 §4 assigns 401 to "unauthenticated / token
expired" and gives it no body.

**Claim to read:** `"UserId"` (a raw string claim type, **not** `ClaimTypes.NameIdentifier`)
— *Carried forward* from KB-013 §JWT claims (`ClaimTypes.Name`, `"UserId"`, `"TenantId"`,
`ClaimTypes.Role`), not re-verified (see §0).

**Trap, worth stating explicitly:** if the filter ever reads the role claim, it must use
`ClaimTypes.Role`, **not** the literal `"role"`. KB-013 records that `CurrentUserService`
reads `"role"` while the token writers emit `ClaimTypes.Role`, which is why
`GetUserRoleAsync()` always returns empty ([KB-060 R-18](../risks/technical-debt-register.md)).
Per **D-5** the filter reads no role claim at all, so this trap is avoided by construction —
but M2-A01-02 must not "helpfully" add one.

### D-4 — `[RequireRight]` without `[RequireScreen]` → **fail closed, and fail at startup**

**Decision, two parts:**

1. **Request time:** if an action carries `[RequireRight]` and no `[RequireScreen]` is found
   on the controller (or its bases), the filter returns **403**. It must never fall through
   to "allow".
2. **Startup (preferred, and required):** `ScreenRightRegistrationValidator` (T-7) scans the
   application's `ApplicationPartManager` / action descriptors at boot and **throws**, halting
   startup, if any of these hold:
   - an action has `[RequireRight]` but its controller has no `[RequireScreen]`;
   - a controller has `[RequireScreen]` whose name is not one of the 152 in **Appendix A**;
   - a non-`[AllowAnonymous]` action on a `[RequireScreen]` controller has no
     `[RequireRight]`.

**Justification.** A misannotated endpoint is exactly the R-03 hole reopened, and a runtime
403 only reveals it when someone happens to call that endpoint with an under-privileged user.
Startup validation converts a silent, permission-dependent security hole into a
deterministic, immediate boot failure. It also catches **D-1**'s typo risk before deployment
rather than in production. ADR-004's *Non-negotiable* section ("No controller ships without
`[RequireScreen]` + `[RequireRight]`") is a rule with no automated enforcement until this
validator exists.

The third bullet is deliberately strict: it forces every new action to make an explicit
choice (`[RequireRight]` or `[AllowAnonymous]`), so "forgot to annotate" cannot read as
"intentionally public".

### D-5 — **No `Administrator` bypass.** ADR-004 grants none, and none is needed

**Decision:** the filter does not special-case `UserRole.Administrator`, `UserId == 1`, or
any role claim. Every caller goes through the same deny-by-default check.

**Justification — this is the strongest-evidenced decision in the document.** A bypass would
be not merely unauthorised by ADR-004 but *redundant*, because the existing system already
provisions administrators with real rows:

- `Login.razor:345-348` — on login, `if (user.UserId == 1)` calls
  `userRightService.SyncRightsForUserAsync(user.UserId)`. Note it keys on **`UserId == 1`**,
  not on `Role == Administrator`. **Confirmed.**
- `UserRightService.SyncRightsForUserAsync` (`UserRightService.cs:32-80`) finds every
  `Screens` row the user has no `UserRight` for and inserts one with
  `CanView = CanCreate = CanEdit = CanDelete = true, IsHide = false`
  (`UserRightService.cs:62-77`). **Confirmed.**

So user 1 satisfies TT-3 by ordinary means. Adding a bypass would (a) diverge from ADR-004,
(b) mask exactly the misconfiguration the permission-matrix suite (M2-A03) exists to catch,
and (c) grant more than the Blazor UI grants, breaking ADR-004's "the two mechanisms read the
same tables and agree".

**A real gap this surfaced — see Q-21.** The sync is triggered from the **Blazor login page**.
It is *not* known to run in the API login path (`AuthController`), because that file is not
in this checkout (§0). If it does not, an administrator who authenticates **only** through
the API will never receive rows for screens added after their last Blazor login, and will
start receiving 403s on new screens. This does not change D-5 — the fix is to trigger the
sync on API login too, not to add a bypass — but M2-A01-02/M2-A07 must confirm it.

### D-6 — Screen names: **string literals now, generated constants via M2-B05**

**Decision:** `[RequireScreen("…")]` takes a string literal in M2-A01-02, and every literal
**must** match an Appendix A entry byte-for-byte. This specification does **not** build the
constants class.

**Justification and migration path.** ADR-004's *Consequences* calls for a generated
`ScreenNames` class with an analyzer forbidding literals; [M2-B05](../execution/tasks/M2-B05.md)
owns that (risk R-10). Building it here would duplicate that task. The interim rule is safe
because **D-4**'s startup validator rejects any screen name not in the seeded set, which is
the same protection the analyzer would give, enforced at boot instead of at compile time.

**When M2-B05 generates the constants, two names need identifier sanitisation** (Confirmed,
mechanically checked): `Stock Position(Internal & External)` and `Profit & Loss Accounts`
contain `&` and parentheses. No seeded name contains `/`. Generation must not silently
mangle them into names that no longer round-trip to the exact string.

### D-7 — `403` body

**Decision:** `application/problem+json` per ADR-002 §4, whose 403 row specifies the body
carries "which screen + right".

```json
{
  "type": "https://nexerp/errors/screen-right-denied",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have the 'Edit' right for the 'Sales Order' screen.",
  "instance": "/api/v1/sales-orders/42",
  "screen": "Sales Order",
  "right": "Edit",
  "traceId": "00-3f8a...-01"
}
```

`screen` and `right` are extension members (RFC 7807 permits them). `traceId` matches the
500 row's requirement in ADR-002 §4 and should be present on every error for correlation.

**Deliberate choice:** the body **names the screen and right that were required**, which is a
small information disclosure to an authenticated user. ADR-002 §4 mandates it, and the
trade-off favours it — the alternative produces unactionable "Forbidden" responses that make
misannotation (D-4) nearly impossible to diagnose. It discloses nothing to an *unauthenticated*
caller, who receives 401 with no body (D-3).

### D-8 — Cache key **must include `TenantId`**; TTL ≈60 s; invalidated on write

**Decision:** cache key is the pair **`(tenantId, userId)`**. TTL 60 seconds, per ADR-004 §2.
Entries are evicted on any `UserRight` write (call sites in F-3).

**Justification — this is a correctness requirement, not an optimisation.** ADR-004 §2 says
"a short (≈60 s) per-user memory cache". Read literally as *keyed on `userId` alone*, that is
a **cross-tenant authorization defect**:

- `UserRight` rows live in the **tenant** database — `UserRightsRepository` takes
  `ApplicationDbContext` (`UserRightsRepository.cs:12,15-19`), which
  `TenantDbContextFactory` builds per-tenant from the `Tenants` row
  ([KB-014](multi-tenancy.md); `TenantDbContextFactory.cs:14-26`). **Confirmed.**
- `User.UserId` is therefore **per-tenant, not globally unique** — every tenant has its own
  user 1 (indeed `SyncRightsForUserAsync` treats `UserId == 1` as the administrator in
  *each* tenant).

A cache keyed on `userId` alone would serve tenant A's rights to tenant B's user of the same
id, in a system whose entire isolation model is per-tenant connection strings. The API host
is a single process serving all tenants, so the two would share one `IMemoryCache`. **The
tenant id is not optional.** M2-A01-03 must treat this as a test case, not a code-review note.

`tenantId` comes from the `"TenantId"` JWT claim (ADR-004 §5; *Carried forward* from KB-013,
see §0).

**Invalidation:** evict `(tenantId, userId)` after any successful write to that user's rights.
The complete write surface is F-3 below. A 60 s TTL bounds staleness even if an invalidation
call site is missed — but missing one is a real bug, because a permission *revocation* that
takes 60 s to apply is a security-relevant delay.

## 4. Non-goals

Explicitly **out of scope** for this specification and for M2-A01-02:

- **No change to `Screens` or `UserRight`** — no new columns, no constraints, no migration.
  (The absence of a uniqueness constraint is *recorded* in F-2 and Q-20, not fixed here.)
- **No rights in the JWT** — ADR-004 §2 is explicit; rights resolve per request.
- **No `UserAuthority` approval checks** — that is ADR-004 §4, implemented by M2-B08 / M3-4.
- **No row-level / record-level scoping** — `StateCodesCsv` and friends are
  [M2-A08](../execution/tasks/M2-A08.md), and per KB-080 §9's risk table that scoping is
  opt-in and currently applies to `Leads` only.
- **No IDOR validation of `{id}` route parameters** — ADR-004 §5 assigns it to the permission
  test suite; the filter authorises *screen × right*, not *row ownership*.
- **No change to Blazor** — `BaseUserRightsComponent` is untouched during the strangler
  period (ADR-004 *Consequences*, "Neutral").
- **No `ScreenNames` constants class** — M2-B05 / R-10 (see D-6).
- **No caching implementation** — M2-A01-03. This document fixes the *key shape* (D-8)
  because getting it wrong is a security defect, and leaves the mechanism open.

## 5. Investigation findings

The three questions M2-A01-01 raised, answered against source in this session. Negative
results recorded per [KB-005](../INDEX.md)'s anti-repetition protocol.

### F-1 — Are `Screens.ScreenName` values unique? **Yes, in seed data.**

```yaml
Finding:        All 152 seeded ScreenName values are unique, including case-insensitively.
                All 152 ScreenCode values are unique. Ids are contiguous 1..152 and
                ScreenCode == Id for every row. No name has leading or trailing whitespace.
                Two names contain '&' and parentheses; none contains '/'.
Evidence:       V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1150-1151 (ToTable then
                HasData); 152 `new Screens` initialisers extracted mechanically and checked
                with sort|uniq -d on the exact values, on lowercased values, and on ScreenCode.
Business rule:  BR-AUTH-002
Confidence:     Confirmed (seed data only — live tenant rows are Unknown, see Q-20)
Last verified:  2026-08-13
```

**Consequence:** `RightsHelper`'s order-dependent `FirstOrDefault` is *not* currently
ambiguous for a correctly-seeded tenant, which is why the existing UI behaves
deterministically. That guarantee comes from the seed data, not from the schema (F-2).

### F-2 — Is there any uniqueness constraint or index on `UserRight` / `Screens`? **No.**

```yaml
Finding:        Negative result. No HasIndex, HasAlternateKey, or unique constraint is
                configured on UserRight or Screens. The only entity configuration for
                Screens is `builder.Entity<Screens>().ToTable("Screens")`. Screens.ScreenName
                is [Required] with no length or uniqueness attribute. Nothing prevents
                duplicate (UserId, ScreenId) rows or duplicate ScreenName values.
Evidence:       grep HasIndex|HasAlternateKey over
                V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs -> 5 hits, all on
                unrelated entities (Quote(QuoteNo,Suffix):581, History(RecordId):586,
                History(ActionDate):589, Assembly(AssmblyID,ItemId):594 and :617).
                Screens config at :1150. Screens.cs:14-15; UserRight.cs:10-33.
Business rule:  BR-AUTH-002
Confidence:     Confirmed
Last verified:  2026-08-13
```

**Consequence:** F-1's uniqueness is a property of the seed, enforced by nothing. D-2 mirrors
`FirstOrDefault` rather than assuming uniqueness, and logs when the assumption is violated.

### F-3 — Where are `UserRight` rows written? **Seven call sites** — M2-A01-03's invalidation surface

```yaml
Finding:        Seven write sites across services and Razor pages. There is no single
                choke point, so cache invalidation cannot be implemented in one place
                unless a seam is introduced.
Evidence:       UserRightService.cs:77            CreateRangeAsync (bulk, via SyncRightsForUserAsync:32)
                UserService.cs:464                CreateAsync
                EmployeeService.cs:191            DeleteAsync
                UserRights.razor:446              UpdateAsync
                UserRights.razor:462              CreateAsync
                EmployeeUpsert.razor:921          DeleteAsync
                Login.razor:348                   SyncRightsForUserAsync (indirect, admin login)
Business rule:  BR-AUTH-002
Confidence:     Confirmed (within V.SMART.Shared and the Blazor pages; the API project is
                absent from this checkout — see §0 — so any writer inside V.SMART.Api is
                Unknown)
Last verified:  2026-08-13
```

**Consequence, and a design note for M2-A01-03.** Four of the seven writes are in the Blazor
host and two are in `.razor` pages calling `IUnitOfWork` directly. A cache living in the API
process **will not observe them** — a rights change made in the Blazor UI is invisible to the
API's in-memory cache until the TTL lapses. That is the real justification for ADR-004's 60 s
TTL, and M2-A01-03 should record it as an accepted bound rather than assume event-driven
invalidation is achievable without a shared cache or a change-notification seam.

## 6. Pipeline placement — *Carried forward, must be re-verified*

**Not verified this session** — `V.SMART/V.SMART.Api/Program.cs` is absent (§0). Per KB-013,
the filter must run **after** authentication has populated `HttpContext.User` and alongside
MVC's authorization stage, i.e. registered as an MVC filter so it executes after
`UseAuthentication()` and within/after `UseAuthorization()`. M2-A01-02 must confirm the exact
line placement against the real `Program.cs` and record it.

Registering the filter globally vs per-controller: **per-controller via the attributes**
(T-2/T-3), not as a global filter. A global filter would have to allow-list every
unannotated endpoint, inverting D-4's fail-closed default.

## 7. Claims the filter relies on — *Carried forward, must be re-verified*

**Not verified this session** — `V.SMART/V.SMART.Api/Auth/JwtTokenService.cs` is absent (§0).

| Claim | Type | Used for |
|---|---|---|
| `"UserId"` | raw string claim | rights lookup (D-3) |
| `"TenantId"` | raw string claim | cache key (D-8), tenant resolution |
| `ClaimTypes.Name` | standard | logging only |
| `ClaimTypes.Role` | standard | **not read** by this filter (D-5) |

Source: KB-013 §JWT claims (INV-004, Complete 2026-08-12). M2-A01-02 re-verifies.

## 8. Open questions raised

Both are recorded in [open-questions.md](../open-questions.md).

- **Q-20** — Do duplicate `(UserId, ScreenId)` rows, or duplicate `Screens.ScreenName`
  values, exist in any live tenant database? Nothing in the schema prevents them (F-2) and
  the answer changes whether D-2's mirrored `FirstOrDefault` is merely faithful or actively
  dangerous. Needs database access; pairs naturally with M0-02's cross-tenant sweep.
- **Q-21** — Does the API login path (`AuthController`) call `SyncRightsForUserAsync`, as
  `Login.razor:348` does for `UserId == 1`? If not, an API-only administrator silently lacks
  rights for screens seeded after their last Blazor login (D-5). Needs
  `V.SMART/V.SMART.Api/` in source control (M0-03-01).

## Appendix A — the 152 seeded screen names

Every `[RequireScreen]` literal **must** match one of these byte-for-byte (D-1 is ordinal).
Extracted mechanically from `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1151`
onward — not transcribed by hand. `ScreenCode == Id` for every row (F-1).

| Id | ScreenCode | `ScreenName` (exact) |
|---|---|---|
| 1 | 1 | `User` |
| 2 | 2 | `Category` |
| 3 | 3 | `UOM` |
| 4 | 4 | `State` |
| 5 | 5 | `Currency` |
| 6 | 6 | `User Rights` |
| 7 | 7 | `Store` |
| 8 | 8 | `Raw Material` |
| 9 | 9 | `Factors` |
| 10 | 10 | `Process` |
| 11 | 11 | `Machine` |
| 12 | 12 | `Grouping` |
| 13 | 13 | `Item` |
| 14 | 14 | `HSN Master` |
| 15 | 15 | `Customer` |
| 16 | 16 | `Expense` |
| 17 | 17 | `Screen Management` |
| 18 | 18 | `Vendor` |
| 19 | 19 | `BOM` |
| 20 | 20 | `Income` |
| 21 | 21 | `Currency Today` |
| 22 | 22 | `Bank` |
| 23 | 23 | `Company` |
| 24 | 24 | `Correspondences` |
| 25 | 25 | `Master Upload` |
| 26 | 26 | `Holiday List` |
| 27 | 27 | `Staff` |
| 28 | 28 | `LeaveType` |
| 29 | 29 | `Employee Leave Balance` |
| 30 | 30 | `Leave Application` |
| 31 | 31 | `Project Type Master` |
| 32 | 32 | `Cost-Center` |
| 33 | 33 | `User Level Authorization` |
| 34 | 34 | `Shift Allocation` |
| 35 | 35 | `Item Rate-Updation` |
| 36 | 36 | `Manufacturing Quotation` |
| 37 | 37 | `Terms and Conditions` |
| 38 | 38 | `Leads` |
| 39 | 39 | `Enquiry Sales` |
| 40 | 40 | `Authorization` |
| 41 | 41 | `Sales Order` |
| 42 | 42 | `Manufacturing DC` |
| 43 | 43 | `General Settings` |
| 44 | 44 | `Store Map` |
| 45 | 45 | `Performa Invoice` |
| 46 | 46 | `Stock-Add` |
| 47 | 47 | `Material Requisition` |
| 48 | 48 | `Material Issue-Note` |
| 49 | 49 | `Enquiry Purchase` |
| 50 | 50 | `Material Requirement Analysis` |
| 51 | 51 | `Print Management` |
| 52 | 52 | `Enquiry Feasibility` |
| 53 | 53 | `Job Order` |
| 54 | 54 | `Production Issue WO Assembly` |
| 55 | 55 | `Production Return GRN Assembly` |
| 56 | 56 | `Production SCN Assembly` |
| 57 | 57 | `Tool-Crib Issue` |
| 58 | 58 | `Tool-Crib Return` |
| 59 | 59 | `Instant Search` |
| 60 | 60 | `Route Card` |
| 61 | 61 | `Production Log Setting` |
| 62 | 62 | `Daily Production Log` |
| 63 | 63 | `Process Flow-RC` |
| 64 | 64 | `MaintenanceSchedule` |
| 65 | 65 | `MaintenanceProcess` |
| 66 | 66 | `BreakdownMaintenance` |
| 67 | 67 | `CalibrationHistoryAndMaintenance` |
| 68 | 68 | `Production Issue WO Component` |
| 69 | 69 | `Inter Store Transfer` |
| 70 | 70 | `Production Return Component` |
| 71 | 71 | `Production SCN Component` |
| 72 | 72 | `Contract Review` |
| 73 | 73 | `Contract Review CheckList` |
| 74 | 74 | `Stock Position` |
| 75 | 75 | `Purchase-Quotation` |
| 76 | 76 | `Purchase Order` |
| 77 | 77 | `Purchase GRN` |
| 78 | 78 | `Purchase SCN` |
| 79 | 79 | `Purchase Invoice` |
| 80 | 80 | `Manufacturing Invoice` |
| 81 | 81 | `Sub-Contract DC-Out` |
| 82 | 82 | `Sub-Contrect GRN` |
| 83 | 83 | `Sub-Contract SCN` |
| 84 | 84 | `Sub-Contract Invoice` |
| 85 | 85 | `Export Invoice` |
| 86 | 86 | `MasterInspection` |
| 87 | 87 | `FinalInspection` |
| 88 | 88 | `IncomingInspection` |
| 89 | 89 | `InspectionSettings` |
| 90 | 90 | `DefectInfo` |
| 91 | 91 | `Assembly Requirement Analysis` |
| 92 | 92 | `Labour GRN` |
| 93 | 93 | `Labour SCN` |
| 94 | 94 | `Labour Delivery Challan` |
| 95 | 95 | `Labour Invoice` |
| 96 | 96 | `Route Card Release` |
| 97 | 97 | `Excel Upload` |
| 98 | 98 | `Credit Note` |
| 99 | 99 | `Printing Map` |
| 100 | 100 | `LabourCostManagement` |
| 101 | 101 | `BOMLabourCost` |
| 102 | 102 | `Sales Track Report` |
| 103 | 103 | `Debit Note` |
| 104 | 104 | `Tags` |
| 105 | 105 | `Labour Track Report` |
| 106 | 106 | `Payments` |
| 107 | 107 | `Advaceadjustment` |
| 108 | 108 | `Receipts` |
| 109 | 109 | `Fundtransactions` |
| 110 | 110 | `Dashboard` |
| 111 | 111 | `Stock Ledger` |
| 112 | 112 | `Stock Analysis` |
| 113 | 113 | `ViewTallyDc-In-Out` |
| 114 | 114 | `Bill Pending List` |
| 115 | 115 | `Bill Paid List` |
| 116 | 116 | `Service Bills` |
| 117 | 117 | `Po Pendings` |
| 118 | 118 | `Pending Statements` |
| 119 | 119 | `ToolCribIssue Summary` |
| 120 | 120 | `ItemHistory` |
| 121 | 121 | `Confirmation Of Accounts` |
| 122 | 122 | `Stock Position(Internal & External)` |
| 123 | 123 | `TaxDetails Report` |
| 124 | 124 | `Profit & Loss Accounts` |
| 125 | 125 | `Item Modification` |
| 126 | 126 | `RejectionMaster` |
| 127 | 127 | `Day Book` |
| 128 | 128 | `Labour Pending` |
| 129 | 129 | `View Po Track` |
| 130 | 130 | `Production Pending Summary` |
| 131 | 131 | `Rejection Analysis` |
| 132 | 132 | `GSTITC04` |
| 133 | 133 | `HRMaster` |
| 134 | 134 | `Biometric Excel Set` |
| 135 | 135 | `Salary Head Print Setting` |
| 136 | 136 | `Salary` |
| 137 | 137 | `Attendance` |
| 138 | 138 | `StaffLoan` |
| 139 | 139 | `BOM Labour` |
| 140 | 140 | `Ratings` |
| 141 | 141 | `Purchase Sales Track` |
| 142 | 142 | `Estimation` |
| 143 | 143 | `Route Card Analysis` |
| 144 | 144 | `Stock Issue-Request` |
| 145 | 145 | `CreditDebit Summary Report` |
| 146 | 146 | `TDSummary Report` |
| 147 | 147 | `HSNSummary Report` |
| 148 | 148 | `PR PO Rating Report` |
| 149 | 149 | `Candidate` |
| 150 | 150 | `Offer Letter` |
| 151 | 151 | `Appointment Letter` |
| 152 | 152 | `Joborder Track` |
