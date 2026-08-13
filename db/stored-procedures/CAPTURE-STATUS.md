# Capture status — stored procedures (task M0-01-02)

This file records **who captured what, from where, and when** — filled in by the DBA
performing the capture (see `db/RUNBOOK-capture-stored-procedures.md`), then reconciled
by the AI session that runs `db/tools/verify-capture.sh` afterward.

**Never put a connection string, hostname, IP address, username, or password in this
file.** Identify the tenant by name or ID only — see the runbook, §7.

---

## Capture record

| Field | Value |
|---|---|
| Source tenant (name or ID — **never** a connection string) | `NexGenErpDb` — **see provenance caveat below; this is not a simple single-tenant capture** |
| SQL Server instance (hostname/instance label only, no credential) | `DESKTOP-FIIBE97\SQLEXPRESS` |
| Capture date | 2026-08-13 |
| Operator (name of the person who ran the capture) | PavanKunar |
| Tool used | `db/tools/Export-StoredProcedures.ps1` (SQL Server authentication, `-SqlPasswordEnvVar`) |
| Rehearsed against a non-production copy first? | Partial — see provenance caveat. A first dry run against `NexGenErpDb` (2026-08-13, before it held any procedures) served as the tool rehearsal; the real capture ran against the same database after it was populated. No separate non-production *copy* of a populated tenant was used. |

### Provenance caveat — read before trusting "source tenant = NexGenErpDb"

The procedures now captured did **not** originate in `NexGenErpDb`. Sequence of events,
reconstructed from this session:

1. `NexGenErpDb` (local, `DESKTOP-FIIBE97\SQLEXPRESS`) started out empty of `Sp_*`
   procedures — confirmed by the first rehearsal (see the superseded finding below).
2. The operator had a file, `AllSp.sql` (local, outside the repository, not committed),
   which is an SSMS "Generate Scripts" dump headed `USE [IQSMARTDEMO_DB_2025-26]` —
   i.e. it was originally scripted out of a **different** database,
   `IQSMARTDEMO_DB_2025-26`. That database name matches the one referenced in a
   commented-out remote connection string already present in
   `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs` (see that
   file directly for the host — not repeated here; this file must not contain a host
   name or IP literal) — a demo/pre-existing tenant, not a fresh EF-only schema.
3. The operator ran `AllSp.sql` against `NexGenErpDb`, deploying those procedure
   definitions (119 objects total, not just `Sp_*`) into it.
4. `db/tools/Export-StoredProcedures.ps1` was then run against `NexGenErpDb` — which is
   why the query source is `NexGenErpDb`, but the DDL's actual origin is
   `IQSMARTDEMO_DB_2025-26`.

**Consequence for Q-14 (representativeness, owned by M0-02):** this capture reflects one
demo/reference database's procedure set, not a live production tenant's. Whether
`IQSMARTDEMO_DB_2025-26` is representative of an actual production tenant is exactly the
open question M0-02 exists to answer — this record must not be read as "captured from a
live production tenant" without that caveat attached.

**Cross-check performed against the source file (2026-08-13, AI session, read-only text
analysis of the local `AllSp.sql` file — no database connection made):** of the 82
`missing` names in `manifest.csv`, 78 had a matching `CREATE PROCEDURE [dbo].[...]` block
in `AllSp.sql`; 4 did not, under any casing. The live capture run against `NexGenErpDb`
(below) returned exactly the same 78/4 split, corroborating that the "4 not found" result
is a genuine gap in what was deployed, not a capture-tool defect.

---

## Per-procedure outcome

82 rows total = 78 `missing` names in `manifest.csv` (confirmed with
`tail -n +2 db/stored-procedures/manifest.csv | grep -c ",missing,"` → 82) captured
cleanly, 4 not found.

Outcome values: `captured` · `not found` · `permission denied` · `encrypted` ·
`multiple objects matched` · `other (explain)`.

| Procedure name | Outcome | Owner (if not simply captured) | Notes |
|---|---|---|---|
| Sp_AttendanceReport | captured | | |
| Sp_CreditDebitNoteSummaryReport | captured | | Initially failed capture ("unrecognized leading statement") — deployed definition carries a leading `-- ====...` comment above `CREATE PROCEDURE`; fixed in `Export-StoredProcedures.ps1` same day, re-captured cleanly. |
| Sp_GetCreditNoteList | captured | | |
| Sp_GetDebitNoteList | captured | | |
| Sp_GetExportInvoiceStatusList | captured | | |
| Sp_GetHSNSummaryReport | captured | | Same leading-comment shape as Sp_CreditDebitNoteSummaryReport above — fixed, re-captured cleanly. |
| Sp_GetItemModificationReport | captured | | |
| Sp_GetJobOrderAssemblySubAssemblyList | captured | | |
| Sp_GetLabourDcInOutTrack | captured | | |
| Sp_GetLabourDcOutgoingStatusList | captured | | |
| Sp_GetLabourGRNStatusList | captured | | |
| Sp_GetLabourInvStatusList | captured | | |
| Sp_GetLabourSCNStatusList | captured | | |
| Sp_GetManufacturingInvoiceStatusList | captured | | |
| Sp_GetMaterialReq | captured | | |
| Sp_GetMfgPOPendingList | captured | | |
| Sp_GetMfgPosPendingList | captured | | |
| Sp_GetProductionIssueAssyStatusList | captured | | |
| Sp_GetProductionIssueCompStatusList | captured | | |
| Sp_GetProductionReturnAssyStatusList | captured | | |
| Sp_GetProductionReturnCompStatusList | captured | | |
| Sp_GetProductionSCNAssyStatusList | captured | | |
| Sp_GetProductionSCNCompStatusList | captured | | |
| Sp_GetPurchandSubQuotePendingList | captured | | |
| Sp_GetPurchaseGRNsPendingList | captured | | |
| Sp_GetPurchaseInvoiceStatusList | captured | | |
| Sp_GetPurchasePosPendingList | captured | | |
| Sp_GetPurchaseSCNsPendingList | captured | | |
| Sp_GetPurchasesandSubcontractEnquiryPendingList | captured | | |
| Sp_GetSalesandLabQuotePendingList | captured | | |
| Sp_GetSalesandlabourPendingList | captured | | |
| Sp_GetSubContractDcGRNPendingList | captured | | |
| Sp_GetSubContractDcInOutTrack | captured | | |
| Sp_GetSubContractDcoutPendingList | captured | | |
| Sp_GetSubContractInvoicePendingList | captured | | |
| Sp_GetSubContractScnPendingList | captured | | |
| Sp_GetTaxDetailsReport | captured | | |
| Sp_GetToolCribIssueNoteStatusList | captured | | |
| Sp_GetToolCribReturnNoteStatusList | captured | | |
| Sp_ItemWiseHistoryReport | captured | | |
| Sp_JobOrderTrack | captured | | |
| Sp_LabourPendingReport | captured | | |
| Sp_Labour_Track | captured | | |
| Sp_Print_AppointmentLetter | captured | | |
| Sp_Print_CREDITNOTE | captured | | |
| Sp_Print_DebitNote | captured | | |
| Sp_Print_EnquiryFeasibility | captured | | |
| Sp_Print_EnquirySales | captured | | |
| Sp_Print_InterStoreTransfer | captured | | |
| Sp_Print_JobOrder | captured | | |
| Sp_Print_LeaveApplication | captured | | |
| Sp_Print_MaterialIssueNoteReduction | captured | | |
| Sp_Print_MaterialReq | captured | | |
| Sp_Print_OfferLetter | captured | | |
| Sp_Print_Payments | captured | | |
| Sp_Print_ProdAssyGRN | captured | | |
| Sp_Print_ProdAssySCN | captured | | |
| Sp_Print_ProdCompSCN | captured | | |
| Sp_Print_ProductionCompGRN | captured | | |
| Sp_Print_ProductionIssueAss | captured | | |
| Sp_Print_ProductionIssueComp | captured | | |
| Sp_Print_PurchaseEnquiry | captured | | |
| Sp_Print_PurchaseGRN | captured | | |
| Sp_Print_PurchaseInvoice | captured | | |
| Sp_Print_PurchasePo | captured | | |
| Sp_Print_PurchaseSCN | captured | | |
| Sp_Print_RouteCard | captured | | |
| Sp_Print_Salary | captured | | |
| Sp_Print_SubconSCN | captured | | |
| Sp_Print_ToolCribIssueNote | captured | | |
| Sp_Print_ToolCribReturn | captured | | |
| Sp_PurchaseSalesTrack | captured | | |
| Sp_RouteCardAnalysisDetails | captured | | |
| Sp_RouteCardAnalysisSummary | captured | | |
| Sp_StockAnalysis | captured | | |
| Sp_StockLedger | captured | | |
| Sp_TDSReport | captured | | Same leading-comment shape as Sp_CreditDebitNoteSummaryReport above — fixed, re-captured cleanly. |
| Sp_VendorPRRating | captured | | |
| Sp_BomAnalysis | not found | **Escalated — needs a human decision, see Findings below** | Not present in `NexGenErpDb` under any schema; not present in the source `AllSp.sql` text either (cross-checked independently). |
| Sp_Print_Estimation | not found | **Escalated — needs a human decision, see Findings below** | Same as above. |
| Sp_Print_Receipts | not found | **Escalated — needs a human decision, see Findings below** | Same as above. |
| Sp_Print_SingleProcessInspection | not found | **Escalated — needs a human decision, see Findings below** | Same as above. |

---

## Findings requiring a human decision

Anything found during capture that is **not** a simple "captured successfully" goes
here, in addition to the per-procedure table above — especially:

- A procedure genuinely absent from the source tenant. This is one of the two most
  valuable outcomes of the whole task: either the name is dead code that should be
  removed from the application, or it is a real latent defect (a screen that will throw
  the moment a user opens it). Both need a human decision — record it here, don't
  resolve it yourself.
- A procedure found under a schema other than `dbo`. The application always executes
  `dbo.{procedureName}`
  (`V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs:25,27,35-39,105-107`),
  so this is very likely a live defect, not a false alarm.
- A procedure that exists under more than one schema (ambiguous — do not guess which
  one the application means).
- Anything about a procedure's body that looked like a bug, a dead branch, or an
  apparent duplicate of another procedure. The capture step must not fix or merge
  these — only record them.

| Finding | Procedure(s) | Recorded by | Date | Status |
|---|---|---|---|---|
| **SUPERSEDED 2026-08-13 (same day).** *Original finding: no populated tenant database was known to be accessible; a rehearsal against `NexGenErpDb` found it had zero `Sp_*` procedures deployed.* This is no longer the blocker — the operator subsequently deployed a procedure set (originally scripted from `IQSMARTDEMO_DB_2025-26`, via a local file `AllSp.sql`) into `NexGenErpDb`, and a real capture ran successfully against it. Kept here for the audit trail; see the row below for the current state. | N/A (historical) | AI session (M0-01-02) | 2026-08-13 | Superseded — see below |
| **4 of 82 `missing` procedures are genuinely absent from the source.** `Sp_BomAnalysis`, `Sp_Print_Estimation`, `Sp_Print_Receipts`, `Sp_Print_SingleProcessInspection` are referenced from C#/Razor (per `manifest.csv`, `stored-procedure-inventory.md`) but do not exist under any schema in `NexGenErpDb`, and were independently confirmed absent from the raw `AllSp.sql` source text as well (not just a live-query miss). Per this task's own framing, this is one of the two most valuable possible outcomes: either these 4 names are dead code that should be removed from the calling C#/Razor, or they are real latent defects — a screen or report that throws the moment a user reaches it. **This is not resolved here** — a human must decide, per procedure, which it is. Cross-reference each name against its calling code (`stored-procedure-inventory.md` / `manifest.csv` gives the call site) before deciding. | Sp_BomAnalysis, Sp_Print_Estimation, Sp_Print_Receipts, Sp_Print_SingleProcessInspection | AI session (M0-01-02), corroborated by operator PavanKunar's capture run | 2026-08-13 | **Escalated — needs a human decision** (dead code vs. latent defect, per procedure) |
| **Source tenant provenance is not a clean single-tenant capture.** The captured DDL's actual origin is `IQSMARTDEMO_DB_2025-26` (a demo/reference database reachable via a remote connection string already commented out in `ApplicationDbContextFactory.cs`), manually relayed through a local `NexGenErpDb` copy rather than captured directly from a nominated production tenant. See "Provenance caveat" above. This directly feeds Q-14 (open-questions.md, owned by M0-02): the representativeness of this capture is now specifically the representativeness of `IQSMARTDEMO_DB_2025-26`, a demo database, not of any customer's live production tenant. | All 78 captured procedures | AI session (M0-01-02) | 2026-08-13 | **Flagged for M0-02** — not this task's question to answer, but must not be lost when Q-14 is picked up |
| **`db/tools/Export-StoredProcedures.ps1` had a real tooling bug, now fixed.** 3 of the 78 successfully-captured procedures (`Sp_CreditDebitNoteSummaryReport`, `Sp_GetHSNSummaryReport`, `Sp_TDSReport`) initially failed with "unrecognized leading statement" on the first real-capture run. Root cause: their deployed definitions carry a leading `-- ====...` divider comment directly above `CREATE PROCEDURE`, in the same batch — legitimate, faithful `OBJECT_DEFINITION()` output, not corruption — but the script's detection/replacement regex only recognized `CREATE`/`ALTER` immediately after whitespace, not after a leading comment, even though `verify-capture.sh`'s own spec already permits a leading comment ("first *non-comment* statement"). Fixed in the script (comment-tolerant regex, only the `CREATE`/`ALTER` keyword pair is ever rewritten); re-run captured all 3 cleanly. No procedure body was altered by hand at any point — see git history for the fix. | Sp_CreditDebitNoteSummaryReport, Sp_GetHSNSummaryReport, Sp_TDSReport | AI session (M0-01-02) | 2026-08-13 | Resolved — tooling fixed, re-captured cleanly |
| **`Sp_Print_CompanyDetails` is out of this task's scope, despite an acceptance-criteria bullet implying otherwise.** `manifest.csv` classifies it `scripted` (already in `Existing Store Procedures/StoredProcedures/`, referenced at `ReportService.cs:200`), not `missing` — so it was never part of this task's worklist, and was correctly not captured into `db/stored-procedures/`. Moving it there is M0-01-03's job (relocating the 13 already-scripted files), not this task's. The manifest is the authoritative classification per this task's own rules; flagging the conflict rather than silently either violating the "don't touch the 13 existing files / M0-01-03 relocates them" constraint or silently ignoring the acceptance criterion. | Sp_Print_CompanyDetails | AI session (M0-01-02) | 2026-08-13 | Informational — no action needed here; confirm M0-01-03 covers it |

---

## Verification

Run from the repository root once files are delivered:

```bash
bash db/tools/verify-capture.sh
```

**Actually run** 2026-08-13, after the fixed-tool re-capture, from the repository root.
Real captured output below (section 2's 78 identical `ok:` blocks, one per file for all
nine per-file checks, omitted here for readability — every one passed; re-run the command
above to see them in full):

```
==================================================================
 verify-capture.sh -- db/stored-procedures/ vs db/stored-procedures/manifest.csv
==================================================================

-- 1. Manifest reconciliation --
  'missing' rows in manifest: 82
    WARN: Sp_BomAnalysis.sql absent, but 'Sp_BomAnalysis' appears in CAPTURE-STATUS.md -- treated as a recorded, owned exception, not a hard failure. Confirm it actually has an owner and reason, not just a passing text match.
    WARN: Sp_Print_Estimation.sql absent, but 'Sp_Print_Estimation' appears in CAPTURE-STATUS.md -- treated as a recorded, owned exception, not a hard failure. Confirm it actually has an owner and reason, not just a passing text match.
    WARN: Sp_Print_Receipts.sql absent, but 'Sp_Print_Receipts' appears in CAPTURE-STATUS.md -- treated as a recorded, owned exception, not a hard failure. Confirm it actually has an owner and reason, not just a passing text match.
    WARN: Sp_Print_SingleProcessInspection.sql absent, but 'Sp_Print_SingleProcessInspection' appears in CAPTURE-STATUS.md -- treated as a recorded, owned exception, not a hard failure. Confirm it actually has an owner and reason, not just a passing text match.

-- 2. Per-file checks --
  .sql files present in db/stored-procedures: 78
  [78 files individually checked: no BOM, LF line endings, no connection-string-shaped
   text, no IPv4-literal-shaped text, no USE statement, no known tenant-name token,
   first statement is CREATE OR ALTER PROCEDURE, declared name matches file name --
   all 78 passed every check, zero per-file failures]

-- 3. Arithmetic --
  manifest 'missing' rows           : 82
  .sql files present                : 78
  of which not an authorized 'missing' name (extras/unauthorized): 0
  README permits zero extras -- any nonzero value above is also counted as a FAIL in section 2.
  'missing' rows with a file actually present: 78 / 82

==================================================================
 RESULT
==================================================================
  hard failures : 0
  warnings      : 4
  verify-capture.sh: PASS
```
