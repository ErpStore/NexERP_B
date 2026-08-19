---
doc_id: KB-107
title: Milestone Review — M0 Stabilise (Gate G0)
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-19
dependencies: [KB-080, KB-081, KB-084, KB-087]
---

# Milestone Review — M0 Stabilise

**Gate:** G0 · **Reviewed:** 2026-08-19 · **Chair:** Vivek (repository owner)

Recorded per [KB-084 § Milestone Review](review-templates.md#milestone-review). A milestone is
not complete because its tasks are complete — it is complete when its gate passes **and this
review is recorded**.

---

## 1. Gate status

| Criterion (verbatim from [KB-080 § Exit Gate — G0](README.md#exit-gate--g0)) | Met | Evidence |
|---|---|---|
| A fresh, empty SQL Server can be brought to a working tenant database **from source control alone** and the app runs against it | **DEFERRED** | Runbook, deploy script and log skeleton all committed; `db/REBUILD-DRILL-LOG.md` every field `TBD`. ~~Blocked on hardware, not work~~ — **struck 2026-08-19: a SQL Server Express instance was already present on the workstation and had been all along. Blocked on running the drill, which is work. See § 7 Correction** |
| No connection string or JWT secret in the working tree **or** in `git grep … HEAD` | **DEFERRED** | Depends entirely on `M0-05` → `M0-04` |
| Exposed credentials rotated, confirmed by the person with production access | **DEFERRED** | `M0-04` `Blocked`; no owner with production SQL / GST gateway access identified |
| Repository visibility deliberately decided and recorded | ✅ | Public by deliberate owner decision 2026-08-12; [KB-085](M0-00-baseline-decisions.md) via INV-034 |
| CI green on `master`, running on every push, with a recorded warning baseline | ✅ | `master` pushed `44e3614..20be92f`; run on `master` **green**, owner-confirmed. Triggers `on: push: branches: ['**']` |
| `CalculationService` and `StockManagerService` characterisation tests passing in CI | ✅ | 79 tests green on the hosted runner. Suite grew 0 → 11 → 36 → 73 → 79 |
| Q-01 answered and recorded in [open-questions.md](../open-questions.md) | ✅ | Owner decided **preserve but surface**, 2026-08-19, against a baseline pinned by `M0-13` |

### Gate verdict: **PASSED WITH EXCEPTIONS**

Four of seven met. Three deferred by explicit owner decision, recorded below.

> **The template requires that "exceptions require a named owner and a date. 'We'll get to it'
> is a failed gate."** The owner is named for all three. **Dates are not yet set for any of
> them** — criteria 2 and 3 have a target anchor ("end of the milestone") but no calendar date,
> and criterion 1 has neither. **This review is therefore recorded as incomplete against its own
> template on that one point**, rather than inventing dates that were never agreed. Setting them
> is the outstanding action on this gate.

### The exceptions

| # | Criterion | Owner | Target | Real consequence if it slips |
|---|---|---|---|---|
| 1 | Rebuild drill | **Vivek** | **not set** | No evidence a tenant database can be rebuilt from source. Every environment stays a snowflake. Bites hardest at **M6**, when production must be built from scratch — the point of maximum cost to discover it does not work |
| 2 | No secrets in tree or history | **Vivek** | "end of milestone" — **no date** | Follows 3; cannot proceed without it |
| 3 | Credentials rotated | **Vivek** (needs an ops/infra person who is *still unidentified*) | "end of milestone" — **no date** | **Live exposure, not hypothetical.** R-01: production database credentials sit in a public repository's history. `M0-05` cannot fix this alone — purging history does not retract what is already cloned, forked or cached. **Rotation is the only remedy** |

---

## 2. Schedule

| | Planned | Actual | Variance |
|---|---|---|---|
| Duration | 2–3 weeks | **8 days** (2026-08-12 → 2026-08-19) | **~1–2 weeks under** |
| Tasks | 24 M0 rows | 16 `Completed`, 1 `Needs Review`, 3 `Ready`, 2 `Blocked`, 2 parent containers | 5 not finished |

**Why the variance — stated honestly, because a favourable variance is the easiest kind to
misread.** The calendar duration is genuinely short, but it is **not** evidence that the
milestone was cheap:

1. **The gate did not fully pass.** Three of seven criteria are deferred. Comparing 8 days
   against a 2–3 week estimate for *all seven* is not like-for-like.
2. **Every task that needed a human outside the repository stalled and is still stalled.**
   `M0-04` has had no identified owner since 2026-08-17. That is not speed; it is a category of
   work the schedule never started.
3. **The autonomous runner did the volume.** Five tasks were implemented, validated and merged
   in a single day (2026-08-19): `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`. This is
   the real finding — where work is repository-bounded, throughput is high; where it needs
   access, credentials or a decision, throughput is zero regardless of tooling.

**The honest lesson for M2's 6–8 week estimate:** do not scale it down by the M0 ratio. M0's
speed came from tasks that needed nothing outside the repository. M2 needs a React toolchain, a
running API, and product decisions about screen behaviour.

---

## 3. Tasks

| Task | Planned | Actual | Status |
|---|---|---|---|
| M0-00, M0-01-01, M0-01-02, M0-02, M0-03, M0-03-01/02/03, M0-05*, M0-07, M0-08, M0-14, M0-15 | — | — | `Completed` |
| M0-12-01 | 0.5 d | 3 dispatches (2 lost to upstream `529`), 1 real | `Completed` — first test project in the repository |
| M0-13 | 3 d | 1 attempt, `PASS` | `Completed` — 25 tests |
| M0-12-02 | 2.5 d | 1 attempt, `FAIL` on a push-only criterion | `Completed`, criterion 8 waived |
| M0-09 | 0.5 d | 1 attempt, `PASS` | `Completed` — 2 dead guards fixed |
| M0-01-03 | — | repo work done, drill not run | `Needs Review` |
| M0-06 | 1 d | 2 attempts, escalated | `Blocked` on **Q-25/Q-26** |
| M0-04 | — | never started | `Blocked` — deferred |
| M0-10, M0-11, M0-06 | — | — | `Ready` / `Blocked`, carried into M2 |

**Not completed, and where they moved:** `M0-04` and `M0-05` → deferred to end of M2 by owner
decision. `M0-01-03` → blocked on hardware. `M0-06` → blocked on Q-25/Q-26, a deployment
decision. `M0-10` and `M0-11` → `Ready`, carried into M2 as M0 debt.

---

## 4. Outstanding risks

| Risk | Severity | Owner | Carried into |
|---|---|---|---|
| **R-01** — production DB credentials in a public repository's history, unrotated | **Critical** | Vivek + unidentified ops | M2, via deferred criteria 2 and 3 |
| **No rebuild evidence** — nobody has proven a tenant database can be reconstructed from source | **High** | Vivek | M2, surfacing at M6 |
| **R-40** *(new, this milestone)* — `UserId == 1` is an undeclared superuser, auto-granted all 152 screen rights by `Login.razor:345-349` | **High** | — | **M2-A01-02 directly** — it contradicts KB-105's decision D-5 |
| **R-08 wider than catalogued** *(new)* — a third compute-one/test-another guard found at `MfgPoService.cs:613-615`, unfixed | Medium | — | `M0-10` |
| **R-07 pinned, not fixed** — silent stock under-issue asserted as current behaviour | Medium | Vivek | Implementation of "preserve but surface", **no task id yet** |
| **Id allocation collides across branches** *(new, process)* — six ids duplicated between two branches; `grep`-before-claim cannot see a sibling branch | Medium | — | M2, where parallel branches are the norm |
| **No required status check on `master`** — CI does not gate merges; a direct push bypassed the PR rule | Medium | Vivek | Q-20's remaining half |

---

## 5. Open questions

| Q | Answered? | Answer / still blocking |
|---|---|---|
| **Q-01** | ✅ | **Preserve but surface.** Implementation deferred past M2; no task id yet |
| **Q-14** | ✅ | Explicitly deferred by the owner (`M0-02`) |
| **Q-20** | **Partly** | Hosted runners available and approved — proven. Owner holds admin rights — proven by the bypass. **Still open:** no required status check |
| **Q-21** | ✅ | Dispatch layer healthy; both empty returns were transient `529 Overloaded`, confirmed per agent |
| **Q-22** | ✅ | Push authorised by the owner; CI red/green loop completed |
| **Q-23, Q-24** | ❌ | `M0-12-02`'s calculation questions — need the product owner |
| **Q-25, Q-26** | ❌ | **Block `M0-06`.** Is `UserId=1` some tenant's only administrator; what is the tenant-provisioning path |
| **Q-27, Q-28, Q-29** | ❌ | From `M2-A01-01`. **Q-28 blocks `M2-A02`** — an API-only user acquires no rights today |
| **Q-02** | ❌ | How EF migrations reach each tenant. Still `Unknown`; feeds Q-26 |

---

## 6. Documentation

- [x] KB documents updated, `last_verified` bumped
- [x] Investigation registry rows added/amended — INV-031, INV-036, INV-037
- [x] New business rules have `file:line` evidence and BR ids
- [x] As-is and proposal documents still strictly separated
- [x] KB-081 tracker reflects reality

---

## 7. Verdict and what M2 inherits

**M0 passes G0 with three exceptions.** M2 may open.

What M2 inherits that is *not* normal starting state, and should not be forgotten because the
gate says "passed":

1. **Unrotated, publicly exposed production credentials** (R-01). No date set.
2. **No proof the database can be rebuilt from source.** No date set.
3. **`M0-06` unfinished** — the seeded default administrator is removed from the model but
   still ships in `InitialCreate.cs`, and R-40 means a replacement admin would authenticate
   into an empty UI.
4. **`M0-10` and `M0-11` still open**, both `Ready`.

> ## ⚠ Correction, 2026-08-19 (same day, post-review) — this section's conclusion was wrong
>
> This review closed by recommending the owner **obtain a disposable SQL Server**, calling it
> "the single highest-value action available." **A SQL Server was already installed on the
> development workstation, and had been throughout M0.** Confirmed independently during
> `M2-B07`:
>
> ```
> Get-Service MSSQL*                 -> MSSQL$SQLEXPRESS   Running
> select name from sys.databases     -> NexGenErpDb, NexGenErpDb_Master, MES_Trikala_DB, …
> NexGenErpDb                        -> 197 tables; Users = 1 row, UserRights = 150 rows
> ```
>
> Reached with `Server=.\SQLEXPRESS;Trusted_Connection=True` — Windows integrated auth, no
> credential acquired or reused.
>
> **Why three consecutive sessions concluded "no database exists."** Nothing in the repository
> points at it. Both hosts ship `"MasterDb": ""` (`V.SMART.Web/appsettings.json:10`,
> `V.SMART.Api/appsettings.json:9`), and both user-secrets stores still hold
> `Database=DoesNotExist_M0-03-01-LocalTest`, left behind by `M0-03-01`'s fail-fast test. Each
> session read an empty default, found nothing configured, and **inferred absence from a config
> default** — then recorded that inference as fact, where the next session read it as
> established. The `Unknown` was never entered in `open-questions.md`; it became a `Confirmed`
> by repetition.
>
> **This is the exact failure mode `CLAUDE.md` warns about** — *"Never write an inference so
> that it reads as fact"* — and it cost the milestone its most-cited blocker. The lesson is not
> about SQL Server: **a negative result needs the same `file:line`-grade evidence as a positive
> one.** "I could not find X" is a statement about the search, not about X.
>
> **What actually changes:**
> - **Criterion 1 was never blocked on hardware.** It is blocked on *running the drill*, which
>   is work — and work that can start now. Its deferral rests on a false premise.
> - **`M0-01-03` moves `Needs Review` → `Ready`.** It needs a disposable target, and
>   `MES_Trikala_DB` / a fresh throwaway database on this instance can serve as one.
> - The three behaviours `M0-13` could not verify are now verifiable.
>
> **A new finding this surfaced, unrelated to the drill:** the tenant row in
> `NexGenErpDb_Master.Tenants` stores its connection string **in plaintext, with `sa`
> credentials**. That is a live secret in the database itself, which `M0-04`/`M0-05` do not
> cover — they address the *repository*. Recorded as **Q-32**.

**The single highest-value action available to the owner** is to **run the rebuild drill**
(`M0-01-03`) against a throwaway database on the SQL Server already present. It closes
criterion 1 and settles the three behaviours `M0-13` could not verify. Nothing else on this
list is blocked on so little — and, as the correction above establishes, it is no longer
blocked on hardware at all.
