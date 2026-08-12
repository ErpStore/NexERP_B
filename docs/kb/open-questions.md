---
doc_id: KB-004
title: Open Questions and Unknowns
module: meta
status: active
confidence: n/a
last_verified: 2026-08-12
---

# Open Questions and Unknowns

Each entry states who can answer it, why it matters, and what it blocks. **Nothing here has
been assumed away in the analysis** — where a question affects a recommendation, the
recommendation says so.

## Product / business decisions (need the product owner)

| ID | Question | Why it matters | Blocks |
|---|---|---|---|
| ~~**Q-19**~~ **CORRECTED 2026-08-12 (INV-034)** | ~~Is the public visibility of `https://github.com/ErpStore/NexERP_B.git` intended?~~ **The repository is PRIVATE, not public — the original premise was wrong.** The `git ls-remote` "succeeds without authentication" result that INV-029 relied on was an artefact of Windows Git Credential Manager (`credential.helper = manager`) silently authenticating with the owner's cached credentials; anonymous access was never actually tested. Re-tested 2026-08-12 with the credential helper disabled: git demands a login and an unauthenticated REST call 404s (GitHub's standard response for a private repo). Commit `c12c5b2` still contains the SA password, the production host `154.61.76.112,1533` and the `bspl` production credential in four files — still requires rotation and a history purge — but they were not published to the public internet. Escalated in writing 2026-08-12 (M0-00) to **Kumar**, confirmed owner of `ErpStore/NexERP_B`, before the correction was found; the correction was also delivered to Kumar the same day. See [KB-085](execution/M0-00-baseline-decisions.md#repository-visibility-correction-inv-034). | Determines whether the exposed credentials must be treated as already harvested by third parties, and whether an incident response — not just a rotation — is required. **Resolved: no internet-wide incident response is needed; rotation (M0-04) still is.** | ~~M0-04, immediately. Also M0-05~~ Answered |
| **Q-01** | Is the silent stock under-issue (R-07 / BR-STK-002) a bug or relied-upon behaviour? `TrackStockUsageAsync` errors only when *no* batch exists; if batches exist but total balance is short, it allocates what it can and returns success, leaving the ledger unbalanced. | Determines whether the API tightens or preserves the behaviour. Tightening could block legitimate back-dated entry; not tightening perpetuates ledger drift. | Phase 4.2 (Inventory), and the `IStockManagerService` test suite |
| **Q-09** | Is the `/register` route still wanted? Self-registration into a multi-tenant ERP with a per-screen permission model is unusual. | Determines whether the React app has a public registration surface at all. | Phase 2 auth |
| **Q-11** | What is the intended future of the MAUI desktop/mobile app? Is it decommissioned once React is responsive, or does it remain for offline shop-floor use? | Determines whether the domain layer must keep supporting a third host, and whether shop-floor React screens need offline capability. | Phase 6, and the shop-floor Production Log design (4.4) |
| **Q-12** | Which tenants are in production, and what are their data volumes? Five template folders exist (`acucom`, `sns`, `srinuenggind`, `sharadaelectrou1`, `default`) — is that the full list? | Sizing for performance testing, pilot-tenant selection, and migration sequencing. | Phase 5 load testing, Phase 6 rollout |
| **Q-13** | Is there a feature freeze on the Blazor app during migration? | Two UIs over one database diverge quickly if new features land only in Blazor. | Phase 3 onward |

## Technical unknowns (need code, database, or ops access)

| ID | Question | Current state | Blocks |
|---|---|---|---|
| **Q-02** | How are EF migrations currently rolled out to each tenant database? | No deployment script anywhere in the repo. **Unknown.** | Phase 0 (fresh-environment rebuild), Phase 6 |
| **Q-03** | What indexes actually exist in the production tenant databases? | Only one index-related migration (`AddInDexingToCustomer`); the EF model is not necessarily the deployed truth. **Unknown.** | R-13, Phase 5 |
| **Q-04** | Which e-Invoice/e-Way gateway does each tenant use — Alankit or eRaahi? How is it selected? | Both are wired in `E_Invoice/`. Selection is **Inferred** to be configuration- or tenant-driven, not confirmed. | INV-015, Phase 4.5 |
| ~~**Q-05**~~ **ANSWERED 2026-08-12** | Is `QrExpiryDate` enforced anywhere? | **Yes — but not in the query.** `UserRepository.GetUserByQrToken:52-60` still returns expired users (re-verified). Expiry is checked *post-query*, in **two duplicated copies**: `QrLogin.razor:50-56` and `Login.razor:422-429`. The earlier note "no other enforcement point found" was wrong. Correctness depends on which caller checks → the API must enforce it in the query or the service. | R-16, R-38, M2-A08 |
| ~~**Q-06**~~ **ANSWERED 2026-08-12** | Where are `User.TrialDays` / `User.ExpiryDate` enforced? | **`Login.razor:271-275` only** — Blazor `@code`, not the service. Three carve-outs: `!IsDesktop`, `UserId > 1`, `TrialDays > 0` (so an expired user with `TrialDays == 0` is exempt). Contains a dead `user != null` check placed *after* `user.UserId` is dereferenced. **`GetUserTrialAsync` has zero call sites — dead code.** | R-38, M2-A08 |
| ~~**Q-07**~~ **ANSWERED 2026-08-12** | Are device bindings enforced, or only recorded? | **Both, badly.** `Login.razor:277-322` gates on them (only when `UserId > 1 && (IsMobile \|\| IsDesktop)`, trust-on-first-use), but **device identity is client-asserted** via `deviceHelper.getDeviceId`/`isMobile`. `UserService.UpdateUserDeviceAsync:713-757` only **records** — never compares, never refuses, never writes `IsMobile`/`IsDesktop` — and calls `IJSRuntime` directly, so it **cannot run in an API request**. Not portable as-is. | R-38, M2-A08 |
| ~~**Q-08**~~ **ANSWERED 2026-08-12 — premise was wrong in both directions** | Where is `User.StateCodesCsv` row-level scoping applied? | **It does NOT scope customers or vendors anywhere** (Confirmed negative result). Grepping `StateCodesCsv\|StateCodes\b` across `V.SMART/` hits only write/audit paths — `User.cs:51,53-64`, `UserVM.cs:108-113`, `RegisterModel.cs:58`, `UserMapping.cs:20-39`, `UserService.cs:373,437,491,532-535` — plus `LeadService.cs:41-44,136` and `LeadsUpsert.razor:695-698,826-827`. The only two `Pages/` hits populate an **assignable-user picker**, not row security. **Real scope exists on `Leads` only**, in the *service* layer: `LeadService.GetAllLoadLeadsAsync:128-152`, one call site (`LeadsList.razor:483`). **The real risk is that it is opt-in:** `GetAllLeadsAsync:95-106` on the same interface (`ILeadService.cs:16,28`) returns every lead **unscoped** — so which method a controller calls decides whether scoping happens. It also filters **in memory** (`:133,141-144`), resolves the user by **username string equality** (`:132-134`), fails **closed** on a blank `StateCodesCsv` (zero leads, `:136-142`), and has a second divergent SQL implementation at `:35-47`. | INV-028, M2-A08 |
| **Q-10** | Do the document-number columns carry unique constraints **in the live tenant databases**? | **Wording corrected 2026-08-12.** "Not visible in the EF model" was wrong: `ApplicationDbContext.cs:579-581` declares `MfgQuote(QuoteNo, Suffix)` unique (confirmed in `ApplicationDbContextModelSnapshot.cs:19817-19818`). That is the **only** document-number unique index in the model — the snapshot's five other `IsUnique()` calls cover `User.EmailId`, `User.UserName`, `AssmblyDef`, `AssemblyDefLabour` and an `InspectId`. So 1 of ~40 document types is protected in the model, and whether even that reached the deployed databases is unknown (Q-02: no migration rollout procedure is documented). If absent, R-12 may **already** be producing duplicates — so the check must also **detect existing duplicates**, not just verify constraints. Note `MfgDcService.IsDuplicateDcNoAsync:771-790` scopes by `CustId`, so duplicates must be counted under both scopings. | R-12, M2-B12-02 |
| **Q-14** | Do the 94 stored procedures differ between tenant databases? | If they have drifted per tenant, scripting them (INV-027) is not a single-artefact job. | **Phase 0 — INV-027** |
| **Q-15** | Are there scheduled jobs outside the application (SQL Agent jobs, Windows Task Scheduler) doing ERP work? | The application has **no** background processing (INV-022 confirmed). Anything periodic must therefore live outside it — but nothing was found in the repo. | Phase 6 completeness |
| **Q-16** | What are the actual reverse-proxy / deployment topology and TLS termination? | **Inferred** from `UseForwardedHeaders` with cleared `KnownNetworks`/`KnownProxies` and per-tenant subdomains. Not confirmed. | Phase 6, CORS and cookie configuration |
| **Q-17** | Are `ScreenManagement` rows (distinct from `Screens`) used at runtime, and how do the two relate? | Both are seeded; `Screens` is clearly the permission catalogue. `ScreenManagement`'s role is **Unknown**. | Admin module (3.3) |
| **Q-18** | What is the retention/backup policy for tenant databases and for the flat-file logs? | **Unknown.** Relevant to R-23 and to rollback planning. | Phase 6 |

## Questions the analysis answered — recorded so they are not re-asked

| Question | Answer | Source |
|---|---|---|
| Can the existing backend serve a React SPA without modification? | No — but additively, not by rewriting. Six specific additions required. | [KB-041](api/api-readiness-assessment.md) |
| Is there an existing API to build on? | 6 endpoints, 2 controllers. `CurrencyController` is a valid template. | [KB-040](api/api-overview.md) |
| Where is business logic? | Split: 128,518 LOC in services (reusable) and ~184,000 LOC inside Razor `@code` (must be extracted). | [KB-011](architecture/backend-architecture.md), [KB-015](architecture/frontend-architecture-existing.md) |
| Is authorization enforced server-side? | **No.** UI only. Hard blocker. | [KB-013](architecture/auth-and-permissions.md), [ADR-004](decisions/ADR-004-server-side-authorization.md) |
| Are there background jobs? | None. | INV-022 |
| Are there tests or CI? | None. | INV-023 |
| How is multi-tenancy done? | Database per tenant; host- or JWT-resolved. | [KB-014](architecture/multi-tenancy.md) |
| Should reporting be rebuilt? | No — keep FastReport + stored procedures, expose over HTTP. | [ADR-005](decisions/ADR-005-reporting-and-printing.md) |
| Should the Angular pilot be converted? | No — archive it. It is a 1-screen learning spike. | [KB-015](architecture/frontend-architecture-existing.md#the-angular-19-pilot-frontendvsmart-erp) |
