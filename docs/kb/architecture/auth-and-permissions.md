---
doc_id: KB-013
title: Authentication, Authorization and Permissions (As-Is)
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Authentication/Custom AuthenticationStateProvider.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs
  - V.SMART/V.SMART.Shared/Shared/BaseUserRightsComponent.cs
  - V.SMART/V.SMART.Shared/Shared/RightsHelper.cs
  - V.SMART/V.SMART.Shared/Data/Master/Admin_Module/UserRight.cs
  - V.SMART/V.SMART.Shared/Data/Master/Admin_Module/UserAuthority.cs
  - V.SMART/V.SMART.Shared/Data/Master/Admin/User.cs
  - V.SMART/V.SMART.Shared/Data/Enum/UserRole.cs
  - V.SMART/V.SMART.Api/Auth/JwtTokenService.cs
  - V.SMART/V.SMART.Api/Auth/RefreshTokenService.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Controllers/MeController.cs
  - V.SMART/V.SMART.Api/Authorization/IUserRightsProvider.cs
entities: [User, UserRight, UserAuthority, Screens, ApprovalHistory, RefreshToken]
api_endpoints: ["POST /api/v1/auth/login", "POST /api/v1/auth/refresh", "POST /api/v1/auth/logout", "GET /api/v1/me"]
database_tables: [Users, UserRights, UserAuthority, Screens, ApprovalHistory, RefreshTokens]
business_rules: [BR-AUTH-001, BR-AUTH-002, BR-AUTH-003, BR-APPR-001, BR-TEN-002]
status: complete
confidence: confirmed
last_verified: 2026-08-27
dependencies: [KB-010, KB-012]
---

# Authentication, Authorization and Permissions (As-Is)

## Three independent authorization mechanisms

The system layers three unrelated concepts. All three must be reproduced in the new
frontend + API.

> **Three is not the whole picture (added 2026-08-20, M2-A08).** Two further concerns exist that
> none of these three covers: **row-level scope** (which *rows* of an entity a user may see) and
> the **account gates** (QR expiry, trial expiry, device binding). Both live in Razor `@code`,
> not in a service. See §5 and [KB-108](row-scope-and-account-gates.md).

| # | Mechanism | Granularity | Storage | Enforced where |
|---|---|---|---|---|
| 1 | **Role** | 2 values: `Administrator`, `User` | `User.Role` (`UserRole` enum) | `<AuthorizeView Roles="…">` in Razor |
| 2 | **Screen rights** | per user × per screen × {View, Create, Edit, Delete, Hide} | `UserRight` rows joined to `Screens` (**150 present**; 152 seeded, 2 deleted by later migrations — corrected 2026-08-21, see **R-65**) | `BaseUserRightsComponent` in the UI **only** |
| 3 | **Approval authority** | per user × per document type × level | `UserAuthority` (12 boolean/level column pairs) | `ApprovalService` + `Authorization` page |

## 1. Authentication

### Blazor Server (the live application)

`CustomAuthStateProvider : AuthenticationStateProvider` holds a `ClaimsPrincipal` **in
memory, in the SignalR circuit**. There is no cookie, no token, no
`UseAuthentication()` middleware.

`MarkUserAsAuthenticated(userName, userId, role, isQrLogin, isProductionUser)` builds a
`ClaimsIdentity` with authentication type `"apiauth_type"` and claims:
`ClaimTypes.Name`, `"UserId"`, `ClaimTypes.Role`, `"IsQrLogin"`, `"IsProductionUser"`.

**Consequences (Confirmed):**
- Refreshing the browser destroys the circuit and logs the user out.
- Sessions cannot survive a server restart or a load-balancer failover.
- There is no session revocation, no "log out other devices".

### Credential verification

`UserRepository.LoginAsync(username, password)` (`UserRepository.cs:34-49`):

```
user = Users.FirstOrDefault(u => u.UserName == username && u.IsActive)
if (user == null) return null
result = _passwordHasher.VerifyHashedPassword(user, user.UserPassword, password)
return result == PasswordVerificationResult.Success ? user : null
```

Passwords use ASP.NET Core `PasswordHasher<User>` (PBKDF2). **This is sound** and must be
kept as-is so existing credentials keep working.

> **BR-AUTH-001 (Confirmed):** Login requires `IsActive == true`. Deactivating a user
> blocks login immediately.

> **Defect (Confirmed):** `LoginAsync` swallows exceptions and returns `null`
> (`UserRepository.cs:44-48`). A database outage is indistinguishable from a wrong
> password, both to the user and in the logs.

### Alternative login modes

| Mode | Route / field | Rule |
|---|---|---|
| **QR login** | `/qrlogin/{token:guid}` → `GetUserByQrToken(Guid)` | requires `QrToken` match **and** `IsQrEnabled` **and** `IsActive`. `QrCreatedDate`, `QrExpiryDate`, `LastQrLogin` are stored. **Unknown:** whether `QrExpiryDate` is enforced anywhere — not checked in `GetUserByQrToken` (Q-05). |
| **Production-user mode** | `User.IsProductionBased`, claim `IsProductionUser` | `NavMenu.razor` collapses to a shop-floor-only menu (`/productionLogList/true`) when set |
| **Trial / expiry** | `User.TrialDays`, `User.ExpiryDate`, `GetUserTrialAsync` | licensing gate. **Unknown:** enforcement point (Q-06) |
| **Device binding** | `IsDesktop`/`DesktopDeviceId`/`IpAddress`, `IsMobile`/`MobileDeviceId` | recorded on `User`. **Unknown:** whether enforced (Q-07) |

### REST API (new, partial)

`POST /api/v1/auth/login` → `AuthController.Login`:
1. `_tenantProvider.GetCurrentTenant()`; 400 if unresolved.
2. `_unitOfWork.Users.LoginAsync(username, password)`; 401 if null.
3. `TrialGate.Evaluate(user, HostIsDesktop, DateTime.Today)`; 403 if refused (M2-A08).
4. **Administrator rights seeding (M2-A10, 2026-08-24):** if — and only if — `user.UserId == 1`,
   `IUserRightService.SyncRightsForUserAsync(user.UserId)` is called
   (`AuthController.cs`, `SeedAdministratorRightsAsync`). This mirrors `Login.razor:345-349`;
   without it an administrator who has only ever authenticated through the API holds zero
   `UserRight` rows and ADR-004's filter answers 403 to every annotated endpoint (Q-28,
   [KB-109](../decisions/KB-109-q28-r65-decision-brief.md) option A).
   **The `UserId == 1` gate is the safety property, not an incidental detail:**
   `SyncRightsForUserAsync` writes `CanView`, `CanCreate`, `CanEdit` and `CanDelete` all `true`
   (`UserRightService.cs:67-70`), so widening it by one user grants delete on every screen — that
   was option B, rejected by the owner on 2026-08-24.
   **Failure behaviour (chosen deliberately):** the API logs a seeding exception and lets the
   login succeed, because the credential check and account gates have already passed and a
   transient fault during a repair should not lock out the only account that can fix it. This is
   the same outcome Blazor reaches — see the correction immediately below; the only divergence is
   the post-login navigation Blazor loses.
   **Correction, 2026-08-24 (validation of M2-A10):** an earlier wording here said a Blazor seeding
   failure "does abort the sign-in". That is wrong (Confirmed). `Login.razor:337` calls
   `customAuth.MarkUserAsAuthenticated` **before** the seeding call at `:345-349`, so the Blazor
   user is already authenticated when seeding runs; the page catch at `:357-362` only toasts an
   error and skips `NavigateTo("/dashboard")`, leaving them signed in but stranded on the login
   page. The actual divergence is that Blazor loses the navigation while the API returns its
   normal `200`. `Login.razor` is unchanged.
5. `JwtTokenService.CreateToken(user, tenant.Id)`, then (M2-A04)
   `IRefreshTokenService.IssueAsync(user.UserId)`.
6. Returns `{ token, refreshToken, tokenExpiresAtUtc, username, userId, tenantId, role }`
   (M2-A04 — `refreshToken`/`tokenExpiresAtUtc` are new; the original four fields keep
   their names, types and order).

JWT claims: `ClaimTypes.Name`, `"UserId"`, `"TenantId"`, `ClaimTypes.Role`.
HS256, `ExpiresMinutes` default **15** (M2-A04, was 480/8 h), `ClockSkew` 1 minute,
issuer/audience validated.

**Gaps in the API auth design (Confirmed), updated by M2-A04 (2026-08-27):**
- ~~No refresh token, no rotation, no revocation list.~~ **Closed.** `POST /api/v1/auth/refresh`
  rotates (one-time use, presented token revoked in the same call);
  `POST /api/v1/auth/logout` revokes on demand. Tokens are stored hashed (SHA-256) in a new
  `RefreshTokens` table, one row per tenant database (`RefreshTokenService`,
  [KB-040](../api/api-overview.md#post-apiv1authrefresh--added-by-m2-a04-2026-08-27)).
  `IsActive` is re-checked on every rotation — this is what makes deactivating a user
  effective within one 15-minute access-token lifetime instead of up to 8 hours.
- **No screen-permission claims** — the token cannot authorise anything beyond role.
  **Unchanged by M2-A04**, deliberately: ADR-004 §2 still forbids embedding rights in any
  token, access or refresh.
- Secret is `builder.Configuration["Jwt:Secret"]`, committed in `appsettings.json`.
  **Superseded in substance, not struck here** — M0-03/M0-03-01 externalised the config
  path and M0-04 rotated the workstation's value away from the published one (still
  `Blocked` on the deployment-side rotation, C-4); see `task-tracker.md` footnote ¹⁰⁰.
- ~~8-hour non-revocable token is long for an ERP.~~ **Closed by M2-A04.** 15 minutes,
  configurable via `Jwt:ExpiresMinutes`, justified against the 1-minute `ClockSkew` above.
- The Angular pilot stores the JWT in `localStorage` (`auth.service.ts`), exposing it to
  XSS. **Unchanged — out of this task's scope.** M2-A04 chose body transport for the new
  refresh token specifically *because* Q-16 (deployment topology / TLS termination) is
  Unknown, so an `HttpOnly` cookie cannot yet be configured correctly; the refresh token
  therefore inherits the same `localStorage`/XSS exposure the access token already has.
  Still M2-C02's note to carry, not resolved here.

## 2. Screen rights — the core permission model

### Data model

```
User (1) ──< UserRight >── (1) Screens
                 │
                 ├─ CanView    bool
                 ├─ CanCreate  bool
                 ├─ CanEdit    bool
                 ├─ CanDelete  bool
                 └─ IsHide     bool
```

`Screens { Id, ScreenCode, ScreenName, IsPrintRequired }` — 152 rows seeded in
`ApplicationDbContext.OnModelCreating`, but **only 150 survive**: later migrations
`DeleteData` two of them, so every real database (rebuilt or live) holds **150** rows with
`ScreenCode` 1…152 and 114/115 absent. The two deleted names, `Bill Paid List` and
`Bill Pending List`, are **still present in `V.SMART.Api/Authorization/ScreenCatalogue.cs`** —
see **R-65**, a silent-lockout risk for `M2-A02`. Screen names are human strings such as
`"Sales Order"`, `"Purchase Order"`, `"Stock-Add"`, `"Labour Invoice"`, `"Job Order"`,
`"Route Card"`, `"Payments"`, `"Salary"`, `"GSTITC04"`.

### Enforcement

`Shared/BaseUserRightsComponent : ComponentBase` — inherited by **296 of 333 pages**
(**Confirmed** by grep).

```csharp
protected abstract string ScreenName { get; }          // e.g. "Sales Order"
protected bool CanView   => RightsHelper.HasViewRight(userRights, ScreenName);
protected bool CanCreate => RightsHelper.HasCreateRight(userRights, ScreenName);
protected bool CanEdit   => RightsHelper.HasEditRight(userRights, ScreenName);
protected bool CanDelete => RightsHelper.HasDeleteRight(userRights, ScreenName);
protected bool IsHidden  => RightsHelper.IsHidden(userRights, ScreenName);
```

`LoadRightsAsync()` calls `_unitOfWork.UserRights.GetUserRightsWithScreensAsync(userId)`
and caches the list on the component. `RightsHelper` matches on
`r.Screens.ScreenName == screenName` and **defaults to `false`** when no row exists —
deny-by-default. **Confirmed** (`RightsHelper.cs`).

The page binds these booleans to button visibility and grid actions.

### 🚨 The critical finding

> **No permission check exists anywhere outside the UI layer.**
>
> **Confirmed** by grepping `BusinessLayer/`, `Repository/`, and `Services/` for
> `CanCreate`, `CanEdit`, `CanDelete`, `HasViewRight`, `UserRights`. Every match in
> `BusinessLayer/` is a `CanDelete…Async` **referential-integrity** guard
> (e.g. `AppointmentLetterService.cs:43`, `SalaryService.cs:206`), not authorization.

In Blazor Server this is *tolerable*, because the C# runs on the server and the user
cannot invoke a service method except through a page the server rendered. **The moment a
REST API exists, this protection evaporates**: any authenticated user can call any
endpoint with a valid JWT, regardless of their `UserRight` rows.

**This is a hard prerequisite, not a nice-to-have.** See
[`decisions/ADR-004-server-side-authorization.md`](../decisions/ADR-004-server-side-authorization.md).

### Related defects

> **Defect (Confirmed):** `CurrentUserService.GetUserRoleAsync()` reads the claim type
> `"role"`, but `CustomAuthStateProvider` and `JwtTokenService` both write
> `ClaimTypes.Role` (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`).
> The method therefore always returns `""`. It currently has **zero call sites**, so it is
> latent — but it must not be copied into the new API. Evidence:
> `Services/CurrentUserService.cs:68-75` vs `Authentication/Custom AuthenticationStateProvider.cs:36`.

> **Defect (Confirmed):** `SessionTimeoutService` is a **singleton** with one shared
> `_lastActivity` field (`V.SMART.Web/Program.cs`; `Services/SessionTimeoutService.cs`).
> All concurrent users share one idle clock.

## 3. Approval authority (multi-level document approval)

`UserAuthority` grants approval rights per document type, each as a `bool` + a `string`
level:

| Flag | Level field | Document |
|---|---|---|
| `IsMfgQuote` | `IsMfgQuoteLevel` | Sales Quotation |
| `IsPR` | `IsPRLevel` | Purchase Requisition / Material Requisition |
| `IsPO` | `IsPOLevel` | Purchase Order |
| `IsPurchSCN` | `IsPurchSCNLevel` | Purchase Store Credit Note |
| `IsSubConSCN` | `IsSubConSCNLevel` | Sub-Contract SCN |
| `IsProdAssySCN` | `IsProdAssySCNLevel` | Production Assembly SCN |
| `IsProdCompSCN` | `IsProdCompSCNLevel` | Production Component SCN |
| `IsLabourSCN` | `IsLabourSCNLevel` | Labour SCN |
| `IsLeave` | `IsLeaveLevel` | Leave Application |
| `IsRc` | `IsRcLevel` | Route Card |
| `IsSalesPo` | `IsSalesPoLevel` | Sales Order |
| `IsStockReq` | `IsStockReqLevel` | Material Issue Request |

`IApprovalService`:
```csharp
Task<List<ApprovalVM>> GetPendingApprovalsAsync(string type, string level, string userName);
Task<bool> ApproveAsync(ApprovalVM record, string type, string level, string userName, UserAuthority authority);
Task<bool> RejectAsync(ApprovalVM record, string type, string level, string reason, string userName);
Task<bool> BulkApproveAsync(List<ApprovalVM> records, string type, string level, string username);
Task<bool> BulkRejectAsync(List<ApprovalVM> records, string type, string level, string reason, string username);
```

Audit trail: `ApprovalHistory { Id, RecordId, ApprovalType, Level, Status, ActionBy, ActionDate, Reason }`.

> **BR-APPR-001 (Confirmed):** Rejection requires a `Reason`; the parameter is mandatory
> on both `RejectAsync` and `BulkRejectAsync`, and `ApprovalHistory.Reason` records it.

The user-facing screen is `/approval` (`Pages/Planning_Module_Pages/Authorization_Pages/`),
plus `/userLevelAuthorization` for maintaining `UserAuthority`.

> **Architectural defect (Confirmed):** `IApprovalService.cs` declares
> `using static V.SMART.Shared.Pages.Planning_Module_Pages.Authorization_Pages.Authorization;`
> — the business interface depends on a Razor page type. This **must** be resolved before
> the approval workflow can be exposed over HTTP. Highest-priority decoupling item.

## 4. Other user-scoped state

| Feature | Entity | Note |
|---|---|---|
| Grid column visibility | `UserColumnPreference { UserName, ScreenName, ColumnJson }` | Serialised `List<ColumnPreferenceVM>`; default column set derived by reflection over the type carrying `[ScreenMapping("<name>")]` (`BaseUserRightsComponent.GetColumnsByScreen`) |
| Theme | `UserThemePreference` + `ThemeStateService` | light/dark |
| Misc preferences | `UserPreference` | |
| Row-level data scoping | `User.StateCodesCsv` → `StateCodes` `List<int>` | **Corrected 2026-08-20 (INV-028, M2-A08).** The earlier `Inferred` claim — "restricts visible customers/vendors by state" — is **wrong**: it scopes **`Leads` only**, in `LeadService.GetAllLoadLeadsAsync` (`LeadService.cs:128-152`), and no customer or vendor query references it. See §5 and [KB-108](row-scope-and-account-gates.md). |
| Feature flags on the user | `IsViewOnly`, `HideManualJobOrderButton`, `LevelAuthorization` | UI-level gates |


## 5. Row scope and account gates — the fourth concern (M2-A08, 2026-08-20)

Full evidence: **[KB-108](row-scope-and-account-gates.md)**. Summarised here because the three
mechanisms above do not cover either of these, and a reader of this document would otherwise
assume they did.

**Row scope** is a *second authorization axis*. A user who legitimately holds `CanView` on the
Leads screen passes every check in §2 while reading another region's rows. It exists in exactly
one place — `LeadService.GetAllLoadLeadsAsync` (`LeadService.cs:128-152`), on `Leads` — it is
**opt-in** (the unscoped sibling `GetAllLeadsAsync` has four call sites to its one), it filters
in memory, and it **fails closed**: a blank `StateCodesCsv` yields zero rows. `UserId == 1` is
exempt (`LeadsList.razor:470-484`). **Confirmed.**

**Account gates** are not in any service either. All three live in Razor `@code`:

| Gate | Where | Carve-outs |
|---|---|---|
| Trial / expiry | `Login.razor:271-275` | `!IsDesktop` (the *host*, `:224`), `UserId > 1`, `TrialDays > 0` |
| Device binding | `Login.razor:277-322` | `UserId > 1 && (IsMobile \|\| IsDesktop)`; identity is **client-asserted**; trust-on-first-use |
| QR expiry | `QrLogin.razor:50-56`, `Login.razor:422-429` | the query did not filter it — fixed by M2-A08 |

`User.IsViewOnly` (`User.cs:66`) is **not** a request-time gate: `UserService.cs:442`
materialises a view-only user into per-screen `UserRight` rows at create time, so it rides on
the §2 mechanism with no extra work. **Confirmed.**

**What the API does now.** The trial gate is enforced on `POST /api/auth/login` with all three
carve-outs verbatim; `GetUserByQrToken` excludes expired tokens; row scope has a mechanism
(`V.SMART.Api/Authorization/RowScope*`, applied **at the query**, with a startup validator that
refuses to boot an undeclared scoped endpoint) but no production caller yet, because no Leads
endpoint exists. The **device gate is deliberately not enforced** — see Q-40.
## Summary: what the new API must implement

| Requirement | Source of truth | Priority |
|---|---|---|
| Password verification via `PasswordHasher<User>` | keep `UserRepository.LoginAsync` unchanged | — (done) |
| `IsActive` gate | BR-AUTH-001 | — (done) |
| **Server-side screen-right enforcement on every endpoint** — **first endpoints DELIVERED**: M2-A01 built the mechanism, **M2-A02 (2026-08-24)** applied it to `CurrencyController`, the first resource controller where BR-AUTH-002 is enforced by the API rather than only by the Blazor UI (`Controllers/CurrencyController.cs:21` + `[RequireRight]` on all five actions). `FilesController`, `CurrencyExcelController` were annotated earlier; `MeController`/`ReferenceController` carry the audited `[NoScreenRight]` opt-out. **"Every endpoint" is not yet true structurally**: a controller that simply omits `[RequireScreen]` is still passed through (`ScreenRightAuthorizationFilter.cs:69-72`), so nothing yet *forces* the next controller to annotate — see Q-71 and R-03 | `UserRight` × `Screens` | **P0 — first application landed; harness (M2-A03) still blocking** |
| ~~Return the user's full right set at login so the UI can render~~ — **DELIVERED by M2-A07 (2026-08-20)**: `GET /api/v1/me` returns the caller's identity, tenant, role and complete rights map, read through the **same** `IUserRightsProvider` the M2-A01 filter enforces with. **Presentation only** (ADR-004 §3) — it does not relax that filter. Contract: [KB-040](../api/api-overview.md) | `GetUserRightsWithScreensAsync`, via `IUserRightsProvider` | P0 |
| Refresh tokens + revocation | new | P1 |
| Approval authority checks server-side | `UserAuthority` | P1 |
| Row-level state scoping | `User.StateCodesCsv` — **`Leads` only** (Q-08, INV-028) | **Mechanism landed 2026-08-20 (M2-A08); no scoped endpoint exists yet** |
| QR login endpoint incl. **expiry enforcement** | `GetUserByQrToken` (Q-05) | **Query fixed 2026-08-20 (M2-A08)**; the endpoint itself is still P2 |
| Per-user column preferences | `UserColumnPreference` | P2 |
| Trial/expiry licensing gate | Q-06 | **Enforced on `POST /api/auth/login` 2026-08-20 (M2-A08)** |
