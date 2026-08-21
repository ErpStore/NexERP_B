---
doc_id: KB-108
title: Row-Level Scoping and Account Gates — INV-028 Output and Implementation Record
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Data/Master/Admin_Module/User.cs
  - V.SMART/V.SMART.Shared/Data/SalesAndLabour/Leads/Leads.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs
  - V.SMART/V.SMART.Shared/Repository/IRepository/IMasterRepository/IAdmins/IUserRepository.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LeadService/LeadService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/IBusinessService/ILeadservice/ILeadService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserService.cs
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/QrLogin.razor
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/RegisterUpsert.razor
  - V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/Leads_Pages/LeadsList.razor
  - V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/Leads_Pages/LeadsUpsert.razor
  - V.SMART/V.SMART.Api/Authorization/RowScope.cs
  - V.SMART/V.SMART.Api/Authorization/RowScopeProvider.cs
  - V.SMART/V.SMART.Api/Authorization/RowScopeQueryExtensions.cs
  - V.SMART/V.SMART.Api/Authorization/ScopedEntityCatalogue.cs
  - V.SMART/V.SMART.Api/Authorization/RowScopeStartupValidator.cs
  - V.SMART/V.SMART.Api/Auth/AccountGates.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
entities: [User, Leads]
api_endpoints:
  - "POST /api/auth/login"
database_tables: [Users, Leads]
business_rules: [BR-AUTH-001, BR-AUTH-002]
status: complete
confidence: confirmed
last_verified: 2026-08-20
dependencies: [KB-013, KB-105, KB-003, KB-004, KB-060]
---

# Row-Level Scoping and Account Gates

**The output of INV-028 and of task M2-A08.** It answers Q-05, Q-06, Q-07 and Q-08, records
the negative results in full, names the product decisions and states which of them were taken
and which were deferred, and documents the mechanism the API now uses.

Every claim below was **re-verified against current source on 2026-08-20**, not taken from the
2026-08-12 evidence in the task file. Where the task file's line numbers had gone stale, §7
says so.

---

## 1. The one-sentence finding

**Every gate that exists at all exists in a Razor page, and before M2-A08 the API reproduced
none of them.** Row scope, trial expiry, device binding and QR expiry are all enforced — in
`Login.razor`, `QrLogin.razor`, `LeadsList.razor` and one service method — and
`AuthController.Login` resolved a tenant, called `LoginAsync` and issued a JWT. That was the
whole method. **Confirmed.**

That is the migration's stated problem — behaviour trapped in `@code` — arriving where the
consequence is a security boundary rather than a UX regression.

---

## 2. Q-08 / INV-028 — `User.StateCodesCsv` row scope

### 2.1 The negative result, which is the most useful thing in this document

`git grep --untracked -n "StateCodesCsv|StateCodes\b" -- V.SMART/`, re-run 2026-08-20,
excluding `Migrations/`, returns hits **only** in:

| File | Lines | What it is |
|---|---|---|
| `Data/Master/Admin_Module/User.cs` | 51, 54, 56, 58, 61 | the column and its `[NotMapped]` façade |
| `ViewModels/.../UserVM.cs` | 108, 110, 112, 113 | view model |
| `.../RegisterModel.cs` | 58 | view model |
| `.../UserMapping.cs` | 20, 22, 24, 36, 38, 39 | mapping |
| `.../UserService.cs` | 373, 437, 491, 532, 533, 535 | **write / audit paths only** |
| `.../LeadService.cs` | 41-44, 136 | the two predicates (§2.3) |
| `Pages/.../LeadsUpsert.razor` | 695, 826, 827 | assignable-user picker |
| `Pages/.../RegisterUpsert.razor` | 622, 1018, 1022, 1024, 1037, 1038, 1042 | admin **assignment** UI |

**No customer, vendor, item, document or report query references it anywhere.** Q-08 recorded
it as *"**Inferred** to restrict visible customers/vendors by state"*. **It does not.** That
inference is corrected in [KB-004](../open-questions.md), not propagated.
**Confirmed — negative result.**

Two further negative results, both re-run 2026-08-20:

- `git grep --untracked -ni statecode -- V.SMART/V.SMART.Api/ tests/` → **nothing** before this
  task. The API and both test projects contained no row-scope code at all.
- `grep -rln StateCodesCsv db/ "Existing Store Procedures"` → **nothing**. **No stored procedure
  references it**, so ADR-005's report path has no scope predicate to preserve — M2-B08 would
  have to add one from scratch. See §8.
- `git grep --untracked -nE "\.Where\([^)]*([Cc]urrent[Uu]ser|currentUserName|currentUserId)" -- V.SMART/V.SMART.Shared/BusinessLayer/`
  → **zero matches**. No business service filters rows by the current user in an inline
  predicate. Three files from a wider proximity search were spot-checked
  (`ProductionLogService`, `CostingService`, `BankService`) and are all audit/`CreatedBy`
  stamping. **Inferred (strong, method shown): `LeadService` is the only row-scoped service in
  the codebase.**

### 2.2 The one real row scope

`LeadService.GetAllLoadLeadsAsync()`,
`V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LeadService/LeadService.cs:128-152`.
It resolves the current user by **username string equality** over a full `GetAllAsync()`
materialisation (`:132-134`), splits `StateCodesCsv` (`:136-139`), and keeps leads whose
`StateId.ToString()` is in that token set — **in memory**, after materialising every lead
(`:141-144`). **Confirmed.**

A user with a null or blank `StateCodesCsv` gets an empty token list and therefore **zero
leads**. **Fail-closed. Preserved.** **Confirmed.**

### 2.3 Two implementations of one predicate

`LeadService.GetUsersBySelectedStateId` (`:35-47`) is the **other direction** — "which users
cover this state", four `EF.Functions.Like` patterns against the CSV column, in SQL. Its one
call site is `LeadsUpsert.razor:985`, filling the assignable-user picker. It is not row
security. The API adopts exactly one predicate family — the SQL-side one (§5). **Confirmed.**

### 2.4 The actual risk: the scope is opt-in

`ILeadService` declares both the scoped `GetAllLoadLeadsAsync` (`:28`) and the unscoped
`GetAllLeadsAsync` (`:24`). Call-site counts, 2026-08-20:

| Method | Call sites |
|---|---|
| `GetAllLoadLeadsAsync` (**scoped**) | `LeadsList.razor:483` — **one** |
| `GetAllLeadsAsync` (**unscoped**) | `LeadsList.razor:398`, `LeadsList.razor:476`, `LeadsUpsert.razor:1275`, `LeadsUpsert.razor:1287` — **four** |

Nothing in the type system or the repository prevents a caller picking the wrong one. **This
is Q-08's real risk, and it survives the migration unless the API applies scope at the query.**
**Confirmed.**

### 2.5 Two facts the task file did not have

Both found 2026-08-20, both undocumented anywhere before this task, both material:

1. **A `UserId == 1` super-user carve-out.** `LeadsList.razor:470-484`:
   `var Userid = await CurrentUserService.GetUserIdAsync(); if (Userid==1) { … GetAllLeadsAsync() … } else { … GetAllLoadLeadsAsync() … }`.
   **User 1 sees every lead today.** Same shape as the `UserId > 1` carve-out in the trial and
   device gates. Ported (decision P6). **Confirmed.**
2. **The paging count leaks.** `LeadsList.razor:396-401` (`LoadPagedLeads`) computes
   `TotalStaffCount` and `totalPage` from the **unscoped** `GetAllLeadsAsync()`, for **every**
   user. A scoped user's page count today reflects leads they cannot see. Decision P7.
   **Confirmed.**

---

## 3. Q-05 / R-16 — QR token expiry

`UserRepository.GetUserByQrToken` filtered `QrToken`, `IsQrEnabled` and `IsActive` and **not**
`QrExpiryDate`. **Confirmed.** Expiry *was* enforced — post-query, in two duplicated Razor
copies:

| Copy | Lines | Message |
|---|---|---|
| `QrLogin.razor` | 50-56 | `"QR Code Expired"` |
| `Login.razor` | 422-429 | `"QR Expired"` |

Both also re-check `IsQrEnabled` (`QrLogin.razor:43-48`, `Login.razor:414-420`) after the query
already filtered on it. Both use `QrExpiryDate.HasValue && QrExpiryDate.Value < DateTime.Now`
— **`Now`, not `Today`**. `RegisterUpsert.razor:1125` sets the expiry a year ahead when a token
is issued, so expiry dates are real data.

**R-16 restated (claim 15):** the risk was never "expiry is never checked". It was **"the query
returns expired users, and only the two Blazor callers happen to reject them"**. Any third
caller — an API controller, a background job, a mobile client — got a valid `User` for an
expired token.

**Fixed by M2-A08**, in the query, which is the only place that makes correctness independent
of the caller. `GetUserByQrToken` has exactly two call sites (`Login.razor:404`,
`QrLogin.razor:34`), so decision P5's "no third caller" precondition is **Confirmed true** and
the fix is behaviour-preserving for both. A null `QrExpiryDate` still returns the user. Both
Blazor callers keep their redundant checks — a redundant check is not a bug.

---

## 4. Q-06 and Q-07 — the account gates

### 4.1 Trial (Q-06)

`Login.razor:271`, verbatim:

```csharp
if (!IsDesktop && user.UserId>1 && user != null && user.TrialDays > 0 && user.ExpiryDate.HasValue && DateTime.Today > user.ExpiryDate.Value.Date)
```

Message at `:273`: `"Your trial period has expired. Please contact Administrator."`

**Three carve-outs, all load-bearing:**

| # | Carve-out | Note |
|---|---|---|
| a | `!IsDesktop` | `IsDesktop` is `Configuration["AppEnvironment"] == "Desktop"` (`Login.razor:224`) — a property of the **host**, not the user, and **not** `User.IsDesktop`. The desktop build never enforces the trial. See **Q-37**. |
| b | `UserId > 1` | user 1 is exempt |
| c | `TrialDays > 0` | exempt **even with a past `ExpiryDate`**. The write path only derives an `ExpiryDate` when `TrialDays > 0` (`RegisterUpsert.razor:1062-1068`) |

The `user != null` term is dead — the page already returned for a null user at `:263-268` — and
is tested *after* `user.UserId` is dereferenced. Recorded in KB-060; **not** fixed, because the
Blazor file must not change.

`GetUserTrialAsync` (`IUserRepository.cs:14`; `UserRepository.cs:63-72` before this task added a comment block above it, `:85` after) has **zero call sites**.
Dead code. **Confirmed — negative result, re-run 2026-08-20.**

### 4.2 Device (Q-07)

`Login.razor:277` — `if (user.UserId > 1 && (user.IsMobile || user.IsDesktop))`. A user with
both flags false is **completely unbound**; user 1 is exempt. Inside the gate:

| Condition | Line | Message |
|---|---|---|
| wrong platform (mobile) | 286 | `"Mobile login is not allowed."` |
| wrong platform (desktop) | 306 | `"Desktop login is not allowed."` |
| device mismatch (mobile) | 298 | `"This account is already registered on another mobile device."` |
| device mismatch (desktop) | 318 | `"This account is already registered on another desktop."` |
| first login | 291-294, 311-314 | trust-on-first-use via `UpdateUserDeviceAsync`; no admin approval |

**The device identity is client-asserted** — both the id and the mobile/desktop classification
come from JS interop at `:279-280`. **It is a convenience lock, not an authentication factor**,
and this document says so rather than presenting it as security it is not.

`UserService.UpdateUserDeviceAsync` (`UserService.cs:713-757`) **records and never refuses**: it
sets the device id only if blank (`:730-733`, `:741-744`), always overwrites the name/IP, never
compares, never writes `IsMobile`/`IsDesktop`, and calls `IJSRuntime` four times at `:722-725`
— **so it cannot run in an API request at all**.

### 4.3 What the API does now

| Gate | API before M2-A08 | API after |
|---|---|---|
| Trial | none | **enforced** in `AuthController.Login`, all three carve-outs, message verbatim, `403` with `type: …/trial-expired` |
| Device | none | **evaluator ported and tested** (`Auth/AccountGates.cs`, `DeviceGate`), **not wired** — P4 deferred, Q-38 |
| QR expiry | no QR path exists at all | fixed at the query (§3); still no QR endpoint |
| Row scope | none | mechanism in place (§5); no scoped endpoint exists yet |

---

## 5. The row-scope mechanism, and the alternatives rejected

**The property that mattered:** *a new endpoint over a scoped entity is scoped by default, and
unscoping is an explicit, reviewable act.* Everything else followed from that.

### 5.1 What was built

| Type | Role |
|---|---|
| `Authorization/RowScope.cs` | the caller's scope — a state-code set, plus `Empty` (fail-closed) and `Unrestricted` (the ported `UserId == 1` carve-out). CSV parsing mirrors `LeadService.cs:136-139` |
| `Authorization/IRowScopeProvider` / `RowScopeProvider` | per-request resolution from `User.StateCodesCsv` by **user id**, one row and one column, behind the same memory cache and TTL as the screen rights, key `rowscope:v1:{tenantId}:{userId}` |
| `Authorization/ScopedEntityCatalogue` | the one list of scoped entities and the property each is scoped by. **Exactly one entry today: `Leads`** |
| `Authorization/RowScopeQueryExtensions.ApplyRowScope<T>` | composes `codes.Contains(entity.StateId)` **into the `IQueryable`** — the enforcement |
| `Authorization/RowScopedAttribute` / `NoRowScopeAttribute` | the declaration and the auditable opt-out |
| `Authorization/RowScopeStartupValidator` | **refuses to start the host** if an action serves a scoped entity without declaring one of the two |

An unregistered entity type makes `ApplyRowScope` **throw**, before the unrestricted
short-circuit. Fail loud, never fail open — and never only for user 1.

### 5.2 Alternatives rejected, so this is not re-litigated

| Option | Why not |
|---|---|
| **Sibling scoped/unscoped service methods**, as Blazor has | This is the defect (§2.4), not a design. Four unscoped call sites to one scoped. |
| **EF global query filter on `Leads`** | It lives in `ApplicationDbContext`, i.e. in `V.SMART.Shared`, which the **Blazor host also uses** — it would silently change what the live UI shows, including for `GetAllLeadsAsync`'s four call sites. Out of scope and dangerous. It also cannot see the request's identity without a scoped context service, and `IgnoreQueryFilters()` is a *less* visible opt-out than an attribute. |
| **A `scope` parameter threaded through M2-B02's `PagedQuery`** | Makes scope a *query* concern the client participates in. Anything the client can send, the client can omit. |
| **A scope claim in the JWT** | Forbidden by ADR-004 §2's reasoning and by this task explicitly: a token would outlive a scope change. `JwtTokenService.cs` is unchanged. |
| **Filtering in the controller after materialisation** | The in-memory filter of `LeadService.cs:141-144`, which the task forbids carrying into the API. It also makes a correct paging total impossible. |

### 5.3 What M2-B03 and M2-B08 need to know

- **M2-B03 (controller template):** a list action over a scoped entity calls
  `IRowScopeProvider.GetAsync(tenantId, userId, ct)` — the same two claims the screen-right
  filter reads (`ScreenRightAuthorizationFilter.cs:79`, `:85`) — then
  `.ApplyRowScope(scope)` **before** filter, sort, `CountAsync` and `Skip/Take`, and declares
  `[RowScoped(typeof(T))]`. Scope composes **with** the screen right; it never replaces it. If
  the template does not carry this, every future controller re-invents it.
- **M2-B08 (reports):** report endpoints call stored procedures directly, and **no stored
  procedure references `StateCodesCsv`** (§2.1). `ApplyRowScope` is an `IQueryable` extension
  and **cannot reach a raw stored-procedure call.** A report over a scoped entity therefore
  needs either a new procedure parameter or a post-filter, and either way it is a **new
  predicate invented rather than ported**. Recorded now rather than discovered in M2-C09.

---

## 6. Deliberate differences from the Blazor behaviour

Each is a change; each is here so nobody has to infer it from a diff.

| # | Blazor | API | Why |
|---|---|---|---|
| 1 | current user found by **username string equality** over every user row (`LeadService.cs:132-134`) | by the `UserId` claim, one row, one column | the API has a user id; a name collision must not be a security question |
| 2 | filter runs **in memory** after materialising every lead (`:141-144`) | composed into the query | required by the task; also the only way to count correctly |
| 3 | CSV tokens compared **as strings** to `StateId.ToString()` | parsed to `int` | only an integer set becomes a SQL `IN`. They agree for every well-formed value; they differ only for a **zero-padded** token (`"07"` matches `StateId 7` in the API, not in Blazor) |
| 4 | a non-numeric CSV token silently never matches; `User.StateCodes` (`User.cs:53-64`) would **throw** on it | dropped | an unparseable column must not turn a login into a 500, and must not fail open |
| 5 | paging total computed from the **unscoped** query (`LeadsList.razor:396-401`) | counted **within** scope | decision P7 — matching Blazor would leak a count |
| 6 | `user != null` tested after dereference (`Login.razor:271`) | omitted | dead term; the Blazor file is not touched |

---

## 7. Line numbers that had gone stale in the task file

Reported as the task requires. Everything not listed here re-verified **unchanged**.

| Task-file citation | Current source |
|---|---|
| `AuthController.cs:39-59` (Login) | **`:40-68`** — M2-A06 landed; it returns `ProblemDetails` now, still with no gate |
| `ILeadService.cs:16,28` | **`:24`** (unscoped) and `:28` (scoped). `:16` is `GetUsersBySelectedStateId` |
| claim 4: *"the only two `Pages/` hits are in `LeadsUpsert.razor`"* | **False.** `RegisterUpsert.razor:622,1018,1022,1024,1037,1038,1042` also touch `UserVM.StateCodes` — admin assignment UI, still not row scoping |
| claim 6: *"exactly one call site"* for the scoped method | true — but the **unscoped** sibling has **four**, not the implied one |
| *"KB-100 is the expected `doc_id`"* | **Taken** by M2-B12-01. `INDEX.md` records next free as **KB-108**, which this document claims |
| Dependencies table: *"M2-A04 owns `POST /api/v1/auth/login`"* | M2-A04 is *"Refresh tokens + revocation"* and **Blocked**. No counterparty — see **Q-38** |
| *React Changes* section, `frontend/nexgen-web/` | superseded by **ADR-007** (Angular), 2026-08-20 |

---

## 8. The product decisions — answers, deferrals, owner, date

Owner for every row: **Vivek, the repository owner**. Raised 2026-08-20 by M2-A08.

| # | Decision | Outcome |
|---|---|---|
| **P1** | Does row scope extend beyond `Leads`? | **No — answered by the default, and by evidence.** It has never applied elsewhere (§2.1). `ScopedEntityCatalogue` has one entry, and a test asserts it. Extending it is available and is a product decision with a blast radius. |
| **P2** | Do users with an empty `StateCodesCsv` keep seeing **zero** leads? | **Yes — preserved**, proven by test. Pair it with an explicit empty-state message client-side (§9) so it reads as a configuration gap, not an outage. |
| **P3** | Are the trial carve-outs preserved? | **All three preserved verbatim** and annotated in code with their `Login.razor` lines. `!IsDesktop` **flagged for confirmation** — **Q-37**, unanswered. |
| **P4** | Does the API enforce device binding? | **DEFERRED, unanswered — Q-38.** The evaluator is ported and tested; nothing calls it. Recorded as a decision, not an omission. |
| **P5** | Does fixing `GetUserByQrToken` change behaviour for a current caller? | **No** — both callers already reject expired tokens, and there is no third caller. **Fixed.** |
| **P6** *(new — not in the task file)* | Is the `UserId == 1` unscoped carve-out (§2.5) ported? | **Yes — preserved.** Dropping it takes data away from that account on day one. Resolved **once, in the provider**, from the claim — never per call site. |
| **P7** *(new)* | Is the paging total computed **within** scope? | **Yes — a deliberate behaviour change.** Blazor counts unscoped (`LeadsList.razor:396-401`); matching it would leak a count. Pinned by test. |
| **P8** *(new — the *API Changes* decision the task required)* | `403` or `404` for a direct fetch of an out-of-scope row by id? | **`404`**, with the **same** body a genuinely missing row returns (`ProblemResults.OutOfScopeProblem`). A `403` confirms the row exists. No current behaviour to preserve — Blazor has no per-id lead route that bypasses the list. Applies to **every** scoped resource. |

---

## 9. Consequences for the SPA client

Recorded in [KB-050](../frontend-new/react-architecture.md). Framework-neutral: **ADR-007**
selects Angular, superseding ADR-003's React.

- **The login screen must render each account-gate refusal distinctly.** An expired trial is a
  `403` with `type: …/trial-expired`, not a `401`. "Contact your administrator", never "invalid
  credentials".
- **A scoped list must be able to explain an empty result.** Per P2, an unconfigured user
  legitimately sees zero rows; the empty state must say *why*, or it reads as a bug and
  generates a support ticket every time.
- **Scope is never a client-side filter.** The client may *display* the caller's scope for
  explanation (from `/me`, if M2-A07 exposes it); it must never *apply* it. ADR-004's
  presentation-only rule covers scope exactly as it covers rights.

---

## 10. What is enforced, and what is still open

**Enforced now:** the trial gate on `POST /api/auth/login`; the QR expiry predicate in the
query; the row-scope mechanism, including the startup refusal for an undeclared scoped
endpoint.

**Not enforced, deliberately:** the device gate (P4/Q-38). **Not applicable yet:** there is no
QR endpoint and no Leads endpoint, so the row-scope mechanism has no production caller — it is
in place so the first one cannot get it wrong.

**Still open:** Q-37 (the desktop exemption), Q-38 (the device gate and login-path ownership),
and M2-B08's report predicate (§5.3), which has to be invented rather than ported.
