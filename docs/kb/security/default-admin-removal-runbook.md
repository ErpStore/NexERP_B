---
doc_id: KB-106  # renumbered from KB-104 on merge 2026-08-26; see INDEX.md's collision note
title: Default Administrator Removal — Per-Tenant Runbook (R-09)
module: security
source_files:
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Migrations/20260819095649_RemoveDefaultAdministratorSeed.cs
  - V.SMART/V.SMART.Shared/Migrations/20260217110637_InitialCreate.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Admins/UserRepository.cs
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/UserRights_Pages/UserRights.razor
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserRightService.cs
  - V.SMART/V.SMART.Shared/Data/MasterDbContext.cs
  - V.SMART/V.SMART.Shared/Data/TenantInfo.cs
entities: [User, UserRight, UserAuthority, UserThemePreference, TenantInfo]
api_endpoints: []
database_tables: [Users, UserRights, UserAuthority, UserThemePreference, Tenants]
business_rules: [BR-AUTH-001, BR-AUTH-002]
status: complete
confidence: mixed
last_verified: 2026-08-27
dependencies: [KB-060, KB-030, KB-013, KB-014, KB-004, KB-003]
---

# Default Administrator Removal — Per-Tenant Runbook (R-09)

**Owner of execution: Vivek (repository owner / deployment owner).** Nothing in this runbook
may be executed by an AI session — every step needs database credentials and a production
judgement call that only the deployment owner holds.

> ## EVERY TENANT DATABASE IS AFFECTED
>
> The credential was seeded in `ApplicationDbContext.OnModelCreating`, and V.SMART is
> **database-per-tenant** (KB-014). The seed therefore exists **once in every tenant
> database**, not once for the installation. "All tenants" is literal.

---

## 1. What task M0-06 changed, and what it deliberately did not

| | |
|---|---|
| **Changed in source** | The `builder.Entity<User>().HasData(...)` block was deleted from `ApplicationDbContext.OnModelCreating` (it stood at `ApplicationDbContext.cs:1136-1148` until 2026-08-19). A database created **from the model** (`EnsureCreated`, or any future scaffolding) now starts with **zero** users. |
| **Changed in the migration set** | One new migration, `20260819095649_RemoveDefaultAdministratorSeed`, carrying the updated `ApplicationDbContextModelSnapshot.cs`. |
| **Deliberately NOT changed** | That migration's `Up()` and `Down()` are **empty**. It executes **no DML against any existing database**. Applying it to a live tenant changes nothing and removes nothing. |
| **Deliberately NOT changed** | The historical `20260217110637_InitialCreate` migration still `InsertData`s the administrator row (`InitialCreate.cs:7562`). Migration history is never rewritten. A database built by replaying migrations (`dotnet ef database update` from empty) therefore **still receives the default administrator**, and section 5 of this runbook removes it. |

**Consequence you must internalise before going further:** removing the seed from source does
**not** remove the account from any database that already exists, nor from any database built by
replaying migrations. That removal is the human procedure below.

> **This was an acceptance criterion M0-06 could not meet on its own, and it was escalated, not
> papered over.** The task required that *"no default administrator credential is seeded into a
> newly created tenant database"*. That holds for a database created from the EF **model**, and
> does **not** hold for one created by replaying the **migrations** — which is the only
> provisioning path this repository supports (Q-02: nothing calls `Migrate()`, `MigrateAsync()`
> or `EnsureCreated()`). Closing it needed a decision only the deployment owner could take; that
> decision was written out as **Q-26** in [KB-004](../open-questions.md) and **answered
> 2026-08-27** — see §1a immediately below.

### 1a. Provisioning a new tenant — the mandatory procedure (Q-26, answered 2026-08-27)

**Decision: option (a), an ops procedure. No new code.** `dotnet ef database update` alone is
**never** sufficient to bring up a tenant — it is step 1 of 2, always:

1. **Provision as today.** Run the migrations (`dotnet ef database update`, or whatever
   deployment mechanism Q-02 eventually names). The resulting database holds the published
   `UserId = 1` / `"Administrator"` credential — unavoidably, since `InitialCreate.cs:7562` is
   history and history is never rewritten.
2. **Immediately afterward, before the tenant is reachable by anyone:** follow **§4a** below to
   create a real administrator and verify it with an **actual login** (do not skip the login —
   see Trap 2), then follow **§5** to remove or deactivate `UserId = 1`.

This is exactly the existing §4a→§5 sequence, unmodified — §5's own guard
(`OtherActiveAdministrators >= 1`) is satisfied by §4a's freshly-created and verified
administrator, so nothing about those two sections needed to change for this case. §3's
pre-check is trivially known for a brand-new database (nothing has happened yet: zero rights
rows, zero authority rows, one user) and may be skipped for step 2 above, but must still be run
in full for every **existing** tenant per §3 and §5's own preconditions.

**What this decision does not do.** It makes the procedure *mandatory*, not *self-enforcing* —
nothing in the codebase currently refuses to serve a tenant where an operator skipped step 2.
That is precisely what the rejected option (b), a runtime bootstrap component failing loudly at
startup, would have added. It was not chosen; if the procedural gap proves to matter in
practice, option (b) remains available as a future task, not reopened here.

---

## 2. Open questions this runbook does not resolve

All four are flagged, not assumed away.

| Question | State | Why it matters here |
|---|---|---|
| **Q-02 — how are EF migrations rolled out per tenant?** | **Unknown.** Re-verified 2026-08-19: `git grep --untracked` across all of `V.SMART/` for `.Migrate()`, `MigrateAsync` and `EnsureCreated` returns **zero** hits. Nothing in the application applies migrations at startup. | A new migration **does not reach any tenant by itself**. Whatever mechanism actually deploys schema changes — manual `dotnet ef database update`, a DBA script, or nothing — is unknown to the repository. The owner must state it before section 5 is scheduled. |
| **Q-12 — which tenants are in production?** | **Unknown contents; the enumeration mechanism is Confirmed.** `MasterDbContext.cs:5-9` declares `DbSet<TenantInfo> Tenants`; `TenantInfo.cs:3-9` gives `Id`, `Name`, `Hostname`, `ConnectionString`. The list is a table in the master database. | Section 3's pre-check must be run against **every row** of that table. The repository cannot tell you how many rows there are. |
| **Q-25 — does any tenant still depend on the seeded `Administrator` account?** | **Unknown pending execution of section 3.** Raised by M0-06; see `open-questions.md` (KB-004). | This is the question that decides whether section 5 may run at all. |
| **Q-26 — how must a newly provisioned tenant database avoid the credential?** | **ANSWERED 2026-08-27 — option (a), the ops procedure in §1a above.** Raised 2026-08-19 by M0-06 (attempt 2); see `open-questions.md` (KB-004). | Provisioning is now a documented two-step procedure: run the migrations, then **immediately** run §4a→§5 before the tenant is reachable. No new code. The procedure is mandatory but not self-enforcing — see §1a's closing note. |

---

## 3. Pre-check — read-only, run this FIRST, on every tenant

Enumerate tenants from the master database:

```sql
-- Master database. Read-only.
SELECT Id, Name, Hostname, ConnectionString FROM Tenants ORDER BY Id;
```

Then, **for each tenant database**, run this read-only diagnostic. It changes nothing.

```sql
-- Tenant database. READ-ONLY. Run once per tenant; record the output per tenant.
-- UserRole.Administrator is the first member of the enum (Data/Enum/UserRole.cs:5) and
-- therefore persists as 0 -- corroborated by InitialCreate.cs:7562, which writes 0 for the
-- seeded administrator's Role column.

SELECT
    -- (a) Is the seeded row still present at all?
    (SELECT COUNT(*) FROM Users WHERE UserId = 1)                                AS SeededRowPresent,
    -- (b) Is it still named Administrator and still active?
    (SELECT COUNT(*) FROM Users WHERE UserId = 1 AND UserName = 'Administrator'
                                  AND IsActive = 1)                              AS SeededRowUntouchedAndActive,
    -- (c) THE DECIDING NUMBER. Any OTHER active administrator? If this is 0, the tenant's
    --     only administrator is the seeded account and NOTHING in section 5 may be run
    --     against it until a replacement administrator exists.
    (SELECT COUNT(*) FROM Users WHERE UserId <> 1 AND IsActive = 1 AND Role = 0) AS OtherActiveAdministrators,
    -- (d) Total real users, for context.
    (SELECT COUNT(*) FROM Users)                                                 AS TotalUsers,
    -- (e-g) HOW MUCH A DELETE WOULD DESTROY. All three FKs to Users are ON DELETE CASCADE
    --       (see section 4), so a DELETE of UserId 1 silently removes all of these rows.
    (SELECT COUNT(*) FROM UserRights          WHERE UserId = 1)                  AS RightsRowsForUser1,
    (SELECT COUNT(*) FROM UserAuthority       WHERE UserId = 1)                  AS AuthorityRowsForUser1,
    (SELECT COUNT(*) FROM UserThemePreference WHERE UserId = 1)                  AS ThemeRowsForUser1;
```

**Results: UNKNOWN pending execution.** No session that produced this document has database
access. The numbers must be filled in per tenant by the deployment owner (Vivek, or whoever
holds the master-database credentials) and recorded — in a ticket, or appended to this
document — before section 5 is scheduled.

Reading the output:

| Observation | Meaning | Action |
|---|---|---|
| `OtherActiveAdministrators = 0` | The seeded account is this tenant's **only** administrator. | **STOP.** Do section 4a (create a replacement) first. Do not proceed to section 5 for this tenant. |
| `SeededRowPresent = 0` | Someone already removed it. | Nothing to do for this tenant. Record it. |
| `RightsRowsForUser1 = 0` | `UserId 1` has **never logged in through the Blazor UI**. `Login.razor:345-349` calls `UserRightService.SyncRightsForUserAsync` on *every* login where `user.UserId == 1`, which inserts one `UserRights` row per screen (`UserRightService.cs:62-78`). Zero rows means that hook never fired. *Caveat:* `V.SMART.Api/Controllers/AuthController.cs:47` authenticates **without** calling that hook, so an API-only login would not create rows. | Low-risk case: the delete in section 5 destroys nothing. Still requires `OtherActiveAdministrators >= 1`. |
| `RightsRowsForUser1 > 0` | The account has been used. | Treat it as a live account. Section 5's delete will silently destroy these rows — see section 4. |

---

## 4. The two traps this procedure exists to avoid

### Trap 1 — the delete is NOT blocked; it cascades, silently

The M0-06 task file predicted that `DeleteBehavior.Restrict` would **block** a delete of
`UserId = 1`. **That prediction is wrong, and the source disagrees with it.** All three foreign
keys that reference `Users` are `Cascade`:

| Relationship | Evidence |
|---|---|
| `UserRight.User` | `ApplicationDbContextModelSnapshot.cs:25924-25928` — `OnDelete(DeleteBehavior.Cascade)`, `IsRequired()` |
| `UserAuthority.User` | `ApplicationDbContextModelSnapshot.cs:25937-25941` |
| `UserThemePreference.User` | `ApplicationDbContextModelSnapshot.cs:26422-26426` |
| Physical DDL agrees | `Migrations/20260217110637_InitialCreate.cs:7196-7200` (`FK_UserAuthority_Users_UserId`, `onDelete: ReferentialAction.Cascade`) and `:7232-7236` (`FK_UserRights_Users_UserId`, Cascade) |

The global loop in `ApplicationDbContext.OnModelCreating` rewrites a relationship to `Restrict`
**only if it is not already** `Cascade`/`NoAction`/`Restrict`, and EF Core defaults a required
FK to `Cascade` — so the loop skips all three. The failure mode is therefore **silent data
loss**, not a loud abort. That is worse, and it is why section 5 deletes the dependants
explicitly, inside a transaction, rather than relying on the cascade.

### Trap 2 — `UserId = 1` is a hard-coded superuser; a replacement admin with a different id is locked out

`UserId == 1` is special-cased in three places, none of which match on the *name*
`"Administrator"`:

| Behaviour | Evidence |
|---|---|
| On every login, `UserId == 1` gets `CanView`/`CanCreate`/`CanEdit`/`CanDelete` on **all 152 screens**, auto-created | `Login.razor:345-349` calling `UserRightService.cs:32-87` (inserts a row per screen at `:62-78`) |
| The User Rights screen makes `UserId 1`'s rights **immutable** — every checkbox disabled, save hidden | `UserRights.razor:82,92,102,112,122,132,146,163` and `:179` |
| `TrialDays` and the whole User Device Settings section are visible only to `UserId 1` | `RegisterUpsert.razor:426`, `:444` |

And rights are **deny-by-default**: every accessor in `Shared/RightsHelper.cs:7-20` ends in
`?? false`. Combine the two and you get the lockout nobody expects:

> **A replacement administrator created with any `UserId` other than 1 will authenticate
> successfully and then see an application with every screen denied.** It cannot grant rights to
> itself, because `UserRights.razor:215` requires `CanEdit || CanCreate` to save. Recovery would
> require direct SQL.

**Therefore, when creating a replacement administrator, you must ALSO create its `UserRights`
rows explicitly.** Do not assume the login hook will do it — it only fires for `UserId == 1`.

### 4a. Creating a replacement administrator

Do this before section 5, in any tenant where `OtherActiveAdministrators = 0`.

Preferred route, because it exercises the same code path the application uses and produces a
hash that `IPasswordHasher<User>.VerifyHashedPassword` will accept (BR-AUTH-001,
`UserRepository.cs:34-49`):

1. Log in to the Blazor application **as the existing `Administrator` account** (it still works
   at this point — that is the whole reason section 5 comes last).
2. Create a new user through the normal user-administration screen, with
   `Role = Administrator` and `IsActive = true`, and a password chosen from a password manager.
3. Open **User Rights** for the new user and grant it every screen right it needs. This step is
   **not optional** — see Trap 2.
4. **Verify by logging out and logging in as the new account**, and confirm the navigation menu
   and at least one screen render. An account that authenticates but shows nothing is the
   lockout described above, not a success.

Do **not** hand-write a password hash into SQL. The hash format is ASP.NET Core Identity v3 as
produced by `PasswordHasher<User>` (registered at `V.SMART.Api/Program.cs:106`,
`V.SMART.Web/Program.cs:270`, `V.SMART/MauiProgram.cs:267`); a hand-made value will fail
`VerifyHashedPassword`, and `LoginAsync` swallows the exception and returns `null` (R-19,
`UserRepository.cs:44-48`), so the symptom will be an unexplained "invalid username or password"
rather than an error.

---

## 5. Removal — per tenant, supervised, after sections 3 and 4

**Preconditions, all mandatory, per tenant:**

- [ ] Section 3 has been run against **this** tenant and the numbers recorded.
- [ ] `OtherActiveAdministrators >= 1`, **and** that other administrator has been verified by an
      actual login (section 4a step 4).
- [ ] A full database backup of this tenant exists and its restore has been at least spot-checked.
- [ ] A maintenance window, or an accepted risk of doing it live.

**The statement.** Run inside an explicit transaction, one tenant at a time, and read the row
counts before committing.

```sql
-- Tenant database. DESTRUCTIVE. One tenant at a time.
BEGIN TRANSACTION;

-- Guard: refuse to proceed if this tenant has no other active administrator.
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId <> 1 AND IsActive = 1 AND Role = 0)
BEGIN
    RAISERROR('ABORT: this tenant has no active administrator other than UserId 1.', 16, 1);
    ROLLBACK TRANSACTION;
END
ELSE
BEGIN
    -- Delete dependants EXPLICITLY rather than relying on the cascade, so the row counts are
    -- visible and auditable rather than silent. See section 4, Trap 1.
    DELETE FROM UserRights          WHERE UserId = 1;   -- expect: RightsRowsForUser1
    DELETE FROM UserAuthority       WHERE UserId = 1;   -- expect: AuthorityRowsForUser1
    DELETE FROM UserThemePreference WHERE UserId = 1;   -- expect: ThemeRowsForUser1
    DELETE FROM Users               WHERE UserId = 1;   -- expect: 1

    -- REVIEW THE ROW COUNTS ABOVE AGAINST THE SECTION 3 NUMBERS BEFORE COMMITTING.
    -- COMMIT TRANSACTION;   -- uncomment only after the counts match
    -- ROLLBACK TRANSACTION; -- otherwise
END
```

**Alternative, if removal is judged too risky for a given tenant:** deactivate instead of
deleting. This preserves all dependent rows, is trivially reversible, and satisfies
BR-AUTH-001's `IsActive` filter (`UserRepository.cs:38` — `u.UserName == username && u.IsActive`),
so the account can no longer authenticate through **either** the Blazor login or
`V.SMART.Api/Controllers/AuthController.cs:47`.

```sql
-- Tenant database. Reversible alternative to deletion.
UPDATE Users SET IsActive = 0 WHERE UserId = 1 AND UserName = 'Administrator';
```

Deactivation is the recommended default for any tenant where `RightsRowsForUser1 > 0`.

---

## 6. Verification — per tenant, after section 5

```sql
-- Tenant database. Read-only. All three must hold.
SELECT COUNT(*) AS MustBeZero_ActiveDefaultAdmin
FROM Users WHERE UserName = 'Administrator' AND IsActive = 1;

SELECT COUNT(*) AS MustBeAtLeastOne_OtherActiveAdmins
FROM Users WHERE UserId <> 1 AND IsActive = 1 AND Role = 0;

SELECT COUNT(*) AS MustBeZero_OrphanRightsRows
FROM UserRights r WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = r.UserId);
```

Then, manually:

1. Attempt to log in as `Administrator` with the known default password. It **must** fail.
2. Log in as the replacement administrator. It **must** succeed **and render screens**.

---

## 7. Rollback

**Rolling back the code does not roll back the data.** Reverting the M0-06 commit restores the
`HasData` block in source; it does **not** re-create a row already deleted from a deployed tenant
database, because nothing in the application applies migrations (Q-02) and the new migration's
`Up()` was a no-op anyway.

| What went wrong | Rollback |
|---|---|
| Section 5 was run and the tenant is now locked out | **Restore the tenant database from the backup taken in section 5's preconditions.** This is the only complete rollback. |
| Section 5's deactivation route was used and needs reversing | `UPDATE Users SET IsActive = 1 WHERE UserId = 1;` — fully reversible, no data was lost. |
| The row was deleted and no backup exists | Create a **new** administrator directly, then repeat section 4a's rights grant. Do not attempt to re-insert `UserId = 1` with the old hash: that hash is published in this repository's git history and recreating it reintroduces R-09. If `UserId = 1` must exist for the superuser hooks in Trap 2, insert it with a **freshly hashed** password via `SET IDENTITY_INSERT Users ON`, and grant rights explicitly. |
| The code change itself must be reverted | `git revert` the M0-06 commit. The migration `20260819095649_RemoveDefaultAdministratorSeed` is removed with it; if any tenant has recorded it in `__EFMigrationsHistory`, delete that history row too, or the next `database update` will complain about a migration it cannot find. |

---

## 8. What this does and does not close

| | |
|---|---|
| **Closed** | New databases created **from the model** no longer carry a default credential. |
| **Closed** | The PBKDF2 hash no longer appears anywhere in the working tree outside pre-existing historical migration files. |
| **NOT closed** | The hash remains in this repository's **published** git history (the remote is public — INV-029). Only **M0-05** removes it, and it must be assumed already harvested. |
| **NOT closed** | Every existing tenant database still contains the account until section 5 is executed **per tenant**, by a human. |
| **NOT closed** | Databases created by replaying migrations from `InitialCreate` still receive the row (section 1). |
| **NOT addressed** | There is still no bootstrap component that creates the first administrator on an empty database. See "Option A, deferred" under R-09 in `technical-debt-register.md` (KB-060). |

R-09 therefore stays **open** in KB-060.
