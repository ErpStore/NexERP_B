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
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
entities: [User, UserRight, UserAuthority, Screens, ApprovalHistory]
api_endpoints: ["POST /api/v1/auth/login"]
database_tables: [Users, UserRights, UserAuthority, Screens, ApprovalHistory]
business_rules: [BR-AUTH-001, BR-AUTH-002, BR-AUTH-003, BR-APPR-001]
status: complete
confidence: confirmed
last_verified: 2026-08-21
dependencies: [KB-010, KB-012]
---

# Authentication, Authorization and Permissions (As-Is)

## Three independent authorization mechanisms

The system layers three unrelated concepts. All three must be reproduced in the new
frontend + API.

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
3. `JwtTokenService.CreateToken(user, tenant.Id)`.
4. Returns `{ token, username, userId, tenantId, role }`.

JWT claims: `ClaimTypes.Name`, `"UserId"`, `"TenantId"`, `ClaimTypes.Role`.
HS256, `ExpiresMinutes` default 480 (8 h), `ClockSkew` 1 minute, issuer/audience validated.

**Gaps in the API auth design (Confirmed):**
- No refresh token, no rotation, no revocation list.
- **No screen-permission claims** — the token cannot authorise anything beyond role.
- Secret is `builder.Configuration["Jwt:Secret"]`, committed in `appsettings.json`.
- 8-hour non-revocable token is long for an ERP.
- The Angular pilot stores the JWT in `localStorage` (`auth.service.ts`), exposing it to XSS.

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
| Row-level data scoping | `User.StateCodesCsv` → `StateCodes` `List<int>` | **Inferred:** restricts visible customers/vendors by state. Enforcement point not yet traced — Q-08 |
| Feature flags on the user | `IsViewOnly`, `HideManualJobOrderButton`, `LevelAuthorization` | UI-level gates |

## Summary: what the new API must implement

| Requirement | Source of truth | Priority |
|---|---|---|
| Password verification via `PasswordHasher<User>` | keep `UserRepository.LoginAsync` unchanged | — (done) |
| `IsActive` gate | BR-AUTH-001 | — (done) |
| **Server-side screen-right enforcement on every endpoint** | `UserRight` × `Screens` | **P0 — blocker** |
| Return the user's full right set at login so the UI can render | `GetUserRightsWithScreensAsync` | P0 |
| Refresh tokens + revocation | new | P1 |
| Approval authority checks server-side | `UserAuthority` | P1 |
| Row-level state scoping | `User.StateCodesCsv` (Q-08) | P1 |
| QR login endpoint incl. **expiry enforcement** | `GetUserByQrToken` (Q-05) | P2 |
| Per-user column preferences | `UserColumnPreference` | P2 |
| Trial/expiry licensing gate | Q-06 | P2 |
