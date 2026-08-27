---
doc_id: KB-115
title: Owner Action List — what only Vivek can do, and what each unblocks
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-25
dependencies: [KB-081, KB-080, KB-004, KB-060]
---

# Owner Action List

> **Every item here is blocked on a person, not on execution capacity.** Three independent Select
> passes on 2026-08-25 confirmed no task is dependency-ready. [KB-081](task-tracker.md) is the
> status authority; this list goes stale and should be re-derived from it, not trusted.
>
> Ordered by **leverage ÷ cost**. Items 1–3 take minutes and need no production access.

---

## 1. Mark the permission-matrix job a required status check — *2 minutes, no access needed*

**Where:** GitHub → `ErpStore/NexERP_B` → Settings → Branches → branch protection for `master` →
**Require status checks to pass before merging** → add **`Test - V.SMART.Api.Tests`**.

**Why it is this small:** protection already exists on `master` — the 2026-08-25 push returned
`Bypassed rule violations … Changes must be made through a pull request`. So this is *adding a
check to an existing ruleset*, not creating protection. `M2-A03` diagnosed it as unreachable
because no `gh` CLI is available here, which was right about the tooling and wrong about the
ruleset.

**The job already runs.** `ci.yml`'s `Test - V.SMART.Api.Tests` step executes the harness with an
explicit `$LASTEXITCODE` check, so a permission regression fails the build **today**. Only the
*required* flag is missing.

**Unblocks:** closes the remaining half of **G2 criterion 3**; `M2-A03` moves `Needs Review` →
`Completed`.

---

## 2. Answer Q-82 — should execution sessions open pull requests? — *a decision, no access needed*

**The situation:** `master` requires PRs. This run merged **~25 branches** straight to local
`master` and pushed the result, bypassing that rule each time with your authority. Nothing was
done without permission, but the repository's stated policy and its actual operation disagree.

**Two coherent answers, either fine:**

- **PRs are the intended workflow** → [KB-088](workflow.md)'s Git Strategy needs rewriting, and
  execution sessions should push branches and open PRs rather than merging locally.
- **The rule is vestigial** for a single-owner repository → relax it so practice and policy agree.

**Why it matters now rather than later:** every future task inherits whichever answer you give,
and the current state trains sessions to bypass a control.

---

## 3. Rule on M2-B03's "two independent users" — *a judgement, no access needed*

[KB-080 §9](README.md)'s Definition of Done: *"the controller template demonstrably followed by
both existing controllers — the template is only real once it has two independent users."*

**Where it stands:** the template is [KB-114](../api/controller-conventions.md), merged.
`M2-B10` then brought `CurrencyController` and `AuthController` into conformance on
`[ProducesResponseType]` — 45 attributes added. `AuthController` carries a written exception for
five checklist items ([KB-114](../api/controller-conventions.md) §13.2).

**The question:** do two controllers, one with a written exception, satisfy "two independent
users"? **Unblocks:** the remaining half of **G2 criterion 6**.

---

## 4. Run M0-04's rotation — *needs production SQL + GST gateway access*

**This is the whole remaining critical path.** Nothing downstream moves without it:

```
M0-04  →  M2-A04  →  M2-A05  →  M2-C02  →  M2-C03  →  the M2-C tree
                                              →  M2-C05-03  →  M2-D01  →  G2 criteria 1 and 5
```

**Read first:** `docs/runbooks/credential-rotation.md` — **being produced now by `M0-04`**, which
was unblocked on 2026-08-25 once it became clear its deliverables are documents, not the rotation
(tracker footnote ⁷⁰). Wait for it; it exists to make this a rehearsed job rather than
improvisation against live tenant databases.

**The step most likely to be forgotten**, quoted from `M0-04`'s own spec: *update the `Tenants`
table rows whose `ConnectionString` embeds the old login — "this is the step most likely to be
forgotten and it is the one that takes every tenant down."* Those connection strings live **in the
master database, not in any file**, so no repository grep will find them.

**Order that preserves rollback:**

1. Create a **new least-privilege** SQL login *alongside* `sa` — do not reuse `sa` (R-01's action
   item is explicit).
2. Deploy the new value to every consumer.
3. **Update the `Tenants` rows.**
4. Verify the application serves every tenant on the new login.
5. Only then **disable** the old login — disable, not drop, so step 6 works.
6. Rollback if needed: re-enable the old login; old connection strings still work.

Then `Jwt:Secret` (C-4): ≥32 bytes from a cryptographic source. **Every outstanding token is
invalidated** — expect all users to be signed out. And the GST gateway (C-5) through the
provider's own reset process; a failed rotation **blocks statutory e-Invoice generation**.

**Unblocks:** `M2-A04` and the entire chain above, plus **G0 criteria 2 and 3**.

**Also closes:** R-01 and R-02, but *only* when the human verification checklist in the runbook is
signed and dated. Writing the runbook does not close them.

---

## 5. Give M2-C10 a reachable database, or relax its criterion — *access, or a decision*

`M2-C10` (decimal handling — no float money arithmetic) is `Blocked`, category `environment`. Its
binding criterion requires **INV-032 recorded with the MEASURED wire format** of a decimal over
HTTP. The only decimal-bearing endpoint is `[Authorize]`d and this workstation has
`ConnectionStrings:MasterDb` and `Jwt:Secret` both **empty**, so no live response can be captured.

**Either** provide a reachable database and credential, **or** relax the criterion to a
static-analysis proof. No retry can clear it. **Unblocks:** `M2-C10`, then `M2-C07`.

---

## 6. Answer Q-38 — what is M2-C11 *for*? — *a decision, no access needed*

`M2-C11` was *"adopt the Angular pilot as the app baseline"*. `M2-C01` has since **built** the
Angular workspace it existed to adopt, so ADR-007 inverted the relationship between pilot and app.
`M2-C12-02` deliberately did not re-specify it — it replaced the supersession banner with a
`BLOCKED ON A HUMAN DECISION` banner and routed it to you rather than inferring an answer.

---

## 7. Decide the fate of `stash@{0}` — *30 seconds, recommend discard*

Orphaned work from a dead runner (`wf_48667c87-4d9`, 2026-08-23): **16 files, zero commits**,
including two **out of scope** for its own task — an empty `try {} catch` writing a "Login failed"
log into `AuthController.cs`, and `.sln` rewritten to drop the `V.SMART` project (the documented
Visual Studio trap).

**Recommend discard.** `M2-C04-01` has since been implemented, validated and merged independently,
so nothing in the stash is needed.

```bash
git stash drop stash@{0}        # to discard
git stash show -p stash@{0}     # to inspect first
```

---

## Not on this list, deliberately

- **`M0-06`** — `Ready` and P1, but `migration/M0-06-remove-default-admin` already exists.
  Selection part 5 excludes it; it needs no decision, only for someone to finish that branch.
- **`M0-11`** — a `Product Decision` (silent FIFO under-issue, Q-01). Owner-only and genuinely
  yours, but it blocks nothing currently selectable, so it is not on the critical path.
- **`M2-B12-01`** — `Blocked`, verdict `FAIL`, escalation budget exhausted. Needs re-specification,
  not a decision.
- **`migration/M2-A08-row-level-scoping`** — duplicate of the merged `M2-A08`, functionally
  identical `UserRepository.cs` change, no validated `PASS`. Safe to delete whenever.
