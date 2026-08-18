---
doc_id: KB-103
title: Stored-Procedure Drift Across Tenant Databases (Q-14)
module: architecture
source_files:
  - db/tools/list-deployed-procedures.sql
  - db/tools/compare-tenant-fingerprints.sh
  - db/RUNBOOK-tenant-drift-check.md
  - db/drift/
  - db/stored-procedures/CAPTURE-STATUS.md
  - db/stored-procedures/manifest.csv
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs
  - V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs
  - V.SMART/V.SMART.Shared/wwwroot/templates/
entities: []
api_endpoints: []
database_tables: [Tenants]
business_rules: []
status: partial
confidence: mixed
last_verified: 2026-08-18
dependencies: [KB-012, KB-014, KB-060, ADR-005]
---

# Stored-Procedure Drift Across Tenant Databases (Q-14)

**Investigation INV-030 — status `Partial`.** Produced by task **M0-02**.

> **The question is not yet answered.** This document records the *method*, the *tooling* and
> the *blocker*. The comparison itself has not run, because no per-tenant fingerprints have
> been delivered. **Nothing here may be read as evidence that tenants do or do not differ.**

---

## 1. The question and why it matters

**Q-14** (`docs/kb/open-questions.md`): *do the stored procedures differ between tenant
databases?*

Multi-tenancy is **database per tenant** (Confirmed, [KB-012](database-architecture.md)):
no `TenantId` discriminator, no EF global query filter, isolation by connection string. So
M0-01-02 could only capture stored-procedure DDL from **one** database
(`db/stored-procedures/`, 78 files, INV-027). If tenants have diverged:

- `db/stored-procedures/` describes exactly one database and silently misdescribes the rest;
- M0-01-03's deployment script (`db/deploy-stored-procedures.ps1`) would **overwrite** other
  tenants' procedures with the source's — changing report figures and statutory document
  content, with no test to catch it (no test project exists; INV-023, Confirmed);
- the G0 criterion "stored procedures are under version control" would hold for one tenant
  only, which is not what it means.

**Why drift is plausible rather than paranoid (Inferred, not Confirmed).** Per-tenant
customisation is already an established pattern in this product:
`V.SMART/V.SMART.Shared/wwwroot/templates/` carries four per-tenant FastReport template
folders beside `default` (`acucom…`, `sharadaelectrou1…`, `sns…`, `srinuenggind…`), and
`ReportService` injects the tenant's own connection string into the loaded report
(`V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:71`, per
[ADR-005](../decisions/ADR-005-reporting-and-printing.md) and INV-009). A system that
customises report *presentation* per tenant is a system where the report *query* may also have
been customised. That is an argument for checking, not a finding.

**A second reason the baseline's representativeness is in doubt (Confirmed).**
`db/stored-procedures/CAPTURE-STATUS.md` ("Provenance caveat") records that the captured DDL
did not originate in a production tenant at all: it came from a demo database
`IQSMARTDEMO_DB_2025-26`, relayed through a local database `NexGenErpDb`, from which the
export was actually taken. So the baseline is a demo database's procedure set. Whether it
matches any live tenant is exactly what this investigation must measure.

## 2. Method — binding

Fingerprint, not full DDL. Exporting every procedure body from every tenant would multiply the
capture problem and push a large volume of business logic through a manual export path for no
gain.

Per procedure, per tenant (`db/tools/list-deployed-procedures.sql`, *Query B*,
`FINGERPRINT_QUERY_VERSION 2`):

| Field | Why |
|---|---|
| `schema_name` | The application always calls `EXEC dbo.{procedureName}` (`…/TrackReportService/ReportExecutor.cs:27`, Confirmed), so a non-`dbo` procedure is a finding in itself |
| `procedure_name` | The join key |
| `create_date`, `modify_date` | Cheap and high-signal: a differing `modify_date` across tenants indicates drift even when hashes match |
| `definition_length` | Coarse comparison; also catches truncation in the export itself |
| `hash_raw` | `HASHBYTES('SHA2_256', OBJECT_DEFINITION(object_id))`, verbatim |
| `hash_normalised` | Same, over the definition with line endings normalised, whitespace runs collapsed to one space, outer whitespace trimmed, and the text case-folded |

**The two hashes are the whole design.** `hash_raw` differing while `hash_normalised` matches
means the drift is **cosmetic** — reformatting, a comment, different casing — and the single
artefact set stands. Both differing means the drift is **potentially behavioural** and is
escalated. Collapsing this to one hash would report every reformatting as behavioural
divergence and bury the real signal in the noise.

**Stated honestly: a `hash_normalised` match is `Inferred` equivalence, not Confirmed.**
Whitespace collapsing and case folding cannot distinguish a semantic difference inside a
string literal (`'A  B'` vs `'A B'`, `'Yes'` vs `'YES'`) from formatting. Every `cosmetic`
classification in this document therefore carries confidence **Inferred**, and the residual
risk is real, not rhetorical.

Further limits, recorded rather than hidden:

- `HASHBYTES` accepts inputs over 8000 bytes only on SQL Server 2016 (13.x) and later; on an
  older engine the hashes return `NULL` for long procedures. The comparison script rejects
  `NULL` hashes as a **collection failure** — never as "no drift".
- `LEN()` ignores trailing whitespace, so `definition_length` is a coarse signal only.
- The normalisation uses `CHAR(1)`/`CHAR(2)` as collapse sentinels; a body containing either
  byte literally would normalise incorrectly. Vanishingly unlikely in T-SQL source.

### Classification, per procedure name

| Class | Definition | Consequence |
|---|---|---|
| `identical` | `hash_raw` equal across every tenant | None |
| `cosmetic` | `hash_raw` differs, `hash_normalised` equal | Record; single artefact set stands (Inferred) |
| `divergent` | `hash_normalised` differs | **Escalate** — per-procedure human decision |
| `missing_in_tenant` | Present in some tenants, absent in others | **Escalate** — a report that works for one customer and throws for another |
| `extra_in_tenant` | Present in a tenant, absent from `manifest.csv` | Record; likely a customisation or dead object. **Do not delete.** |

## 3. Tooling delivered (2026-08-17, M0-02)

| Artefact | Role |
|---|---|
| `db/tools/list-deployed-procedures.sql` *(extended)* | *Query B* added — `FINGERPRINT_QUERY_VERSION 2`, emitting the seven fingerprint columns including **both** hashes. Query A (M0-01-02's human-readable listing) is unchanged and explicitly **not** the drift export |
| `db/tools/compare-tenant-fingerprints.sh` *(new)* | Reads every `db/drift/*.csv`, joins on `procedure_name`, classifies, prints per-class counts, the arithmetic, and the `divergent` / `missing_in_tenant` / `extra_in_tenant` / `cosmetic` lists. No database. Aborts loudly on a header mismatch, a malformed row, a `NULL` hash or a duplicate row |
| `db/RUNBOOK-tenant-drift-check.md` *(new)* | DBA-facing procedure: tenant selection, the query, export, naming, the no-secrets rule, the self-test, the handback checklist |
| `db/drift/README.md` *(new)* | CSV schema and naming convention, plus the standing no-secrets rule |

**Why the query had to be extended (Confirmed, 2026-08-17).** Before this task,
`list-deployed-procedures.sql` emitted a single `DefinitionSha256Hex` computed over the
definition with only `CR` and `TAB` stripped (`db/tools/list-deployed-procedures.sql`, M0-01-02
version, lines 39–49). That is neither `hash_raw` (it is not verbatim) nor `hash_normalised`
(no whitespace-run collapsing, no case folding), so it could not serve as either half of the
two-hash design. **Any tenant fingerprinted with the pre-M0-02 query must be re-collected** —
mixing query versions silently invalidates the comparison, which is why the comparison script
aborts on a non-conforming header.

### Harness verification actually performed (2026-08-17, no database)

Run against synthetic fixtures in a scratch directory outside the repository — **fixtures, not
tenant data; no fingerprint CSV was fabricated in `db/drift/`**:

| Test | Observed result |
|---|---|
| Two well-formed fixtures with one of each class | Correct counts (1 identical, 1 cosmetic, 1 divergent, 1 missing_in_tenant, 1 extra_in_tenant); arithmetic `1+1+1+1+1 = 5 = distinct names`; manifest coverage `4 classified + 91 absent = 95 = manifest rows`; exit 4 |
| Deliberately deformed header (the old M0-01-02 column names) | `FAIL: … header does not match FINGERPRINT_QUERY_VERSION 2`, aborted, exit 2 |
| Row with an unquoted comma (8 fields) and a row with `NULL` hashes | Both reported as structural failures, aborted, exit 2 |
| One tenant CSV only | "NOT ENOUGH INPUT … Q-14 stays UNDECIDED", exit 3 |
| Empty directory | "no fingerprints have been delivered … absence of fingerprints is not evidence of absence of drift", exit 3 |
| Same inputs run twice | Byte-identical output (`diff` empty) |
| Two identical fixtures | "NO DRIFT across 2 tenants", exit 0 |

## 4. Result — **not available**

| | |
|---|---|
| Tenants fingerprinted | **0** (`db/drift/` contains no CSV as of 2026-08-17) |
| Classification | TBD |
| `identical` / `cosmetic` / `divergent` / `missing_in_tenant` / `extra_in_tenant` | TBD / TBD / TBD / TBD / TBD |
| Arithmetic closure | TBD |
| Divergent procedures, by name | TBD |
| Answer to Q-14 | **Unknown — undecided, not "no drift"**. **Explicitly deferred 2026-08-18 by Vivek** — see the deferral decision below |

**Blocker.** A DBA with `VIEW DEFINITION` on **at least two** tenant databases, plus a working
tenant list. Neither is obtainable by an AI session: this repository contains no tenant
inventory (**Q-12**, owned by ops, unanswered until M6-03), and the task forbids acquiring or
reusing any database credential.

**Owner.** DBA (the same role that owns Q-14 in `docs/kb/open-questions.md`; the M0-01-02
capture was performed by operator **PavanKunar**, per `db/stored-procedures/CAPTURE-STATUS.md`,
who is the obvious first candidate), with the migration lead to resolve which database the
"baseline" label refers to given the provenance caveat in §1.

**Consequence of leaving it here (record this, do not soften it).** Until fingerprints land,
`db/stored-procedures/` is a single artefact set **by assumption, not by evidence**, and
`db/deploy-stored-procedures.ps1` has no per-tenant path. Any later per-tenant surprise — a
report that renders different figures after a deployment, or a procedure that vanishes from
one customer — traces back to this gap.

### Deferral decision — 2026-08-18

**Q-14 is explicitly deferred. Named owner: Vivek** (repository owner / migration lead).

This closes M0-02 via the second of the two paths [KB-080 §7](../execution/README.md) allows —
*"Q-14 answered **or explicitly deferred with reason"*. It does **not** close the question, and
§4's `TBD` rows above stay `TBD` **by design**: filling them in from zero fingerprints would be
the exact error this document exists to prevent.

| | |
|---|---|
| Decision | Defer Q-14; do not schedule DBA time now |
| Decided by | **Vivek**, 2026-08-18 |
| Reason | No DBA with `VIEW DEFINITION` on ≥2 tenant databases is scheduled; a session may not acquire or reuse a credential (R-01) |
| Evidence state at deferral | `db/drift/` holds **zero** fingerprint CSVs — **zero tenants compared** (Confirmed, re-verified 2026-08-18) |
| What this is **not** | It is **not** a finding of "no drift". Drift remains **undecided** |
| Risk accepted | The consequence recorded immediately above, in full — accepted knowingly, not overlooked |
| Reopen trigger | Any CSV landing in `db/drift/`, **or** any per-tenant report / statutory-document surprise in the field |
| On reopen | Resume at [`tasks/M0-02.md`](../execution/tasks/M0-02.md) Implementation Steps §9. **Do not re-derive the tooling** — it is complete and verified |
| Still owed by Vivek on reopen | Which database the "baseline" label denotes, given §1's provenance caveat — ideally fingerprint both, as `baseline-demo-origin` and `baseline-relay` |

## 5. What happens when the fingerprints land

1. Run `bash db/tools/compare-tenant-fingerprints.sh` from the repository root and paste the
   output into the M0-02 ticket.
2. Fill §4 above with the counts, the tenant labels (**labels only — never a connection
   string**) and every `divergent` procedure by name.
3. Then:
   - **No drift, or cosmetic only** → answer Q-14 in `docs/kb/open-questions.md` with the
     evidence, close INV-030, and record explicitly that `db/stored-procedures/` stands as a
     single artefact set (a negative result worth recording, so nobody pays for this check
     twice). Cosmetic-only additionally requires the normalisation rule and the residual
     `Inferred` risk to be written down.
   - **Divergent, missing or extra** → present the options below **without choosing**,
     escalate to a named product owner and DBA, record Q-14 as *answered: drift exists* with
     the consequential decision left open and owned, and raise a finding against **M0-01-03**
     that its deployment script needs a per-tenant path. Add the per-tenant caveat to **R-04**
     in `docs/kb/risks/technical-debt-register.md`.

### Decision framework, if drift is found — presented, never decided by a session

**Option A — per-tenant variants directory.**
`db/stored-procedures/tenants/<tenant-label>/<ProcedureName>.sql` overriding the shared
baseline, with the deployment script taking a tenant parameter and applying
`base + tenant override`. Preserves every tenant's current behaviour exactly. Cost: N artefact
sets to maintain, and the divergence becomes permanent and blessed. **Recommended default when
the divergent count is small and the customisations look deliberate** — it is the only option
that changes nobody's behaviour, and M0 is a stabilisation milestone, not a consolidation one.

**Option B — reconcile to one canonical version.**
Choose one definition per procedure and deploy it everywhere. Cheapest long-term, and it
**changes ERP behaviour for every tenant that loses its variant** — silently, in report figures
and statutory documents, with no test to catch it. A **product-owner decision**, escalated per
procedure with the diff attached. Never taken by a session, never presented as a cleanup.

**Option C — declare the drift cosmetic and proceed.**
Valid only when *every* divergence classifies `cosmetic`. Record the normalisation rule, the
evidence, and the residual `Inferred` risk (§2).

**No variants directory is implemented by M0-02 under any outcome** — that is a change to
M0-01-03's deliverable, not to this investigation.

## 6. Evidence index

| Claim | Confidence | Evidence |
|---|---|---|
| Database per tenant; isolation by connection string; no `TenantId` discriminator | Confirmed | [KB-012](database-architecture.md), [KB-014](multi-tenancy.md), INV-003/INV-005 |
| Procedures are invoked as `EXEC dbo.{procedureName}` | Confirmed | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs:27` |
| Per-tenant report templates exist (4 + `default`) | Confirmed | `V.SMART/V.SMART.Shared/wwwroot/templates/`, INV-009, [ADR-005](../decisions/ADR-005-reporting-and-printing.md) |
| Per-tenant *procedure* customisation follows from per-tenant *template* customisation | **Inferred** | Reasoning only — this is the hypothesis under test, not a finding |
| The captured baseline came from a demo database via a relay, not a production tenant | Confirmed | `db/stored-procedures/CAPTURE-STATUS.md`, "Provenance caveat" |
| The pre-M0-02 fingerprint query emitted neither `hash_raw` nor `hash_normalised` | Confirmed | `db/tools/list-deployed-procedures.sql` (M0-01-02 version), single `DefinitionSha256Hex` over a CR/TAB-stripped definition |
| No per-tenant fingerprints exist | Confirmed (negative result) | `db/drift/` contains only `README.md` as of 2026-08-17 |
| Whether tenant stored procedures actually differ | **Unknown** | Not measurable without DBA access to ≥2 tenant databases |
