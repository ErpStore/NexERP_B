---
doc_id: KB-109
title: Decision Brief — Q-28 (API-only users hold no rights) and R-65 (two phantom screen names)
module: decisions
source_files:
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Authorization/ScreenCatalogue.cs
  - V.SMART/V.SMART.Api/Authorization/ScreenRightStartupValidator.cs
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserRightService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/AdminService/UserService.cs
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
entities: [User, UserRight, Screens]
api_endpoints: ["POST /api/v1/auth/login"]
database_tables: [Users, UserRights, Screens]
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-24
dependencies: [KB-004, KB-060, KB-081, ADR-004]
---

# Decision Brief — Q-28 and R-65

> **For: Vivek. Two decisions, one dependency chain.** Between them they gate **three of the six
> G2 exit criteria**. Everything below is traced to code read on 2026-08-24; where something is
> inferred rather than observed, it says so.
>
> **This brief recommends but does not decide.** Both questions are owner-only.

## Why these two, and why together

```
Q-28 + R-65  →  M2-A02  →  M2-A03  ──────────────→  G2 criterion 3
                   └──→  M2-B03  →  M2-B10  ──────→  G2 criteria 4 and 6
```

`M2-A02` ("apply the screen-right filter to `CurrencyController` and prove denial") is the only
`Ready` task in the repository that fails purely on an unanswered question. It is blocked by
**both** of these, and nothing downstream of it moves until it lands.

They are briefed together because **they fail in opposite directions and the fix for one can mask
the other**: Q-28 leaves a legitimate user with *no* rights rows, and R-65 lets an endpoint demand
a right that *cannot exist*. Both produce the same symptom — `403` forever — and both are silent.

---

## Q-28 — a user who only ever uses the API acquires no rights, ever

### What the code does

**Rights are seeded on the Blazor login path only, and only for one user.**
`Login.razor:345-348`:

```csharp
if (user.UserId == 1)
{
    await LoggingService.LogDeveloperInfo($"[UserRights] Administrator login detected. Syncing rights…");
    await userRightService.SyncRightsForUserAsync(user.UserId);
}
```

`AuthController.Login` (`AuthController.cs:39-59`) contains no equivalent call. **Confirmed** by
reading both paths.

**There are two seeding routines and they disagree.**

| Path | When it runs | What it writes |
|---|---|---|
| `SyncRightsForUserAsync` (`UserRightService.cs:32-80`) | Blazor login, `UserId == 1` only | A row per *missing* screen with `CanView/CanCreate/CanEdit/CanDelete = true`, `IsHide = false` |
| `UserService` (`UserService.cs:442-464`) | User creation | Rows **only if** `vm.IsViewOnly`, and then `CanView = true`, everything else `false` |

So a non-`IsViewOnly` user created through the UI gets **no rows at all**, and only `UserId == 1`
is ever back-filled — on a Blazor login it may never perform.

### What happens when M2-A02 lands

ADR-004's filter is deny-by-default. An administrator who has only ever authenticated through the
API holds zero `UserRight` rows and therefore receives **403 from every annotated endpoint**.

**This is pre-existing legacy inconsistency that server-side enforcement makes visible — not a
defect M2-A01 introduced.** That distinction matters for how it is fixed: nothing has regressed,
so there is no "restore previous behaviour" option. Previous behaviour was that the API enforced
nothing at all.

### Options

| | Option | What it means | Cost | Risk |
|---|---|---|---|---|
| **A** | **Call `SyncRightsForUserAsync` from `AuthController.Login`, gated as Blazor gates it (`UserId == 1`)** | The API mirrors the Blazor path exactly. Administrator self-heals on first API login; nobody else changes. | ~1 h | **Low.** Reuses a routine already in production on the Blazor path. Does not touch non-admin users. |
| **B** | **Call it for every user on first API login** | Any user missing rows gets a full-rights row per screen. | ~1 h | **High — do not choose without reading this.** `SyncRightsForUserAsync` writes **all four operations `true`**. Applied to every user it is a silent privilege escalation: a view-only clerk would receive create, edit and delete on all 150 screens. |
| **C** | **Seed rights at user creation instead, for all users** | Fix `UserService.cs:442-464` so every user gets rows, with defaults matching their role. | ~1 d | **Medium.** Correct long-term shape, but it does not help the users who *already* exist with no rows — a back-fill is still needed. |
| **D** | **Decide it is out of scope for M2-A02; unblock by narrowing the task** | `M2-A02` proves denial with a permission-less user, which works *because* rights are absent. Q-28 becomes its own task. | ~0 | **Low for the gate, medium for the product.** Unblocks three G2 criteria immediately, but leaves a real operational trap for whoever first uses the API in anger. |

### Recommendation

**A, with D as the sequencing.** Option A is a faithful mirror of behaviour already trusted in
production and is genuinely an hour's work; it removes the trap without inventing policy. But
`M2-A02`'s own acceptance criteria do not *require* Q-28 to be solved — the task proves that a
permission-less user is denied, and that is true either way. So:

1. Answer Q-28 with **A** as the intended fix.
2. Let `M2-A02` proceed **now**, noting Q-28's answer rather than implementing it.
3. Implement A as a small follow-up task before any real API user exists.

**Explicitly not recommended: B.** It is the option that looks like "just make it work" and it
grants delete rights on 150 screens to every user in the database.

---

## R-65 — two screen names exist in the catalogue and in no database

### What the code does

`ApplicationDbContext.cs:1151` seeds **152** `Screens` rows. Later migrations delete **two** —
`ScreenCode` 114 and 115, `Bill Pending List` and `Bill Paid List`. Every real database holds
**150**.

`V.SMART.Api/Authorization/ScreenCatalogue.cs` still lists **all 152**, including both phantoms at
lines **146-147**. **Confirmed** by direct inspection 2026-08-24: the file contains 152 quoted
names and both strings are present.

### Why the existing guard does not save you

`ScreenRightStartupValidator.cs:98` validates declared screen names against
`ScreenCatalogue.SeededScreenNames` — the same 152-name list. So:

```
[RequireScreen("Bill Paid List")]
   → passes startup validation      (the name IS in the catalogue)
   → no Screens row exists           (deleted by migration)
   → no UserRight row can exist      (FK is to Screens.Id)
   → deny-by-default fires
   → 403 for every user, forever, with no error and no startup warning
```

**The validator gives false assurance precisely here.** It is designed to catch typos, and it
would catch `"Bil Paid List"`. It cannot catch a name that is real in the catalogue and absent
from the database, because it never consults the database.

### Blast radius, stated honestly

**Zero, today.** No endpoint currently carries either annotation — this is a trap, not an active
fault. It becomes real the moment someone annotates a billing endpoint with a name that looks
correct because it is in the catalogue.

### Options

| | Option | What it means | Cost | Risk |
|---|---|---|---|---|
| **A** | **Delete the two names from `ScreenCatalogue.cs`** | The catalogue becomes the 150 that exist. The validator then rejects `[RequireScreen("Bill Paid List")]` at startup, loudly. | ~15 min | **Very low.** Two lines. The failure mode inverts from silent-forever to loud-at-startup. |
| **B** | **Generate the catalogue from the database** | A build step derives `ScreenCatalogue.cs` from `Screens`, so drift cannot recur. | ~1 d | **Medium.** Right long-term answer and it fixes the *class*, not the instance. Needs a reachable database at build time — which this workstation does not have (the same gap blocking `M2-C10`). |
| **C** | **Make the startup validator query the database** | Validation checks names against live `Screens` rows rather than the constant. | ~0.5 d | **Medium.** Catches drift in both directions at every startup, but moves a startup dependency onto the database and would fail the host on an unreachable one. |
| **D** | **Leave it; rely on reviewers** | Document the trap and move on. | 0 | **High relative to cost.** A 15-minute fix versus a defect whose signature is "one screen is permanently inaccessible to everyone" — the hardest kind to diagnose, because nothing errors. |

### Recommendation

**A now, B later.** Deleting two lines converts a silent permanent lockout into a startup failure
with an exact message, which is the whole of the risk for 15 minutes of work. **B** is the real
fix for the *class* of problem and belongs with `M2-B10`'s generated-artefact work — the same
brief already argues for a generated, database-derived constants class (see
`server-side-authorization-spec.md` §1.3, which notes it "would serve that need *and* fix R-65").

**Do not adopt C alone**: it trades a silent lockout for a host that will not start when the
database is briefly unreachable, which is a worse operational property.

---

## What answering both unblocks

| Answered | Immediately selectable | Then |
|---|---|---|
| Q-28 + R-65 | **`M2-A02`** (P0, 1 d) | `M2-A03` → **G2 criterion 3**; `M2-B03` → `M2-B10` → **G2 criteria 4 and 6** |

Three of six G2 exit criteria sit behind this one decision point. The other three run through the
`M2-C`/`M2-D` tree, which is specification-ready but gated on **`M0-04`**.

## The minimum you have to say

If the recommendations are acceptable, the smallest sufficient answer is:

> **Q-28 — A, deferred.** Mirror the Blazor seeding call in `AuthController.Login`, gated on
> `UserId == 1`, as a follow-up task. `M2-A02` proceeds now.
>
> **R-65 — A.** Delete `Bill Pending List` and `Bill Paid List` from `ScreenCatalogue.cs`.
> Generated catalogue (B) deferred to `M2-B10`.

That unblocks `M2-A02` and, through it, three G2 criteria. Anything more detailed is welcome but
not required to proceed.

## Open items this brief does not resolve

- **R-65 is fully recorded** in [KB-060](../risks/technical-debt-register.md) as section `### R-65`
  (line 1572), confirmed by measurement against two databases during the M0-01-03 rebuild drill.
  An earlier draft of this brief claimed it had no register entry; that was wrong — the register
  uses `### R-NN` sections, not table rows, and the check used the row format.
- **Whether `UserId == 1` is the right gate at all.** Both Q-28 option A and today's Blazor path
  assume administrator means `UserId == 1`. That is a magic number, and no evidence was found
  that it is guaranteed. Out of scope here; worth a question of its own if option A is chosen.
